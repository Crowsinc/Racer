using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleModule : MonoBehaviour
{
    /// <summary>
    /// Name of the module
    /// </summary>
    public string Name;

    /// <summary>
    /// The cost of buying this module
    /// </summary>
    public float Cost = 0;


    /// <summary>
    /// The amount of energy this module can store
    /// </summary>
    public float EnergyCapacity = 0;


    /// <summary>
    /// The mass (kg) of this module
    /// </summary>
    public float Mass = 1;


    /// <summary>
    /// The cell size (width, height) of this module.
    /// </summary>
    public Vector2 Size = Vector2.one;


    /// <summary>
    /// An optional predicate function for validating the placement of this module onto 
    /// a vehicle building grid using per module restrictions. For example, a jet 
    /// module could be made which can only be placed if nothing is placed infront
    /// or behind it. Such restrictions would need to be re-checked any time the grid
    /// is going to be updated. 
    /// </summary>
    /// <param name="grid"> The vehicle builder grid </param>
    /// <param name="position"> The cell coordinate onto which this module is going to be placed</param>
    /// <returns> True if the module can be placed, false otherwise. </returns>
    public delegate bool ValidatePlacement(GameObject[,] grid, Vector2Int position);


    /// <summary>
    /// The polygon collider which describes the physical shape of the module
    /// </summary>
    public PolygonCollider2D Collider;


    /// <summary>
    /// A list of anchored joints which attach external Rigidbodies to the module.
    /// These will be attached to the VehicleCore when the module is added 
    /// to a VehicleCore.
    /// </summary>
    public List<AnchoredJoint2D> Attachments = new List<AnchoredJoint2D>();


    /// <summary>
    /// The vehicle that this module is linked/attached to
    /// </summary>
    [HideInInspector]
    public VehicleCore LinkedVehicle = null;


    /// <summary>
    /// Turns the module into a static elements by disabling all joints and bodies
    /// </summary>
    public void Freeze()
    {
        var joints = GetComponentsInChildren<Joint2D>();
        foreach (var j in joints)
            j.enabled = false;

        var bodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (var b in bodies)
            b.constraints = RigidbodyConstraints2D.FreezeAll;
    }


    void OnValidate()
    {
        Mass = Mathf.Max(Mass, 1.0f);
        EnergyCapacity = Mathf.Max(EnergyCapacity, 0.0f);
    }
}
