using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilityGoal : AIGoal
{
    /// <summary>
    /// The maximum amount of angle deviation from the ground slope 
    /// allowed before stability becomes a max priority goal of the vehicle.
    /// </summary>
    public float MaxDeviation = 12.5f;


    /// <summary>
    /// The length of the area that will be sampled underneath the 
    /// centre of mass to determine the slope of the ground.
    /// </summary>
    public float SampleArea = 2.0f;


    /// <summary>
    /// An offset from the centre of mass of the vehicle, 
    /// which defines where the sample area will begin. 
    /// A positive offset means the vehicle will consider
    /// the ground ahead of its path. 
    /// </summary>
    public float SampleOffset = 2.0f;


    /// <summary>
    /// Smooths out ground sampling to help filter out noise or
    /// transient changes in ground slope (e.g. the edge of a cliff)
    /// </summary>
    public bool SampleSmoothing = true;

    
    /// <summary>
    /// The smoothing factor (0,1) to use when sample smoothing is turned on.
    /// </summary>
    public float SmoothingFactor = 0.03f;

    private float _groundSlope = 0.0f;

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
        var step = new Vector2(SampleArea * 0.5f, 0);
        var offset = new Vector2(SampleOffset, 0);

        // Sample the ground
        var leftRay = Physics2D.Raycast(CentreOfMass + offset - step, Vector2.down, _maxRayDistance, _mapMask);
        var rightRay = Physics2D.Raycast(CentreOfMass + offset + step, Vector2.down, _maxRayDistance, _mapMask);

        if (leftRay.collider != null && rightRay.collider != null)
        {
            // Determine the slope of the ground

            var sampleVector = rightRay.point - leftRay.point;
            var sampleSlope = Mathf.Rad2Deg * Mathf.Atan(sampleVector.y / sampleVector.x);

            if(SampleSmoothing)
                // Apply smoothing through an exponential moving average approximation
                _groundSlope = SmoothingFactor * sampleSlope + (1.0f - SmoothingFactor) * _groundSlope;
            else
                _groundSlope = sampleSlope;


            _targetAngularAcceleration = _groundSlope - Rigidbody.rotation;
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


    void OnValidate()
    {
        SmoothingFactor = Mathf.Clamp01(SmoothingFactor);
    }
}