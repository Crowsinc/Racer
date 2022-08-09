using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCore : MonoBehaviour
{
    /// <summary>
    /// Game feel value which scales the strength of aerodynamic drag on the vehicle.
    /// </summary>
    public float AerodynamicDragCoefficient = 0.75f;


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
    public CompositeCollider2D Collider { get; set; }


    /// <summary>
    /// A counter-clockwise list of vertices outlining shape of the vehicle's collider in the world.
    /// </summary>
    public List<Vector2> Hull { get; private set; }


    /// <summary>
    /// A list of all actuators built into vehicle
    /// </summary>
    public List<ActuatorModule> Actuators { get; private set; }


    /// <summary>
    /// A list of all modules built into this vehicle. 
    /// </summary>
    public List<VehicleModule> Modules { get; private set; }


    /// <summary>
    /// A list of all rigidbodies attached to the vehicle through joints
    /// </summary>
    public List<Rigidbody2D> Attachments { get; private set; }


    /// <summary>
    /// True if the vehicle has been built into a design, false otherwise.
    /// </summary>
    public bool IsBuilt { get; private set; }


    /// <summary>
    /// Generates the given design of the vehicle onto the VehicleCore.
    /// </summary>
    /// <param name="design">
    /// A dictionary of ModuleSchematics, describing a prefab that is to be placed as a vehicle module.
    /// Each ModuleSchematic is keyed by its positional offset from the VehicleCore. As such, the offset
    /// (0,0) will be reserved for the vehicle core and ignored. 
    /// </param>
    /// <returns> 
    /// False if the provided design is not fully connected, otherwise true.
    /// </returns>
    public bool TryBuildStructure(Dictionary<Vector2Int, ModuleSchematic> design)
    {
        // Add ourselves to the design so our module properties are taken into account
        design[new Vector2Int(0, 0)] = new ModuleSchematic(gameObject);

        ClearStructure();

        float totalMass = 0.0f;
        float totalEnergyCapacity = 0.0f;
        Vector2 centreOfMass = Vector2.zero;

        foreach (var (offset, (prefab, rotation)) in design)
        {
            // Instantiate new vehicle module, unless this is the core
            var position = transform.position + new Vector3(offset.x, offset.y);
            var module = prefab == gameObject ? gameObject
                : Instantiate(prefab, position, Quaternion.identity, transform);

            // Rotate module to desired orientation
            var rotationQuat = Quaternion.Euler(0, 0, rotation);
            module.transform.rotation = rotationQuat;

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
                    Vector2 localColliderOffset = properties.Collider.transform.position - position;
                    Vector2 localColliderScale = properties.Collider.transform.localScale;
                    Vector2[] localPoints = properties.Collider.points;
                    for (int i = 0; i < localPoints.Length; i++)
                    {
                        localPoints[i] = rotationQuat * (localColliderScale * localPoints[i]);
                        localPoints[i] += offset + localColliderOffset;
                    }

                    // Create our own collider to represent the module's collider
                    var polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
                    polygonCollider.points = localPoints;
                    polygonCollider.pathCount = 1;
                    polygonCollider.usedByComposite = true;

                    // Old collider is no longer necessary
                    properties.Collider.enabled = false;

                    Modules.Add(properties);
                }
                else if (prefab != gameObject)
                    Debug.LogWarning($"Vehicle module at {offset} has no PolygonCollider2D");

                // If the module provides joints for attached Rigidbodies, then connect
                // the joint onto the vehicle core so that it acts in union with the vehicle.
                foreach (var joint in properties.Attachments)
                {
                    joint.connectedBody = Rigidbody;
                    Attachments.Add(joint.attachedRigidbody);

                    // NOTE: we treat the set connectedAnchor value (as set in the editor) as
                    // an offset from center of the rigidbody that holds the joint (i.e. the attachment). 
                    Vector2 localBodyOffset = joint.attachedRigidbody.transform.position - module.transform.position;
                    joint.connectedAnchor = (offset + localBodyOffset) + joint.connectedAnchor;
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
        if (Collider.pathCount > 1)
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

        // Link all vehicle modules to the vehicle
        foreach (var module in Modules)
            module.LinkedVehicle = this;

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
        Hull?.Clear();
        Actuators?.Clear();
        Attachments?.Clear();
        Modules?.Clear();

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


    void Awake()
    {
        Hull = new List<Vector2>();
        Attachments = new List<Rigidbody2D>();
        Actuators = new List<ActuatorModule>();
        Modules = new List<VehicleModule>();

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


        // Calculate aerodynamic drag
        // The aerodynamic drag formula is:
        // Fd = 0.5 * rho * v^2 * C * A,
        // where:
        //   rho = air density
        //   v = relative velocity
        //   C = drag coefficient
        //   A = drag area
        //
        // Unity's linear drag equation, v = v * (1 - drag), is too simple to 
        // implement proper aerodynamic drag, which is proportional to the square
        // of the velocity. So instead we set the rigidbody linear drag to 0 and
        // apply our own drag force. This will be a simplified version which acts 
        // on the centre of mass and removes air density.The drag coefficient will 
        // be a vehicle parameter set during game balancing to introduce the correct
        // game feel. The drag area will be approximated on fly for each hull segment via:
        //
        // drag area = sum(segment area * clamp(1.0 - dot(segment normal, vehicle velocity), 0, 1))
        //
        // Hull segments which are perpendicular to the vehicle velocity will have their
        // full area considered, while those which are parralel or on the opposite side
        // of the vehicle will be ignored. Note that we care about the opposite velocity
        // of the drag for the vector dot product, which is why we are using the vehicle
        // velocity.

        // Velocity of vehicle, assuming zero wind speed.
        var velocityDir = Rigidbody.velocity.normalized;
        var velocitySqr = velocityDir * Rigidbody.velocity.sqrMagnitude;

        // Find drag area
        float dragArea = 0.0f;
        Vector2 prevPoint = Hull[^1];
        foreach (Vector2 currPoint in Hull)
        {
            // The hull is in counter-clockwise order but we want the segment
            // to be clockwise so that Vector2.perpendicular() gives us the
            // outer facing normals of the vehicle hull.
            var segment = prevPoint - currPoint;
            var outerNormal = Vector2.Perpendicular(segment).normalized;
            
            dragArea += segment.magnitude * Mathf.Clamp01(Vector2.Dot(outerNormal, velocityDir));

            prevPoint = currPoint;
        }

        // NOTE: we invert the force direction here, to make it drag (-0.5f)
        var aerodynamicDragForce = -0.5f * AerodynamicDragCoefficient * dragArea * velocitySqr;

        Rigidbody.drag = 0.0f;
        Rigidbody.AddForce(aerodynamicDragForce);
    }


    void OnValidate()
    {
        AerodynamicDragCoefficient = Mathf.Max(AerodynamicDragCoefficient, 0);
    }
}

/// <summary>
/// Defines information about a prefab that is to be built onto a VehicleCore as
/// a vehicle module. A struct is used so that it can be easily extended with more
/// necessary data as time moves on.
/// </summary>
public struct ModuleSchematic
{
    /// <summary>
    /// The prefab which implements a vehicle module. 
    /// </summary>
    public GameObject Prefab { get; private set; }

    /// <summary>
    /// The rotation (degrees) the prefab when placed onto the vehicle. 
    /// </summary>
    public float Rotation { get; private set; }

    /// <summary>
    /// Constructs the schematic
    /// </summary>
    /// <param name="prefab"> 
    /// The prefab which implements a vehicle module.
    /// The prefab should contain a VehicleModule and/or ActuatorModule.
    /// </param>
    /// <param name="rotation">
    /// The rotation (degrees) utilized when placing the module onto the VehicleCore.
    /// This allows modules to be placed with different orientations.
    /// </param>
    public ModuleSchematic(GameObject prefab, float rotation = 0.0f)
    {
        Prefab = prefab;
        Rotation = rotation;
    }


    public void Deconstruct(out GameObject prefab, out float rotation)
    {
        prefab = Prefab;
        rotation = Rotation;
    }
}