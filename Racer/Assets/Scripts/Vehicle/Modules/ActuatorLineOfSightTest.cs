using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActuatorLineOfSightTest : MonoBehaviour
{
    ActuatorModule _actuator;

    // Raycast properties
    const int _distance = 20;
    int _vehicleLayer = 0;
    int _moduleLayer = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        _actuator = GetComponent<ActuatorModule>();
        _vehicleLayer = LayerMask.NameToLayer("Vehicle");
        _moduleLayer = LayerMask.NameToLayer("Module");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_actuator != null)
            _actuator.Disabled = TestBlocked();
    }

    /// <summary>
    /// Tests if the force of an actuator is blocked by another vehicle part
    /// </summary>
    /// <returns> True if blocked, otherwise false </returns>
    public bool TestBlocked()
    {
        // Check if another module is blocking force of this actuator
        LayerMask mask;
        if (gameObject.layer == _vehicleLayer || gameObject.layer == _moduleLayer)
            mask = LayerMask.GetMask("Vehicle", "Module");
        else
            mask = LayerMask.GetMask("Opponent Vehicle", "Opponent Module");

        // Offset the actuation force to avoid starting the force within a collider
        var forceDir = _actuator.ActuationForce.normalized;
        var colliderOffset = forceDir * 0.2f;
        RaycastHit2D hit = Physics2D.Raycast(
            _actuator.ActuationPosition + colliderOffset,
            forceDir,
            _distance,
            mask
        );

        Debug.DrawRay(_actuator.ActuationPosition + colliderOffset, _actuator.ActuationForce, hit.collider != null ? Color.red : Color.green);

        // It is blocked if there is a collider, otherwise it is free
        return hit.collider != null;
    }
}
