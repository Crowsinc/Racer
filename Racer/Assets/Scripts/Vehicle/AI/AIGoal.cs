using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class AIGoal : MonoBehaviour
{

    /// <summary>
    /// The AI controller that runs the AI goal
    /// </summary>
    public AIController Controller;

    /// <summary>
    /// Set to false to disable execution of this AIGoal
    /// </summary>
    public bool Enabled { get; protected set; }


    protected VehicleCore Vehicle { get => Controller.Vehicle; }

    protected Rigidbody2D Rigidbody { get => Controller.Vehicle.Rigidbody; }

    protected Vector2 Velocity { get => Controller.Vehicle.Rigidbody.velocity; }

    protected float AngularVelocity { get => Controller.Vehicle.Rigidbody.angularVelocity; }

    protected Vector2 CentreOfMass { get => Controller.Vehicle.Rigidbody.worldCenterOfMass; }

    protected CompositeCollider2D HullCollider { get => Controller.Vehicle.Collider; }

    protected List<Collider2D> Colliders { get => Controller.Colliders; }

    protected List<ContactPoint2D> Contacts { get => Controller.Contacts; }

    protected List<Vector2> Hull { get => Controller.Vehicle.Hull; }

    public List<ActuatorModule> Actuators { get => Controller.Actuators; }

    protected List<Rigidbody2D> Attachments { get => Controller.Vehicle.Attachments; }

    protected float EnergyLevel { get => Controller.Vehicle.EnergyLevel; }

    protected float EnergyCapacity { get => Controller.Vehicle.EnergyCapacity; }

    protected float EnergyPercentage { get => EnergyLevel / EnergyCapacity; }

    protected Vector2 GroundShadow { get => Controller.Shadow; }

    protected Vector2 ForwardShadow { get => Controller.ForwardShadow; }

    protected Vector2 ProjectedShadow { get => Controller.ProjectedShadow; }

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
        Enabled = true;
    }
}
