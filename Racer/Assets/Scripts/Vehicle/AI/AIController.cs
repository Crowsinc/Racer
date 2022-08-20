using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController : MonoBehaviour
{
    /* TODO:
     *  3 - improve Forward and stability goals
     *  4- Test out multiple goals at the same time
     */

    /// <summary>
    /// Set to true to run the AI controller, otherwise set to false. 
    /// </summary>
    public bool Simulate = false;
    private bool _running = false;


    /// <summary>
    /// The number of FixedUpdate ticks that vehicle projections are estimated over.
    /// </summary>
    public uint ProjectionSampleTime = 2;


    /// <summary>
    /// The AI goals being serviced by this controller. 
    /// </summary>
    public List<AIGoal> Goals { get; private set; }


    /// <summary>
    /// The vehicle that the controller is attached to
    /// </summary>
    public VehicleCore Vehicle { get; private set; }


    /// <summary>
    /// The local extents of the vehicle's hull.
    /// </summary>
    public Vector2 LocalSize { get; private set; }


    /// <summary>
    /// List of AI actuatable actuators
    /// </summary>
    public List<ActuatorModule> Actuators { get; private set; }


    /// <summary>
    /// List of all vehicle colliders, including the hull and all attachments
    /// </summary>
    public List<Collider2D> Colliders { get; private set; }


    /// <summary>
    /// List of all current contact points across all colliders
    /// </summary>
    public List<ContactPoint2D> Contacts { get; private set; }


    /// <summary>
    /// A vector representing the slope of the ground under the vehicle
    /// </summary>
    public Vector2 Shadow { get; private set; }


    /// <summary>
    /// A vector representing the slope of the ground under the front half of the vehicle.
    /// </summary>
    public Vector2 ForwardShadow { get; private set; }


    /// <summary>
    /// A vector representing the slope of the ground where the vehicle is expected 
    /// </summary>
    public Vector2 ProjectedShadow { get; private set; }


    private int _mapMask = 0;
    private int _maxRayDistance = 10000;

    private void Awake()
    {
        Vehicle = GetComponent<VehicleCore>();
        if (Vehicle == null)
            Debug.LogError("AIController is not attached to a vehicle!");

        // This should be the layer that the map is on
        _mapMask = LayerMask.GetMask("Default");

        Actuators = new List<ActuatorModule>();
        Contacts = new List<ContactPoint2D>();
        Colliders = new List<Collider2D>();
        Goals = new List<AIGoal>();
    }


    private void Initialize()
    {
        // Collect all vehicle colliders
        Colliders.Clear();
        Colliders.Add(Vehicle.Collider);
        foreach(var body in Vehicle.Attachments)
        {
            var bodyColliders = new List<Collider2D>();
            body.GetAttachedColliders(bodyColliders);
            Colliders.AddRange(bodyColliders);
        }

        // Get the dimensions of the vehicle in its local state. 
        var localHull = Vehicle.LocalHull;

        Vector2 min = localHull[0], max = localHull[0];
        foreach (var point in localHull)
        {
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
        LocalSize = new Vector2(
            Mathf.Abs(max.x - min.x),
            Mathf.Abs(max.y - min.y)
        );

        DetectActuators();

        // Gather all AI goals
        Goals.Clear();
        Goals.AddRange(GetComponentsInChildren<AIGoal>());

        foreach (var goal in Goals)
        {
            goal.Controller = this;
            goal.Begin();
        }
    }


    private void DetectActuators()
    {
        // Collect all AI controllable actuators
        Actuators.Clear();
        foreach (var actuator in Vehicle.Actuators)
            if (actuator.AIControllable)
                Actuators.Add(actuator);
    }


    private void UpdateSensors()
    {
        // Get all contact points
        Contacts.Clear();
        var contactList = new List<ContactPoint2D>();
        foreach(var collider in Colliders)
        {
            collider.GetContacts(contactList);
            Contacts.AddRange(contactList);
        }

        float verticalOffset = 500.0f; // This is just to avoid raycasting into the map
        Vector2 halfSizeOffset = new Vector2(0.5f * LocalSize.x, 0.0f);

        Vector2 centreLine = new Vector2(Vehicle.Collider.bounds.center.x, verticalOffset);
        Vector2 rightBound = centreLine + halfSizeOffset;
        Vector2 leftBound = centreLine - halfSizeOffset;
        
        // Sample the map shadows through ray casts
        var leftRay = Physics2D.Raycast(leftBound, Vector2.down, _maxRayDistance, _mapMask);
        var rightRay = Physics2D.Raycast(rightBound, Vector2.down, _maxRayDistance, _mapMask);

        Vector2 projection = ProjectionSampleTime * Time.fixedDeltaTime * Vehicle.Rigidbody.velocity;
        var projectedLeftRay = Physics2D.Raycast(leftBound + projection, Vector2.down, _maxRayDistance, _mapMask);
        var projectedRightRay = Physics2D.Raycast(rightBound + projection, Vector2.down, _maxRayDistance, _mapMask);

        if (leftRay.collider != null || rightRay.collider != null)
            Shadow = rightRay.point - leftRay.point;
        else
            Shadow.Set(0, 0);

        if (projectedLeftRay.collider != null || projectedRightRay.collider != null)
            ProjectedShadow = projectedRightRay.point - projectedLeftRay.point;
        else
            ProjectedShadow.Set(0, 0);

        Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, rightRay.point, Color.yellow);
        Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, leftRay.point, Color.yellow);
        Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, projectedLeftRay.point, Color.red);
        Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, projectedRightRay.point, Color.red);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // If we just started simulating, initialize the controller and all goals
        if (Simulate && !_running)
            Initialize();

        _running = Simulate;
        if (!_running || Vehicle.EnergyLevel <= 0) 
            return;

        UpdateSensors();

        // Run highest priority goal.
        AIGoal priorityGoal = null;
        float highestPriority = float.NegativeInfinity;

        foreach (var goal in Goals)
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
