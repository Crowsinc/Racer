using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleModule : MonoBehaviour
{
    /// <summary>
    /// The amount of energy this module can store
    /// </summary>
    public float EnergyCapacity = 0; 


    /// <summary>
    /// The mass (kg) of this module
    /// </summary>
    public float Mass = 1;


    /// <summary>
    /// The polygon collider which describes the physical shape of the module
    /// </summary>
    public PolygonCollider2D Collider;


    /// <summary>
    /// A list of joints which attach external Rigidbodies to the module.
    /// These will be attached to the VehicleCore when the module is added 
    /// to a VehicleCore.
    /// </summary>
    public List<Joint2D> Attachments = new List<Joint2D>();


    void OnValidate()
    {
        Mass = Mathf.Max(Mass, 1.0f);
        EnergyCapacity = Mathf.Max(EnergyCapacity, 0.0f);
    }
}
