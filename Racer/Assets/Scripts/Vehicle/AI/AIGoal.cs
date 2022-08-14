using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class AIGoal : MonoBehaviour
{
    /// <summary>
    /// The vehicle that the AIGoal is attached to
    /// </summary>
    public VehicleCore Vehicle;

    // Vehicle Shortcuts

    protected Rigidbody2D Rigidbody { get => Vehicle.Rigidbody; }
    
    protected Vector2 Velocity { get => Vehicle.Rigidbody.velocity; }

    protected float AngularVelocity { get => Vehicle.Rigidbody.angularVelocity; }

    protected Vector2 CentreOfMass { get => Vehicle.Rigidbody.worldCenterOfMass; }

    protected CompositeCollider2D Collider { get => Vehicle.Collider; }

    protected List<Vector2> Hull { get => Vehicle.Hull; }

    protected List<Rigidbody2D> Attachments { get => Vehicle.Attachments; }
    
    protected List<ActuatorModule> Actuators { get => Vehicle.Actuators; }

    protected float EnergyLevel { get => Vehicle.EnergyLevel; }

    protected float EnergyCapacity { get => Vehicle.EnergyCapacity; }

    protected float EnergyPercentage { get => Vehicle.EnergyLevel / Vehicle.EnergyCapacity; }


    /// <summary>
    /// Runs when the AI controller begins simulating. 
    /// The Vehicle member is set before this runs. 
    /// </summary>
    public virtual void Begin() { }


    /// <summary>
    /// Ran once per FixedUpdate to determine the priority of the AI goal.
    /// </summary>
    /// <returns>
    /// Returns a value representing the priority of running this AI goal's actions.
    /// </returns>
    public abstract float Plan();


    /// <summary>
    /// Ran by the controller if the AI goal is chosen to be executed. 
    /// </summary>
    /// <returns>
    /// A list of ActuatorModules to activate, and their corresponding activation proportion. 
    /// </returns>
    public abstract List<Tuple<ActuatorModule, float>> GenerateActions();


    private void Awake()
    {
        if (!TryGetComponent<VehicleCore>(out Vehicle))
            Debug.LogWarning("AIGoal is not attached to a vehicle");
    }
}
