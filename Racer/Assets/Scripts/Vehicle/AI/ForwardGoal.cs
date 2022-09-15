using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardGoal : AIGoal
{
    // This goal is solely for moving towards the right edge of the map


    /// <summary>
    /// The number of seconds through out which corrective actions will be distributed.
    /// As the reaction time approaches zero, the required acceleration of each thruster
    /// approaches infinity as corrective actions cannot be applied instantaneously. 
    /// </summary>
    public float ReactionTime = 0.2f;


    /// <summary>
    /// The target forward speed for the goal. 
    /// This is effectively the forward speed limit of the vehicle.
    /// </summary>
    public float TargetSpeed = 100.0f;


    /// <summary>
    /// The minimum priority of the goal. 
    /// Use this to ensure that the goal always wants to try move forward.
    /// </summary>
    public float MinimumPriority = 0.1f;


    /// <summary>
    /// How much leniency, in degrees, the goal has in its choice of prioritising moving forward.
    /// </summary>
    public float Leniency = 15.0f;


    private Vector2 _targetAcceleration;
    private List<ActuatorModule> _linearActuators = new List<ActuatorModule>();


    public override void Begin()
    {
        // Reset state
        _linearActuators = Actuators;
        _targetAcceleration = Vector2.zero;
    }


    public override float Plan()
    {
        var groundSlope = Mathf.Rad2Deg * Mathf.Atan(ProjectedShadow.y / ProjectedShadow.x);

        // Determine the target forward velocity for the vehicle. If the upcoming ground is 
        // flat or has an upwards incline, then we want to move forward in the direction of
        // the ground. That is, we want to climb or ride along the ground. If the upcoming
        // ground is downhill, speeding down the hill is generally a bad idea because there
        // will likely be a valley at the bottom which we will crash into. Hence, we simply
        // want to keep rightwards velocity when going downhill. 
        var targetDirection = (groundSlope >= 0) ? ProjectedShadow.normalized : Vector2.right;
        var targetVelocity = TargetSpeed * targetDirection;
        
        _targetAcceleration = (targetVelocity - Velocity) / ReactionTime;

        // If we don't need to accelerate, or we need to decelerate, don't run the goal
        if(_targetAcceleration.magnitude <= 0)
            return 0;

        // The AI goal will be prioritised when its forward direction is towards the 
        // target velocity. Depending on the vehicle design, this should be the most
        // efficient time to speed up. A minimum priority is also considered to ensure
        // the vehicle is always attempting to move forwards by at least a little. 
        //
        // NOTE: We could calculate the average direction of the vehicle's linear actuators
        // to determine the best direction for the vehicle to move linearly. But this is
        // probably taking it too far, the user should be punished for their bad designs.
        return Mathf.Clamp01(Mathf.Max(
            1.0f - Mathf.Clamp01((Vector2.Angle(targetDirection, Velocity.normalized) - Leniency) / 90.0f),
            1.0f - Mathf.Clamp01((Vector2.Angle(targetDirection, Forward) - Leniency) / 90.0f),
            MinimumPriority
        ));
    }


    public override List<Tuple<ActuatorModule, float>> GenerateActions()
    {
        // Decompose the target acceleration into a force direction and magnitude
        var targetForceDirection = _targetAcceleration.normalized;
        var targetForceMagnitude = Rigidbody.mass * _targetAcceleration.magnitude;

        // Rank each of the actuators in order of how much linear acceleration
        // they can provide in the target direction, while minimising angular
        // and linear acceleration effects in the wrong direction. 
        var ranking = new List<(float, float, int)>();
        for(int i = 0; i < _linearActuators.Count; i++)
        {
            var actuator = _linearActuators[i];

            if (actuator.Disabled)
                continue;

            // Scalar projection of the linear force in the direction of the target force.
            var usefulForce = Vector2.Dot(actuator.LinearForce, targetForceDirection);
            var byproduct = actuator.LinearForce - usefulForce * targetForceDirection;

            var wasteForces = byproduct.sqrMagnitude + actuator.AngularForce.sqrMagnitude;
            ranking.Add((usefulForce, wasteForces, i));
        }
        // Sort by ratio of useful forces to waste forces
        ranking.Sort((a, b) => (b.Item1 / b.Item2).CompareTo(a.Item1 / a.Item2));

        // Generate actions using the ranking
        var actions = new List<Tuple<ActuatorModule, float>>();
        foreach(var (actuatorForce, _, index) in ranking)
        {
            float requiredProportion = targetForceMagnitude / actuatorForce;
            if (requiredProportion > 0.0f) // Negative proportion => wrong direction
            {
                var actuator = _linearActuators[index];

                if (!actuator.ProportionalControl)
                    requiredProportion = 1.0f;
                else
                    requiredProportion = Mathf.Clamp01(requiredProportion);

                targetForceMagnitude -= requiredProportion * actuatorForce;

                actions.Add(new Tuple<ActuatorModule, float>(actuator, requiredProportion));
            }
        }

        return actions;
    }
}