using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController : MonoBehaviour
{
    /// <summary>
    /// True if the controller is running the AI, otherwise false
    /// </summary>
    public bool Running { get; private set; }

    /// <summary>
    /// Whether multiple goals should run at once, or just the highest priority one.
    /// Keep this false, the blend mode sucks :(
    /// </summary>
    public bool BlendGoals = false;


    /// <summary>
    /// The number of FixedUpdate ticks that vehicle projections are estimated over.
    /// </summary>
    public uint ProjectionSampleTime = 15;


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
    /// A vector representing the forward direction of the vehicle 
    /// </summary>
    public Vector2 Forward { get; private set; }


    /// <summary>
    /// A vector representing the slope of the ground under the vehicle
    /// </summary>
    public Vector2 Shadow { get; private set; }


    /// <summary>
    /// A vector representing the slope of the ground where the vehicle is expected 
    /// </summary>
    public Vector2 ProjectedShadow { get; private set; }

    private int _maxRayDistance = 10000;
    private int _terrainLayerMask = 0;
    private int _raycastLayerMask = 0;
    private int _raycastLayerID = 0;

    private void Awake()
    {
        Running = false;
        Vehicle = GetComponent<VehicleCore>();
        if (Vehicle == null)
            Debug.LogError("AIController is not attached to a vehicle!");

        // This should be the layer that the map is on
        _terrainLayerMask = LayerMask.GetMask("Terrain");
        _raycastLayerMask = LayerMask.GetMask("AIRaycast");
        _raycastLayerID = LayerMask.NameToLayer("AIRaycast");

        Actuators = new List<ActuatorModule>();
        Contacts = new List<ContactPoint2D>();
        Colliders = new List<Collider2D>();
        Goals = new List<AIGoal>();
    }


    private void Initialize()
    {
        if (Vehicle.IsBuilt)
        {
            // Collect all vehicle colliders
            Colliders.Clear();
            Colliders.Add(Vehicle.Collider);
            foreach (var body in Vehicle.Attachments)
            {
                var bodyColliders = new List<Collider2D>();
                body.GetAttachedColliders(bodyColliders);
                Colliders.AddRange(bodyColliders);
            }

            // Get the dimensions of the vehicle in its local state. 
            var localHull = Vehicle.LocalHull;

            if (localHull.Count > 0)
            {
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
            }

            DetectActuators();
        }
        else Debug.LogError("Starting AI on unbuilt vehicle!");

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

        Forward = Vehicle.transform.TransformDirection(Vector2.right);

        // Reset vehicle shadows
        Shadow.Set(0, 0);
        ProjectedShadow.Set(0, 0);
        
        // Determine vehicle shadow bounds
        float verticalOffset = 100.0f; // This is just to avoid raycasting from within the map
        Vector2 halfSizeOffset = new Vector2(0.5f * LocalSize.x, 0.0f);

        Vector2 centreLine = new Vector2(
            Vehicle.Collider.bounds.center.x,
            Vehicle.Collider.bounds.center.y + verticalOffset
        );
        Vector2 rightBound = centreLine + halfSizeOffset;
        Vector2 leftBound = centreLine - halfSizeOffset;

        // Raycast downwards to detect the terrain beneath us
        var groundRay = Physics2D.Raycast(Vehicle.Rigidbody.worldCenterOfMass, Vector2.down, _maxRayDistance, _terrainLayerMask);
        if(groundRay.collider != null)
        {
            // Temporarily isolate the terrain into its own raycast layer
            var terrain = groundRay.collider.gameObject;
            
            var old_layer = terrain.layer;
            terrain.layer = _raycastLayerID;

            // Sample the map shadows through ray casts
            var leftRay = Physics2D.Raycast(leftBound, Vector2.down, _maxRayDistance, _raycastLayerMask);
            var rightRay = Physics2D.Raycast(rightBound, Vector2.down, _maxRayDistance, _raycastLayerMask);

            // NOTE: we add a right offset to the projection to ensure we are always looking forward some amount
            Vector2 projection = ProjectionSampleTime * (Time.fixedDeltaTime * Vehicle.Rigidbody.velocity + 0.1f * Vector2.right);
            var projectedLeftRay = Physics2D.Raycast(leftBound + projection, Vector2.down, _maxRayDistance, _raycastLayerMask);
            var projectedRightRay = Physics2D.Raycast(rightBound + projection, Vector2.down, _maxRayDistance, _raycastLayerMask);

            // Revert the ground to its original layer
            terrain.layer = old_layer;

            if (leftRay.collider != null || rightRay.collider != null)
                Shadow = rightRay.point - leftRay.point;

            if (projectedLeftRay.collider != null || projectedRightRay.collider != null)
                ProjectedShadow = projectedRightRay.point - projectedLeftRay.point;

            Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, rightRay.point, Color.yellow);
            Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, leftRay.point, Color.yellow);
            Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, projectedLeftRay.point, Color.red);
            Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, projectedRightRay.point, Color.red);
            Debug.DrawLine(Vehicle.Rigidbody.worldCenterOfMass, groundRay.point, Color.magenta);
        }
    }

    /// <summary>
    /// Starts the simulation of the AI controller.
    /// Does nothing if its already running
    /// </summary>
    public void StartSimulating()
    {
        if (!Running)
            Initialize();
        Running = true;
    }

    /// <summary>
    /// Stops the simulation of the AI controller
    /// </summary>
    public void StopSimulating()
    {
        Running = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Running || Vehicle.EnergyLevel <= 0) 
            return;

        UpdateSensors();

        // Run the controller for this update tick
        if (BlendGoals)
            BlendController();
        else
            MaxController();
    }
   

    private void BlendController()
    {
        Dictionary<ActuatorModule, float> globalActions = new Dictionary<ActuatorModule, float>();

        // Gather all goal actuators and their respective prioritised proportions
        var totalPriority = 0.0f;
        foreach (var goal in Goals)
        {
            var priority = goal.Plan();
            var actions = goal.GenerateActions();

            // Only accept the goal if it can actually action its plans
            if (actions.Count > 0)
            {
                totalPriority += priority;
                foreach (var (actuator, proportion) in actions)
                {
                    if(!globalActions.TryAdd(actuator, priority * proportion))
                        globalActions[actuator] += priority * proportion;
                }
            }
        }

        // Blend and run all required actuators 
        foreach(var (actuator, blendedProportion) in globalActions)
        {
            actuator.TryActivate(blendedProportion / totalPriority, true);
        }    
    }


    private void MaxController()
    {
        // Always run the highest priority goal which also has actions.
        var priorityActions = new List<Tuple<ActuatorModule, float>>();
        AIGoal priorityGoal = null;

        float highestPriority = float.NegativeInfinity;
        foreach (var goal in Goals)
        {
            var priority = goal.Plan();
            if (priority > highestPriority)
            {
                var actions = goal.GenerateActions();

                // Only accept the goal if it can actually action its plans
                if(actions.Count > 0)
                {
                    highestPriority = priority;
                    priorityActions = actions;
                    priorityGoal = goal;
                }
            }
        }

        foreach (var (actuator, proportion) in priorityActions)
            actuator.TryActivate(proportion, true);
    }

}
