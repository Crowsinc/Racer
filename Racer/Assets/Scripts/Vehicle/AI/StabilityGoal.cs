using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilityGoal : AIGoal
{
    // This goal is solely for preventing flipping and keeping the vehicle stable.


    /// <summary>
    /// The maximum amount of angle deviation from the ground slope 
    /// allowed before stability becomes a max priority goal of the vehicle.
    /// </summary>
    public float MaxDeviation = 15.0f;


    /// <summary>
    /// The number of seconds through out which corrective actions will be distributed.
    /// As the reaction time approaches zero, the required acceleration of each thruster
    /// approaches infinity as corrective actions cannot be applied instantaneously. 
    /// </summary>
    public float ReactionTime = 0.4f;


    /// <summary>
    /// Smooths out slope sampling to help filter out noise or
    /// transient changes in ground slope (e.g. the edge of a cliff)
    /// </summary>
    public bool SlopeSmoothing = true;


    /// <summary>
    /// The smoothing factor (0,1) to use when slope smoothing is turned on.
    /// </summary>
    public float SmoothingFactor = 0.005f;

    private float _targetSlope = 0.0f;
    private float _targetAcceleration = 0.0f;

    private List<ActuatorModule> _rankedActuators;

    public override void Begin()
    {
        // Sort actuators from best to worst. We consider an actuator to be good if
        // it has a high ratio of angular to linear effects. That is, it can rotate
        // the vehicle without also moving it linearly. 
        _rankedActuators = new List<ActuatorModule>(Actuators);
        _rankedActuators.Sort(
            (a, b) => Mathf.Abs(b.AngularAcceleration / b.LinearAcceleration.magnitude)
            .CompareTo(Mathf.Abs(a.AngularAcceleration / a.LinearAcceleration.magnitude))
        );

        // Reset state
        _targetSlope = 0;
        _targetAcceleration = 0;
    }


    public override float Plan()
    {
        var groundSlope = Mathf.Rad2Deg * Mathf.Atan(ProjectedShadow.y / ProjectedShadow.x);

        if (SlopeSmoothing)
            // Apply smoothing through an exponential moving average approximation
            _targetSlope = SmoothingFactor * groundSlope + (1.0f - SmoothingFactor) * _targetSlope;
        else
            _targetSlope = groundSlope;

        // The rigidbody rotation isn't bounded, so we need to keep it 0-360 ourselves.
        var vehicleRotation = Rigidbody.rotation % 360.0f;
        var targetDisplacement = _targetSlope - vehicleRotation;

        // Always rotate in the shortest direction
        if (targetDisplacement > 180.0f)
            targetDisplacement -= 360.0f;
        else if (targetDisplacement < -180.0f)
            targetDisplacement += 360.0f;

        // Use an equation of motion to solve for the acceleration required to meet our target displacement.
        // s = ut + 0.5 * at^2 => a = 2(s - ut)/t^2
        // Where...
        //  s = target angular displacement
        //  u = current angular velocity
        //  t = the time over which we are accelerating
        //  a = resulting angular acceleration
        _targetAcceleration = 2.0f * (targetDisplacement - Rigidbody.angularVelocity * ReactionTime) / (ReactionTime * ReactionTime);

        return Mathf.Clamp01(Mathf.Abs(targetDisplacement) / MaxDeviation);
    }


    public override List<Tuple<ActuatorModule, float>> GenerateActions()
    {
        var actions = new List<Tuple<ActuatorModule, float>>();

        // We choose actions as a linear combination of actuator angular accelerations.
        // The actuators are ordered from best to worst, so the algorithm will greedily
        // pick the most effective choice of actuators.
        foreach (var actuator in _rankedActuators)
        {
            if (actuator.Disabled)
                continue;

            float requiredProportion = _targetAcceleration / actuator.AngularAcceleration;
            if (requiredProportion > 0.0f) // Negative proportion => wrong direction
            {
                if (!actuator.ProportionalControl)
                    requiredProportion = 1.0f;
                else
                    requiredProportion = Mathf.Clamp01(requiredProportion);

                _targetAcceleration -= requiredProportion * actuator.AngularAcceleration;

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