using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCore : MonoBehaviour
{
    /// <summary>
    /// If set to true, the vehicle will discover its own rigidbody properties, colliders, actuators and attachments on start().
    /// </summary>
    public bool Pregenerated = false;


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
    /// The composite collider for the hull of the vehicle
    /// </summary>
    [HideInInspector]
    public CompositeCollider2D Collider;


    /// <summary>
    /// A counter-clockwise list of vertices outlining shape of the vehicle's collider in the world.
    /// </summary>
    public List<Vector2> Hull { get; private set; }
    private List<Vector2> _localHull = new List<Vector2>();


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
    /// False if the provided design has an invalid hull, otherwise true.
    /// A hull is considered invalid if it isn't fully connected or has holes. 
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
            //Debug.Log("off:" + offset);
            // Instantiate new vehicle module, unless this is the core
            var position = transform.position + new Vector3(offset.x, offset.y) +
                (rotation == 90 ? Vector3.right : rotation == 180 ? Vector3.one : rotation == 270 ? Vector3.up : Vector3.zero);
            var instance = prefab == gameObject ? gameObject
                : Instantiate(prefab, position, Quaternion.identity, transform);

            // Rotate module to desired orientation
            instance.transform.rotation = Quaternion.Euler(0, 0, rotation);

            // Register VehicleModule to the vehicle
            if (instance.TryGetComponent<VehicleModule>(out VehicleModule module))
            {
                totalEnergyCapacity += module.EnergyCapacity;
                centreOfMass += module.Mass * (Vector2)offset;
                totalMass += module.Mass;

                RegisterModule(module, rotation);
            }
            else Debug.LogError($"Vehicle module at {offset} has no VehicleModule component");

            // Register ActuatorModules to the vehicle
            if (instance.TryGetComponent<ActuatorModule>(out ActuatorModule actuator))
                Actuators.Add(actuator);
        }

        // Merge all our module colliders together into one composite collider
        Collider.GenerateGeometry();
        _localHull = DetectHull(out bool disjoint);

        // Validate our vehicles hull to make sure
        if (disjoint)
        {
            Debug.LogError($"Vehicle hull is disjoint");
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
    /// Self-analysis of rigidbody properties, colliders, actuators and attachments.
    /// </summary>
    private void Discover()
    {
        Modules.Clear();
        Actuators.Clear();
        Attachments.Clear();

        float totalMass = 0.0f;
        float totalEnergyCapacity = 0.0f;
        Vector2 centreOfMass = Vector2.zero;

        // register modules
        var modules = gameObject.GetComponentsInChildren<VehicleModule>(false);
        foreach (var module in modules)
        {
            Vector2 offset = module.transform.position - transform.position;

            totalEnergyCapacity += module.EnergyCapacity;
            centreOfMass += module.Mass * offset;
            totalMass += module.Mass;

            RegisterModule(module);
        }

        Collider.GenerateGeometry();
        if (Collider.pathCount != 1)
            Debug.LogError($"Pre-generated vehicle hull is invalid ({Collider.pathCount} Sections)");

        EnergyCapacity = totalEnergyCapacity;
        Rigidbody.mass = totalMass;
        Rigidbody.centerOfMass = centreOfMass / totalMass;

        // Register actuators
        var actuators = gameObject.GetComponentsInChildren<ActuatorModule>(false);
        foreach (var actuator in actuators)
        {
            actuator.LinkedVehicle = this;
            Actuators.Add(actuator);
        }

        ResetVehicle();
        IsBuilt = true;
    }

  
    /// <summary>
    /// Clears the vehicle structure design, leaving it as an empty VehicleCore
    /// that is ready to be built again.
    /// </summary>
    public void ClearStructure()
    {
        _localHull?.Clear();
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


    /// <summary>
    /// Registers a vehicle module onto the vehicle core
    /// </summary>
    /// <param name="module"> 
    /// The module to register 
    /// </param>
    /// <param name="generateCollider"> 
    /// True, if the collider for this module should be generated automatically 
    /// </param>
    private void RegisterModule(VehicleModule module, float rotation = 0)
    {
        Vector2 offset = module.transform.position - transform.position;
        
        // If the module has a collider, then absorb it into the vehicle core
        if (module.Collider != null)
        {
            // Get the collider vertices relative to the VehicleCore, taking  
            // into account any transforms and local offsets of the collider
            Vector2 localColliderOffset = module.Collider.transform.position - module.transform.position;
            localColliderOffset += (
                rotation == 90 ? new Vector2(-module.Collider.offset.y, module.Collider.offset.x) : 
                rotation == 180 ? -module.Collider.offset : 
                rotation == 270 ? new Vector2(module.Collider.offset.y, -module.Collider.offset.x) : 
                module.Collider.offset);
            Vector2 localColliderScale = module.Collider.transform.localScale;
            Vector2[] localPoints = module.Collider.points;
            for (int i = 0; i < localPoints.Length; i++)
            {
                localPoints[i] = module.transform.rotation * (localColliderScale * localPoints[i]);
                localPoints[i] += offset + localColliderOffset;
            }

            // Create our own collider to represent the module's collider
            var polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
            polygonCollider.points = localPoints;
            polygonCollider.pathCount = 1;
            polygonCollider.usedByComposite = true;

            // Old collider is no longer necessary
            module.Collider.enabled = false;
        }
        else if (module.gameObject != gameObject)
            Debug.LogWarning($"Vehicle module at {offset} has no PolygonCollider2D");

        module.LinkedVehicle = this;
        Modules.Add(module);

        // If the module provides joints for attached Rigidbodies, then connect
        // the joint onto the vehicle core so that it acts in union with the vehicle.
        foreach (var joint in module.Attachments)
        {
            Attachments.Add(joint.attachedRigidbody);

            if(joint.connectedBody == null)
            {
                joint.connectedBody = Rigidbody;

                // NOTE: we treat the set connectedAnchor value (as set in the editor) as
                // an offset from center of the rigidbody that holds the joint (i.e. the attachment). 
                Vector2 localBodyOffset = joint.attachedRigidbody.transform.position - module.transform.position;
                joint.connectedAnchor = (offset + localBodyOffset) + joint.connectedAnchor;
            }
        }
    }


    /// <summary>
    /// Attempts to detect the hull path of the vehicle's composite collider
    /// </summary>
    /// <param name="disjoint">
    /// This output parameter is set to true if multiple disjoint hulls exist (vehicle is not fully connected)
    /// </param>
    /// <returns>
    /// The hull path of the vehicles collider. 
    /// This is only guaranteed to be correct if the vehicle is not disjoint. 
    /// </returns>
    private List<Vector2> DetectHull(out bool disjoint)
    {
        // All collider paths whose bounds are smaller than this number are filtered out.
        const float minColliderArea = 0.01f;

        // If theres only one path, then it must be the hull
        if (Collider.pathCount < 2)
        {
            disjoint = false;

            var path = new List<Vector2>();
            Collider.GetPath(0, path);
            return path;
        }

        // Collect all collider paths
        var paths = new List<List<Vector2>>();
        for (int i = 0; i < Collider.pathCount; i++)
        {
            var path = new List<Vector2>();
            Collider.GetPath(i, path);

            // Get bounding box of collider
            Vector2 max = path[0], min = path[0];
            foreach (var p in path)
            {
                max = Vector2.Max(max, p);
                min = Vector2.Min(min, p);
            }
            var diagonal = max - min;
            var area = diagonal.x * diagonal.y;

            // Filter out any tiny collider paths, probably caused
            // by bad colliders on a module. This is a hack but
            // the problem lies with the colliders not the hull
            // detection ¯\_(?)_/¯

            if (area > minColliderArea)
                paths.Add(path);
            else
                Debug.LogWarning("A bad module collider was filtered out for hull detection");
        }

        // Remove all paths representing holes within a hull defined by another path.
        // We can determine if one path is inside another, if any of its points is
        // inside the (potentially convex) polygon formed by another path. Similarly,
        // we can determine if one path is outside of another if any of its points 
        // are outside of this polygon. This holds because by definition of the 
        // composite collider, one path cannot collide with another path; otherwise
        // the composite collider would have merged them together into a single path.
        // All of this together means that we just need to test a single point of
        // each path. Point in polygon tests are performed through the standard
        // raycast crossing number test. 

        for (int p1 = 0; p1 < paths.Count; p1++)
        {
            for (int p2 = 0; p2 < paths.Count; p2++)
            {
                if (p1 == p2) continue;

                var polygonPath = paths[p1];
                var testPoint = paths[p2][0];

                int intersections = 0;
                var s1 = polygonPath[^1];
                foreach (var s2 in polygonPath)
                {
                    var segment = s2 - s1;
                    var min = Vector2.Min(s1, s2);
                    var max = Vector2.Max(s1, s2);
                    var plane = new Plane(Vector2.Perpendicular(segment), s1);

                    Ray ray = new Ray(testPoint, Vector2.right);
                    if (plane.Raycast(ray, out float distance))
                    {
                        var intersectPoint = ray.GetPoint(distance);
                        if (intersectPoint.x >= min.x && intersectPoint.x <= max.x 
                         && intersectPoint.y >= min.y && intersectPoint.y <= max.y)
                            intersections++;
                    }
                    
                    s1 = s2;
                }

                if (intersections % 2 == 1)
                {
                    // If there is an odd number of intersections, then the point
                    // must be inside an outer hull, so it must be a hole.
                    paths.RemoveAt(p2--);
                    if (p2 < p1)
                        p1--;
                }
            }
        }

        // The hull is disjoint if we end up with more than one path here.
        disjoint = paths.Count > 1;

        // If the hull isnt disjoint than paths[0] should contain the hull.
        // This is not guaranteed if it is disjoint, but we could easily
        // guarantee it if required by testing for the hull which contains
        // the vehicle core (0,0) point inside of it. 
        return paths[0];
    }


    void Awake()
    {
        Hull = new List<Vector2>();
        Attachments = new List<Rigidbody2D>();
        Actuators = new List<ActuatorModule>();
        Modules = new List<VehicleModule>();

        if (!TryGetComponent<Rigidbody2D>(out Rigidbody))
            Debug.LogError("VehicleCore failed to find a Rigidbody2D");

        if(!TryGetComponent<CompositeCollider2D>(out Collider))
        {
            Collider = gameObject.AddComponent<CompositeCollider2D>();
            Collider.geometryType = CompositeCollider2D.GeometryType.Polygons;
        }

        if (Pregenerated)
            Discover();
    }


    void FixedUpdate()
    {
        // Update the vehicle hull
        Hull.Clear();
        for (int i = 0; i < _localHull.Count; i++)
            Hull.Add((Vector2)transform.TransformPoint(_localHull[i]));

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