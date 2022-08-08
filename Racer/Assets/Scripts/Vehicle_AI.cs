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

    private List<Rigidbody2D> _wheel;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _core = GetComponent<VehicleCore>();
    }

    private void Start()
    {
        _actuator = _core.Actuators;
        _wheel = _core.Attachments;
        Time.timeScale = 0.1f;
    }

    private void FixedUpdate()
    {
        UpdateVariables();
        _actuator[0].TryActivate();

        
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

    private void OnDrawGizmos()
    {
        float size = 0.25f;
        Debug.DrawLine(
            _rb.worldCenterOfMass + new Vector2(size, 0),
            _rb.worldCenterOfMass + new Vector2(-size, 0),
            Color.red
        );
        Debug.DrawLine(
            _rb.worldCenterOfMass + new Vector2(0, size),
            _rb.worldCenterOfMass + new Vector2(0, -size),
            Color.red
        );
        Debug.DrawLine(
           _rb.worldCenterOfMass + new Vector2(0, size),
           _rb.worldCenterOfMass + new Vector2(size, 0),
           Color.red
       );
        Debug.DrawLine(
           _rb.worldCenterOfMass + new Vector2(-size, 0),
           _rb.worldCenterOfMass + new Vector2(0, -size),
           Color.red
       );


        Vector2 pos = _wheel[1].position;
        Vector2 pos1 = _wheel[0].position;
        var center = Vector2.Lerp(pos, pos1, 0.5f);

        Debug.DrawLine(pos, pos + new Vector2(0, 5), Color.blue);
        Debug.DrawLine(pos1, pos1 + new Vector2(0, 5), Color.blue);
        Debug.DrawLine(_rb.worldCenterOfMass, _rb.worldCenterOfMass + new Vector2(0, 5), Color.green);

    }
}
