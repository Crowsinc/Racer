using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController : MonoBehaviour
{
    /// <summary>
    /// Set to true to run the AI controller, otherwise set to false. 
    /// </summary>
    public bool Simulate { get; set; }

    public bool _running = false;

    private AIGoal[] _goals;

    private VehicleCore _vehicle;

    private void Awake()
    {
        if (!TryGetComponent<VehicleCore>(out _vehicle))
            Debug.LogError("AIController is not attached to a vehicle!");

        _goals = GetComponents<AIGoal>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Simulate && !_running)
        {
            // If we just started simulating, the initialize all the goals
            foreach (var goal in _goals)
            {
                goal.Vehicle = _vehicle;
                goal.Begin();
            }
        }
        _running = Simulate;

        if (!_running || _vehicle.EnergyLevel <= 0) return;

        // Run highest priority goal.

        AIGoal priorityGoal = null;
        float highestPriority = float.NegativeInfinity;

        foreach (var goal in _goals)
        {
            var priority = goal.Plan();
            if (priority > highestPriority)
            {
                highestPriority = priority;
                priorityGoal = goal;
            }
        }

        if (priorityGoal != null)
        {
            var actions = priorityGoal.GenerateActions();
            foreach (var (actuator, proportion) in actions)
                actuator.TryActivate(proportion, true);
        }
    }
}
