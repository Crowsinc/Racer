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
    public float TargetSpeed = 50.0f;


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
        // Determine the target velocity of the vehicle. We always want to be moving
        // right towards the flag, but we also want to take the terrain into account.
        // Hence, the target velocity will be blend of the right vector and the shape
        // of the ground ahead of the vehicle. 
        var targetDirection = (0.5f * (Vector2.right + ProjectedShadow.normalized)).normalized;
        var targetVelocity = TargetSpeed * targetDirection;

        _targetAcceleration = (targetVelocity - Velocity) / ReactionTime;

        Debug.DrawRay(CentreOfMass, 10.0f * targetDirection, Color.magenta);
        Debug.DrawRay(CentreOfMass, 10.0f * Velocity.normalized, Color.black);

        // The AI goal will be prioritised when its velocity is in the direction of the
        // target velocity.That is, when it is most efficient to speed up. This also helps
        // with stability as the goal will prioritise speed less when on tricky terrain.
        return Mathf.Clamp01(Mathf.Abs(Vector2.Dot(Velocity.normalized, targetDirection)));
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