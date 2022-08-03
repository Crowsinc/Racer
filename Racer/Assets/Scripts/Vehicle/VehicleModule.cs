using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleModule : MonoBehaviour
{
    // Module Statistics
    public float EnergyCapacity = 0; 
    public float Mass = 1;

    // Shape Descriptors (optional)
    public PolygonCollider2D Collider;
    public Rigidbody2D Rigidbody;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {
        Mass = Mathf.Max(Mass, 1.0f);
        EnergyCapacity = Mathf.Max(EnergyCapacity, 0.0f);
    }
}
