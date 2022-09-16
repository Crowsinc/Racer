using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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


    /// <summary>
    /// A counter-clockwise list of vertices outlining shape of the vehicle's collider in local coordinates.
    /// </summary>
    public List<Vector2> LocalHull { get; private set; }


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

    public struct DesignFeedback
    {
        public float TotalEnergyCapacity;

        public float TotalMass;
        public Vector2 LocalCentreOfMass; 

        public bool ValidDesign; // True if the design is valid, otherwise false
        public List<Vector2Int> ValidModules; // Design offset of valid modules
        public List<Vector2Int> DisjointModules; // Design offset of disjoint modules
    };


    /// <summary>
    /// Validates the given design of the vehicle, returning feedback statistics.
    /// </summary>
    /// <param name="design">
    /// A dictionary of ModuleSchematics, describing a prefab that is to be placed as a vehicle module.
    /// Each ModuleSchematic is keyed by its positional offset from the VehicleCore. All offsets are taken
    /// from the bottom left of the set of cells onto which the prefab will be placed, regardless of its 
    /// rotation. The offset (0,0) is reseved for the vehicle core and will be ignored. 
    /// </param>
    /// <returns> 
    /// Feedback statistics on the vehicle design. 
    /// </returns>
    public DesignFeedback ValidateDesign(Dictionary<Vector2Int, ModuleSchematic> design)
    {
        // To validate our design we will just build a vehicle off-screen, then analyse it
        var testCore = Instantiate<VehicleCore>(
            GetComponent<VehicleCore>(),
            new Vector3(-10000f, -10000f, -10000),
            Quaternion.identity
        );
        testCore.Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        var feedback = new DesignFeedback
        {
            ValidModules = new List<Vector2Int>(),
            DisjointModules = new List<Vector2Int>(),
            ValidDesign = testCore.TryBuildStructure(design, false),
            TotalMass = testCore.Rigidbody.mass,
            LocalCentreOfMass = testCore.Rigidbody.centerOfMass,
            TotalEnergyCapacity = testCore.EnergyCapacity
        };

        // Analyse individual modules
        var modules = testCore.gameObject.GetComponentsInChildren<VehicleModule>();
        foreach (var module in modules)
        {
            // Get the offset to the bottom left cell of the module. If the rotated size
            // of the module is negative, then it means that the origin has been shifted
            // from the bottom left, so we need to shift it back
            var rotatedSize = VehicleModule.RotateSize(module.Size, module.transform.rotation);
            Vector3 originOffset = new Vector2(
                Mathf.Min(rotatedSize.x, 0),
                Mathf.Min(rotatedSize.y, 0)
            );

            var offset = (module.transform.position + originOffset) - testCore.transform.position;
            var gridOffset = new Vector2Int((int)offset.x, (int)offset.y);

            // If the module does not have a collider or is the core, then ignore it
            if (module.Collider == null || gridOffset == new Vector2Int(0,0))
                continue;

            module.Collider.enabled = true;

            // We consider a module to be disjoint if it is not found within the hull polygon.
            // To test this, we will grab a point from the center of the module and perform
            // a point in polygon test with the hull. It is possible that some colliders may
            // have a center point which is outside the collider, so intead we grab the colliders
            // closest point to the center. Additionally, we will test all points on the module's
            // collider for extra robustness. If at least one of these is inside, then the module
            // is not disjoint. 
            var testPoint = module.Collider.ClosestPoint(module.Collider.bounds.center);
            bool inside = Algorithms.PointInPolygon(testPoint, testCore.Hull);
 

            if (inside)
                feedback.ValidModules.Add(gridOffset);
            else
                feedback.DisjointModules.Add(gridOffset);
        }

        Destroy(testCore.gameObject);

        return feedback;
    }

    /// <summary>
    /// Generates the given design of the vehicle onto the VehicleCore.
    /// </summary>
    /// <param name="design">
    /// A dictionary of ModuleSchematics, describing a prefab that is to be placed as a vehicle module.
    /// Each ModuleSchematic is keyed by its positional offset from the VehicleCore. All offsets are taken
    /// from the bottom left of the set of cells onto which the prefab will be placed, regardless of its 
    /// rotation. The offset (0,0) is reseved for the vehicle core and will be ignored. 
    /// </param>
    /// <param name="clearOnFail">
    /// If set to true, the vehicle state and structure will cleared if building fails to pass validation. 
    /// Otherwise the failed structure is left behind and must be cleared manually with ClearStructure()
    /// </param>
    /// <returns> 
    /// False if the provided design has an invalid hull, otherwise true.
    /// A hull is considered invalid if it isn't fully connected or has holes. 
    /// </returns>
    public bool TryBuildStructure(Dictionary<Vector2Int, ModuleSchematic> design, bool clearOnFail = true)
    {
        // Add ourselves to a copy of the design so our module properties are taken into account
        design = new Dictionary<Vector2Int, ModuleSchematic>(design); 
        design[new Vector2Int(0, 0)] = new ModuleSchematic(gameObject);

        ClearStructure();

        float totalMass = 0.0f;
        float totalEnergyCapacity = 0.0f;
        Vector2 centreOfMass = Vector2.zero;
        foreach (var (offset, (prefab, rotation)) in design)
        {
            var position = transform.position + new Vector3(offset.x, offset.y);
            var rotationQuat = Quaternion.Euler(0, 0, rotation);

            // Instantiate new vehicle module, unless this is the core
            var instance = prefab == gameObject ? gameObject
                : Instantiate(prefab, position, rotationQuat, transform);

            // Register VehicleModule to the vehicle
            if (instance.TryGetComponent<VehicleModule>(out VehicleModule module))
            {
                // The provided offset was to the bottom left corner of cells in which the prefab will recide. 
                // While each prefab's origin is in the bottom left, this no longer holds for rotated modules. 
                // For example, a module that is rotated 180 degrees will have its origin in the top right, 
                // however the provided offset is still for the bottom left. If the rotated size of the module
                // is negative, this means that the origin point switched to the opposite side of the module's
                // cells, so we should add the size back as an offset
                var rotatedSize = VehicleModule.RotateSize(module.Size, rotationQuat);
                Vector3 originOffset = new Vector2(
                    Mathf.Abs(Mathf.Min(rotatedSize.x, 0)),
                    Mathf.Abs(Mathf.Min(rotatedSize.y, 0))
                );
                instance.transform.position += originOffset;


                totalEnergyCapacity += module.EnergyCapacity;
                totalMass += module.Mass;
            
                // We take the objects mass from the centroid of all colliders.
                // NOTE: the origin offset hasn't had time to propogate to the colliders
                // yet, so we need to add it ourselves to the centroids. 
                Collider2D[] colliders = {module.Collider};
                if(module.Attachments.Count > 0)
                    colliders = module.gameObject.GetComponentsInChildren<Collider2D>();

                if (colliders.Length > 0)
                {
                    Vector2 moduleCentre = Vector2.zero;
                    foreach (var c in colliders)
                        moduleCentre += (Vector2)(c.bounds.center + originOffset);
                    moduleCentre /= colliders.Length;

                    centreOfMass += module.Mass * (moduleCentre - (Vector2)transform.position);
                }

                RegisterModule(module);
            }
            else Debug.LogError($"Vehicle module at {offset} has no VehicleModule component");

            // Register ActuatorModules to the vehicle
            if (instance.TryGetComponent<ActuatorModule>(out ActuatorModule actuator))
                Actuators.Add(actuator);
        }

        // Merge all our module colliders together into one composite collider
        Collider.GenerateGeometry();
        LocalHull = DetectHull(out bool disjoint);
        UpdateHull();

        // Set physical properties of the Vehicle
        EnergyCapacity = totalEnergyCapacity;
        Rigidbody.mass = totalMass;
        Rigidbody.centerOfMass = centreOfMass / totalMass;

        // Link all actuators to the vehicle, this must happen after
        // all of the vehicles physical properties have been discovered.
        foreach (var actuator in Actuators)
            actuator.LinkedVehicle = this;

        IsBuilt = true;
        ResetVehicle();

        // Validate our vehicles hull to make sure it will function as expected.
        // NOTE: any further validation tests should be evaluated here
        var validated = !disjoint;

        // If the validation fails and clearOnFail is set, then we will clear and 'unbuild' the vehicle
        if (!validated && clearOnFail)
            ClearStructure();

        return validated;
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
        LocalHull = DetectHull(out bool disjoint);
        UpdateHull();

        // Validate our vehicles hull to make sure
        if (disjoint)
        {
            Debug.LogError($"Pregenerated Vehicle hull is disjoint");
            ClearStructure();
            return;
        }

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
        LocalHull?.Clear();
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
    private void RegisterModule(VehicleModule module)
    {
        Vector2 gridOffset = module.transform.position - transform.position;

        // If the module has a collider, then absorb it into the vehicle core
        if (module.Collider != null)
        {
            // Get the collider vertices relative to the VehicleCore, taking  
            // into account any transforms and local offsets of the collider
            Vector2[] localPoints = module.Collider.points;
            for (int i = 0; i < localPoints.Length; i++)
            {
                // Scale the collider points
                localPoints[i] *= module.Collider.transform.localScale;

                // Add local collider offset
                localPoints[i] += module.Collider.offset;
                
                // Transform point to world space, then obtain it relative to the vehicle core
                localPoints[i] = module.transform.TransformPoint(localPoints[i]) - transform.position;
                
                // Add transform offset between the module, and the module child that the collider is placed on
                localPoints[i] += (Vector2)(module.Collider.transform.position - module.transform.position);
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
            Debug.LogWarning($"Vehicle module at {gridOffset} has no PolygonCollider2D");

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
                joint.connectedAnchor = (gridOffset + localBodyOffset) + (Vector2)module.transform.TransformDirection(joint.connectedAnchor);
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
            // detection

            if (area > minColliderArea)
                paths.Add(path);
            else
                Debug.LogWarning("A bad module collider was filtered out for hull detection");
        }

        // Remove all paths representing holes within a hull defined by another path.
        // We can determine if one path is inside another, if all of its points are
        // inside the (potentially convex) polygon formed by another path. Similarly,
        // we can determine if one path is outside of another if all of its points 
        // are outside of this polygon. 

        for (int p1 = 0; p1 < paths.Count; p1++)
        {
            for (int p2 = 0; p2 < paths.Count; p2++)
            {
                if (p1 == p2) continue;

                bool inside = true;
                var polygonPath = paths[p1];
                foreach(var testPoint in paths[p2])
                    inside &= Algorithms.PointInPolygon(testPoint, polygonPath);
                
                // If all points in polygon p2 are inside p2, remove p2
                if(inside)
                {
                    paths.RemoveAt(p2--);
                    if (p2 < p1)
                        p1--;
                }
            }
        }

        // The hull is disjoint if we end up with more than one path here.
        disjoint = paths.Count > 1;
        if(disjoint)
        {
            // If the hull is disjoint, then we need to find
            // the path that contains the vehicle core, as this
            // will be the main hull. This is in local space, so
            // the center of the core should be the point (0.5,0.5)
            var corePosition = new Vector2(0.5f, 0.5f);
            for (int p = 0; p < paths.Count; p++)
                if (Algorithms.PointInPolygon(corePosition, paths[p]))
                    return paths[p];

            Debug.LogError("Failed to find disjoint hull!");
        }

        // If the hull isnt disjoint than paths[0] must contain the hull.
        return paths[0];
    }

    private void UpdateHull()
    {
        Hull.Clear();
        for (int i = 0; i < LocalHull.Count; i++)
            Hull.Add((Vector2)transform.TransformPoint(LocalHull[i]));
    }

    void Awake()
    {
        Hull = new List<Vector2>();
        LocalHull = new List<Vector2>();
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
        if(IsBuilt)
        {
            // Update the vehicle hull
            UpdateHull();

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

                // The amount of drag area applied will scale based on how parallel the normal of
                // the segment is to the velocity direction. If they are parallel (angle = 0), then
                // the full drag area is added. If they are perpendicular or an obtuse angle, then
                // zero drag area is added. 
                dragArea += segment.magnitude * (1.0f - Mathf.Clamp01(Vector2.Angle(outerNormal, velocityDir) / 90.0f));

                prevPoint = currPoint;

                Debug.DrawRay(currPoint, segment, Color.yellow);
            }

            // NOTE: we invert the force direction here, to make it drag (-0.5f)
            var aerodynamicDragForce = -0.5f * AerodynamicDragCoefficient * dragArea * velocitySqr;

            Rigidbody.drag = 0.0f;
            Rigidbody.AddForce(aerodynamicDragForce);

        }
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