using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle_AI : MonoBehaviour
{
    public float vehicleTiltAngle;
    public float currentSpeed;
    public bool isGrounded;

    private Rigidbody2D _rb;
    private VehicleCore _core;
    private List<ActuatorModule> _actuator;
    private bool b = true;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _core = GetComponent<VehicleCore>();
    }

    private void Start()
    {
        _actuator = _core.Actuators;
    }

    private void FixedUpdate()
    {
        UpdateVariables();
    }

    private void UpdateVariables()
    {
        vehicleTiltAngle = transform.rotation.eulerAngles.z;
        currentSpeed = _rb.velocity.magnitude;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.name == "Terrain")
        {
            isGrounded = true;
        }
    } 

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.name == "Terrain")
        {
            isGrounded = false;
        }
    }
}
