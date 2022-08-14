using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardGoal : AIGoal
{
    public float targetSpeed = 1000f;
    public float reactionTime = 0.2f;

    private float _targetAcceleration;
    private List<ActuatorModule> _rankedActuators;
    public override void Begin()
    {
        _rankedActuators = new List<ActuatorModule>(Actuators);
        _rankedActuators.Sort((a, b) => b.LinearAcceleration.magnitude.CompareTo(a.LinearAcceleration.magnitude));
        // _rankedActuators.Sort((a, b) => b.LinearAcceleration.x.CompareTo(a.LinearAcceleration.x));
    }

    public override List<Tuple<ActuatorModule, float>> GenerateActions()
    {
        var actions = new List<Tuple<ActuatorModule, float>>();
        foreach(var actuator in Actuators)
        {
            float requiredProportion = _targetAcceleration / actuator.LinearAcceleration.magnitude;
            // float requiredProportion = _targetAcceleration / actuator.LinearAcceleration.x;

            if (requiredProportion > 0.0f)
            {
                requiredProportion = Mathf.Clamp01(requiredProportion);
                _targetAcceleration -= requiredProportion * actuator.LinearAcceleration.magnitude;
                // _targetAcceleration -= requiredProportion * actuator.LinearAcceleration.x;

                actions.Add(new Tuple<ActuatorModule, float>(actuator, requiredProportion));
            }
        }
        return actions;
    }

    public override float Plan()
    {
        _targetAcceleration = 2.0f * (targetSpeed - Rigidbody.angularVelocity * reactionTime) / (reactionTime * reactionTime);
        return 0.9f;
    }
}