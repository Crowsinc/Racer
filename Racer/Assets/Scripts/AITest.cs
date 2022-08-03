using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITest : MonoBehaviour
{
    [SerializeField] private Thruster thrust;

    public float vehicleTiltAngle;
    public float currentSpeed;
    public bool isGrounded;

    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        UpdateVariables();
        
        if (isGrounded)
            thrust.EnableThrusters();
        else
            thrust.DisableThrusters();
        
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
