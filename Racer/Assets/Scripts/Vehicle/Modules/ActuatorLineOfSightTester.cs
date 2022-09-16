using Assets.Scripts.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActuatorLineOfSightTester : MonoBehaviour
{
    ActuatorModule _actuator;

    // Raycast properties
    const int _distance = 20;
    int _ignoreRaycastLayer = 0;
    int _vehicleLayer = 0;
    int _moduleLayer = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        _actuator = GetComponent<ActuatorModule>();
        _vehicleLayer = LayerMask.NameToLayer("Vehicle");
        _moduleLayer = LayerMask.NameToLayer("Module");
        _ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_actuator != null)
            _actuator.Disabled = !TestLineOfSight();
    }

    /// <summary>
    /// Tests if the force of an actuator has free line of sight or is blocked by another vehicle part
    /// </summary>
    /// <returns> True if it has line of sight, otherwise false </returns>
    public bool TestLineOfSight()
    {
        // Check if another module is blocking force of this actuator
        LayerMask mask;
        if (gameObject.layer == _vehicleLayer || gameObject.layer == _moduleLayer)
            mask = LayerMask.GetMask("Vehicle", "Module");
        else
            mask = LayerMask.GetMask("Opponent Vehicle", "Opponent Module");

        // Temporarily move the module onto a different layer so it avoids raycasting itself
        var orgLayer = gameObject.layer;
        Algorithms.SetLayers(gameObject, _ignoreRaycastLayer);

        // Raycast along the actuator's force line to test if any modules are in the way
        RaycastHit2D hit = Physics2D.Raycast(
            _actuator.ActuationPosition,
            _actuator.ActuationForce,
            _distance,
            mask
        );

        Algorithms.SetLayers(gameObject, orgLayer);

        Debug.DrawRay(_actuator.ActuationPosition, _actuator.ActuationForce, hit.collider != null ? Color.red : Color.green);

        // It is blocked if there is a collider
        return hit.collider == null;
    }
}
