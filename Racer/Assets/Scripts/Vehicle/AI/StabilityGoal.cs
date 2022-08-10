using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StabilityGoal : AIGoal
{

    public override List<Tuple<ActuatorModule, float>> GenerateActions()
    {
        return new List<Tuple<ActuatorModule, float>>();
    }

    public override float Plan()
    {
        return 0.0f;
    }

}
