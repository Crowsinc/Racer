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
    public float MaxDeviation = 8.0f;


    /// <summary>
    /// The number of seconds through out which corrective actions will be distributed.
    /// As the reaction time approaches zero, the required acceleration of each thruster
    /// approaches infinity as corrective actions cannot be applied instantaneously. 
    /// </summary>
    public float ReactionTime = 0.2f;

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
    
    private float _targetAcceleration;
    private List<ActuatorModule> _rankedActuators;

    void Awake()
    {
        _mapMask = LayerMask.GetMask("Default");
    }


    public override void Begin()
    {
        // Sort actuators from best to worst. We consider an actuator to be good if
        // it has a high ratio of angular to linear effects. That is, it can rotate
        // the vehicle without also moving it linearly. 
        _rankedActuators = new List<ActuatorModule>(Actuators);
        _rankedActuators.Sort(
            (a, b) => Mathf.Abs(b.AngularAcceleration/b.LinearAcceleration.magnitude)
            .CompareTo(Mathf.Abs(a.AngularAcceleration/a.LinearAcceleration.magnitude))
        );
    }


    public override float Plan()
    {
        var step = new Vector2(SampleArea * 0.5f, 0);
        var offset = new Vector2(SampleOffset, 0);

        // Sample the ground
        var leftRay = Physics2D.Raycast(CentreOfMass + offset - step, Vector2.down, _maxRayDistance, _mapMask);
        var rightRay = Physics2D.Raycast(CentreOfMass + offset + step, Vector2.down, _maxRayDistance, _mapMask);

        Debug.DrawLine(CentreOfMass, rightRay.point, Color.yellow);
        Debug.DrawLine(CentreOfMass, leftRay.point, Color.yellow);

        float targetDisplacement = 0.0f;
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


            targetDisplacement = _groundSlope - Rigidbody.rotation;
        }
        else targetDisplacement = -Rigidbody.rotation;


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
        foreach(var actuator in _rankedActuators)
        {
            float requiredProportion = _targetAcceleration / actuator.AngularAcceleration;
            if(requiredProportion > 0.0f) // Negative proportion => wrong direction
            {
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