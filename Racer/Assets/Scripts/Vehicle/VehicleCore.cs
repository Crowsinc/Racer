using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCore : MonoBehaviour
{

    /// <summary>
    /// The total energy capacity of the vehicle
    /// </summary>
    public float EnergyCapacity { get; private set; }

    /// <summary>
    /// The current energy level of the vehicle, from 0.0 to EnergyCapacity
    /// </summary>
    public float EnergyLevel 
    {
        get => _energyLevel;
        set { _energyLevel = Mathf.Clamp(value, 0.0f, EnergyCapacity); } 
    }
    private float _energyLevel = 0.0f;


    /// <summary>
    /// The core rigidbody of the vehicle
    /// </summary>
    [HideInInspector]
    public Rigidbody2D Rigidbody;


    /// <summary>
    /// The composite collider generated for the hull of the vehicle
    /// </summary>
    public CompositeCollider2D Collider { get; private set; }


    /// <summary>
    /// A counter-clockwise list of vertices outlining shape of the vehicle's collider in the world.
    /// </summary>
    public List<Vector2> Hull { get; private set; }


    /// <summary>
    /// A list of all actuators built into vehicle
    /// </summary>
    public List<ActuatorModule> Actuators { get; private set; }


    /// <summary>
    /// A list of all rigidbodies attached to the vehicle through joints
    /// </summary>
    public List<Rigidbody2D> Attachments { get; private set; }


    /// <summary>
    /// True if the vehicle has been built into a design, false otherwise.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Generates the structure of the vehicle given the provided design.
    /// </summary>
    /// <param name="design">
    /// A dictionary of prefabs that represent vehicle modules, keyed by their offset from the vehicle core at (0,0).
    /// The prefabs should contain VehicleModule and/or ActuatorModule components to be useful, and must form a
    /// fully connected mass. Note that the offset at (0,0) is reserved for the VehicleCore.
    /// </param>
    /// <returns> 
    /// False if the provided design is does not meet the design pre-conditions, otherwise true.
    /// </returns>
    public bool TryBuildStructure(Dictionary<Vector2Int /* module offset */, GameObject /* module prefab */> design)
    {
        // Add ourselves to the design so our module properties are taken into account
        design[new Vector2Int(0, 0)] = gameObject; 

        ClearStructure();

        float totalMass = 0.0f;
        float totalEnergyCapacity = 0.0f;
        Vector2 centreOfMass = Vector2.zero;

        foreach (var (offset, prefab) in design)
        {
            // Instantiate new vehicle module, unless this is the core
            var position = transform.position + new Vector3(offset.x, offset.y);
            var module = prefab == gameObject ? gameObject
                : Instantiate(prefab, position, Quaternion.identity, transform);

            // Register VehicleModule to the vehicle
            if (module.TryGetComponent<VehicleModule>(out VehicleModule properties))
            {
                totalEnergyCapacity += properties.EnergyCapacity;
                centreOfMass += properties.Mass * (Vector2)offset;
                totalMass += properties.Mass;

                // If the module has a collider, then absorb it into the vehicle core
                if (properties.Collider != null)
                {
                    // Get the collider vertices relative to the VehicleCore, taking  
                    // into account any transforms and local offsets of the collider
                    var localColliderOffset = (Vector2)(properties.Collider.transform.position - position);
                    var localPoints = properties.Collider.points;
                    for (int i = 0; i < localPoints.Length; i++)
                    {
                        localPoints[i] *= (Vector2)module.transform.localScale;
                        localPoints[i] += offset + localColliderOffset;
                    }

                    // Create our own collider to represent the module's collider
                    var polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
                    polygonCollider.points = localPoints;
                    polygonCollider.pathCount = 1;
                    polygonCollider.usedByComposite = true;

                    // Old collider is no longer necessary
                    properties.Collider.enabled = false;
                }
                else if(prefab != gameObject)
                    Debug.LogWarning($"Vehicle module at {offset} has no PolygonCollider2D");

                // If the module provides joints for attached Rigidbodies, then connect
                // the joint onto the vehicle core so that it acts in union with the vehicle.
                foreach (var joint in properties.Attachments)
                {
                    joint.connectedBody = Rigidbody;
                    Attachments.Add(joint.attachedRigidbody);
                }
            }
            else Debug.LogError($"Vehicle module at {offset} has no VehicleModule component");

            // Register ActuatorModules to the vehicle
            if (module.TryGetComponent<ActuatorModule>(out ActuatorModule actuator))
                Actuators.Add(actuator);
        }
        
        // Merge all our module colliders together into one composite collider
        Collider.GenerateGeometry();

        // If we have more than one path, then the vehicle structure is not fully connected
        if(Collider.pathCount > 1)
        {
            Debug.LogError($"Vehicle structure is not fully connected ({Collider.pathCount} Sections)");
            ClearStructure();
            return false;
        }

        // Set physical properties of the Vehicle
        EnergyCapacity = totalEnergyCapacity;
        Rigidbody.mass = totalMass;
        Rigidbody.centerOfMass = centreOfMass / totalMass;

        // Link all actuators to the vehicle, this must happen after
        // all of the vehicles physical properties have been discovered.
        foreach (var actuator in Actuators)
            actuator.LinkedVehicle = this;

        ResetVehicle();

        IsBuilt = true;
        return true;
    }


    /// <summary>
    /// Clears the vehicle structure design, leaving it as an empty VehicleCore
    /// that is ready to be built again.
    /// </summary>
    public void ClearStructure()
    {
        if (IsBuilt)
        {
            Hull?.Clear();
            Actuators?.Clear();
            Attachments?.Clear();

            // Delete vehicle modules
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<VehicleModule>() != null)
                    Destroy(child.gameObject);
            }
 
            // Clean up the composite colliders
            var colliders = new List<PolygonCollider2D>();
            GetComponents<PolygonCollider2D>(colliders);
            for (int i = 0; i < colliders.Count; i++)
                if (colliders[i].usedByComposite && colliders[i].composite == Collider)
                    Destroy(colliders[i]);

            // Reset vehicle properties
            EnergyCapacity = 0;
            Rigidbody.mass = 1;
            Rigidbody.centerOfMass = Vector3.zero;
        }

        IsBuilt = false;
        ResetVehicle();
    }


    /// <summary>
    /// Resets vehicle statistics (EnergyLevel etc.) to their initial value 
    /// </summary>
    public void ResetVehicle()
    {
        EnergyLevel = EnergyCapacity;
    }


    void Start()
    {
        Hull = new List<Vector2>();
        Attachments = new List<Rigidbody2D>();
        Actuators = new List<ActuatorModule>();

        Collider = gameObject.AddComponent<CompositeCollider2D>();
        Collider.geometryType = CompositeCollider2D.GeometryType.Polygons;

        if (!TryGetComponent<Rigidbody2D>(out Rigidbody))
            Debug.LogError("VehicleCore failed to find a Rigidbody2D");

    }


    void FixedUpdate()
    {
        // Update the vehicle hull
        Hull.Clear();
        Collider.GetPath(0, Hull);
        for (int i = 0; i < Hull.Count; i++)
            Hull[i] = (Vector2)transform.TransformPoint(Hull[i]);
    }


    void OnDrawGizmos()
    {
        // Draw the vehicle hull for debug purposes
        if(Hull?.Count > 0)
        {
            Vector2 prev = Hull[^1];
            foreach(var point in Hull)
            {
                Debug.DrawLine(prev, point, Color.magenta);
                prev = point;
            }
        }

        // Draw a cross at the centre of gravity
        if(Rigidbody != null)
        {
            float size = 0.25f;
            Debug.DrawLine(
                Rigidbody.worldCenterOfMass + new Vector2( size, 0),
                Rigidbody.worldCenterOfMass + new Vector2(-size, 0),
                Color.red
            );
            Debug.DrawLine(
                Rigidbody.worldCenterOfMass + new Vector2(0,  size),
                Rigidbody.worldCenterOfMass + new Vector2(0, -size),
                Color.red
            );
            Debug.DrawLine(
               Rigidbody.worldCenterOfMass + new Vector2(0,  size),
               Rigidbody.worldCenterOfMass + new Vector2(size, 0),
               Color.red
           );
            Debug.DrawLine(
               Rigidbody.worldCenterOfMass + new Vector2(-size, 0),
               Rigidbody.worldCenterOfMass + new Vector2(0, -size),
               Color.red
           );
        }
    }
}
