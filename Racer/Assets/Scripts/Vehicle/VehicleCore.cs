using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCore : MonoBehaviour
{
    public GameObject ChassisPrefab; // Temp

    // Vehicle Properties
    public float EnergyCapacity { get; private set; }
    public float EnergyLevel { get; private set; }

    // Vehicle Shape
    private Rigidbody2D Rigidbody;
    private CompositeCollider2D Collider;
    bool TryBuildStructure(Dictionary<Vector2Int /* module offset */, GameObject /* module prefab */> design)
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

            // Register VehicleModule to the VehicleCore
            if(module.TryGetComponent<VehicleModule>(out VehicleModule properties))
            {
                totalEnergyCapacity += properties.EnergyCapacity;
                centreOfMass += properties.Mass * (Vector2)offset;
                totalMass += properties.Mass;

                // If the module has a collider, then absorb it into the vehicle core
                if (properties.Collider != null)
                {
                    // Get the collider vertices relative to the VehicleCore, taking  
                    // into account any local offsets of the collider within the prefab
                    var localColliderOffset = properties.Collider.transform.position - position;
                    var localPoints = properties.Collider.points;
                    for (int i = 0; i < localPoints.Length; i++)
                        localPoints[i] += offset + (Vector2)localColliderOffset;

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

                // If the module has its own Rigidbody2D, then we need to attach it to the
                // vehicle core with a joint, so that it acts in union with the vehicle. 
                if (properties.Rigidbody != null && properties.Rigidbody != Rigidbody)
                {
                    var joint = properties.Rigidbody.gameObject.AddComponent<RelativeJoint2D>();
                    joint.connectedBody = Rigidbody;
                    joint.linearOffset = offset;
                }
            }
            else Debug.LogError($"Vehicle module at {offset} has no VehicleModule component");

            // TODO: ActuatorModule
        }
        
        // Merge all our module colliders together into one composite collider
        Collider = gameObject.AddComponent<CompositeCollider2D>();
        Collider.geometryType = CompositeCollider2D.GeometryType.Polygons;
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

        return true;
    }

    void ClearStructure()
    {
        // Delete vehicle modules
        for(int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<VehicleModule>() != null)
                Destroy(child.gameObject);
        }
 
        // Clean up all colliders
        if(Collider != null)
        {
            var colliders = new List<PolygonCollider2D>();
            GetComponents<PolygonCollider2D>(colliders);
            for (int i = 0; i < colliders.Count; i++)
                if (colliders[i].usedByComposite && colliders[i].composite == Collider)
                    Destroy(colliders[i]);

            Destroy(Collider);
            Collider = null;
        }

        // Reset vehicle properties
        EnergyCapacity = 0;
        Rigidbody.mass = 1;
        Rigidbody.centerOfMass = Vector3.zero;
    }

    List<Vector2> GetHull()
    {
        var path = new List<Vector2>();
        Collider.GetPath(0, path);

        // The path is local, so we need to transform it to the vehicle's transform
        for(int i = 0; i < path.Count; i++)
        {
            var worldPoint = transform.position + transform.rotation * path[i];
            path[i] = new Vector2(worldPoint.x, worldPoint.y);
        }

        return path;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!TryGetComponent<Rigidbody2D>(out Rigidbody))
            Debug.LogError("VehicleCore failed to find a Rigidbody2D");


        // TEMP
        var design = new Dictionary<Vector2Int, GameObject>();
        design.Add(new Vector2Int(-1, 0), ChassisPrefab);
        design.Add(new Vector2Int(-2, 0), ChassisPrefab);
        design.Add(new Vector2Int(-2, -1), ChassisPrefab);
        for (int i = 0; i < 7; i ++)
        {
            design.Add(new Vector2Int(i, i), ChassisPrefab);
            design.Add(new Vector2Int(i + 1, i), ChassisPrefab);
        }

        TryBuildStructure(design);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        // Draw the vehicle hull for debug purposes
        if(Collider != null)
        {
            List<Vector2> hull = GetHull();

            Vector2 prev = hull[^1];
            foreach(var point in hull)
            {
                Debug.DrawLine(prev, point, Color.red);
                prev = point;
            }
        }
    }
}
