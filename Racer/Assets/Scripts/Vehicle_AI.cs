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

    private Rigidbody2D _leftTippingPoint;
    private Rigidbody2D _rightTippingPoint;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _core = GetComponent<VehicleCore>();
    }

    private void Start()
    {
        _actuator = _core.Actuators;
        _leftTippingPoint = _core.Attachments[1];
        _rightTippingPoint = _core.Attachments[0];
        Time.timeScale = 0.4f;
    }

    private void FixedUpdate()
    {
        var center = Vector2.Lerp(_leftTippingPoint.position, _rightTippingPoint.position, 0.5f).x;
        var counterClockwiseForce = (_rb.worldCenterOfMass.x - _leftTippingPoint.position.x) / (center - _leftTippingPoint.position.x);
        var clockwiseForce = ( _rightTippingPoint.position.x - _rb.worldCenterOfMass.x) / (_rightTippingPoint.position.x - center);
        if (clockwiseForce >= 1)
            clockwiseForce = 1.0f;
        else if (clockwiseForce <= 0)
            clockwiseForce = 0.0f;
        Debug.Log(clockwiseForce);
        _actuator[0].TryActivate(proportion: clockwiseForce);

    }

    private void OnDrawGizmos()
    {
        float size = 0.25f;
        if (_rb == null) return;

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


        Vector2 pos = _leftTippingPoint.position;
        Vector2 pos1 = _rightTippingPoint.position;

        Debug.DrawLine(pos, pos + new Vector2(0, 5), Color.blue);
        Debug.DrawLine(pos1, pos1 + new Vector2(0, 5), Color.blue);

    }
}
