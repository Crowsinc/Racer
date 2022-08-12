using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilityGoal : AIGoal
{
    /// <summary>
    /// The length of area samples to determine the slope of the ground
    /// </summary>
    public float SampleLength = 2.0f;

    /// <summary>
    /// The maximum amount of angle deviation from the ground slope 
    /// allowed before stability becomes a max priority goal of the vehicle.
    /// </summary>
    public float MaxDeviation = 12.5f;

    private int _mapMask = 0;
    private int _maxRayDistance = 1000;
    
    private float _targetAngularAcceleration;
    private List<ActuatorModule> _rankedActuators;

    void Awake()
    {
        _mapMask = LayerMask.GetMask("Default");
    }


    public override void Begin()
    {
        // Sort actuators from best to worst in terms of their angular effects. 
        _rankedActuators = new List<ActuatorModule>(Actuators);
        _rankedActuators.Sort(
            (a, b) => Mathf.Abs(b.AngularAcceleration).CompareTo(Mathf.Abs(a.AngularAcceleration))
        );
    }


    public override float Plan()
    {
        var ray = Physics2D.Raycast(CentreOfMass, Vector2.down, _maxRayDistance, _mapMask);
        if (ray.collider != null)
        {
            var step = new Vector2(0.5f * SampleLength, 0.0f);

            var left = ray.collider.ClosestPoint(ray.point - step);
            var right = ray.collider.ClosestPoint(ray.point + step);

            var groundVector = right - left;
            var groundSlope = Mathf.Rad2Deg * Mathf.Atan(groundVector.y / groundVector.x);

            _targetAngularAcceleration = groundSlope - Rigidbody.rotation;
        }
        else _targetAngularAcceleration = -Rigidbody.rotation;

        return Mathf.Clamp01(Mathf.Abs(_targetAngularAcceleration) / MaxDeviation);
    }


    public override List<Tuple<ActuatorModule, float>> GenerateActions()
    {
        var actions = new List<Tuple<ActuatorModule, float>>();

        // We choose actions as a linear combination of actuator angular accelerations.
        // The actuators are ordered from best to worst, so the algorithm will greedily
        // pick the most effective choice of actuators.
        foreach(var actuator in _rankedActuators)
        {
            float requiredProportion = _targetAngularAcceleration / actuator.AngularAcceleration;
            if(requiredProportion > 0.0f) // Negative proportion => wrong direction
            {
                requiredProportion = Mathf.Clamp01(requiredProportion);
                _targetAngularAcceleration -= requiredProportion * actuator.AngularAcceleration;

                actions.Add(new Tuple<ActuatorModule, float>(actuator, requiredProportion));
            }
        }

        return actions;
    }

}