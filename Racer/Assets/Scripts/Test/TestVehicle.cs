using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVehicle : MonoBehaviour
{
    public GameObject TempChassisPrefab;
    public GameObject TempThrusterPrefab;
    public GameObject TempWheelPrefab;

    private VehicleCore _core;

    void Start()
    {
        if (!TryGetComponent<VehicleCore>(out _core))
            Debug.LogError("Vehicle does not have a core");

        var design = new Dictionary<Vector2Int, GameObject>();
        design.Add(new Vector2Int(-1, 0), TempChassisPrefab);
        design.Add(new Vector2Int(-2, 0), TempChassisPrefab);
        design.Add(new Vector2Int(-2, -1), TempChassisPrefab);
        design.Add(new Vector2Int(-3, 0), TempThrusterPrefab);
        design.Add(new Vector2Int(0, -1), TempWheelPrefab);
        design.Add(new Vector2Int(-2, -2), TempWheelPrefab);

        if (!_core.TryBuildStructure(design))
            Debug.LogError("Vehicle design was invalid");
    }

    void FixedUpdate()
    {
        if(Input.GetMouseButton(0))
        {
            Debug.Log($"Energy Left: {_core.EnergyLevel}/{_core.EnergyCapacity}");
            int i = 0;
            foreach (var a in _core.Actuators)
            {
                if(a.TryActivate())
                {
                    Debug.DrawLine(a.ActuationForcePosition, a.ActuationForcePosition + 0.1f * a.ActuationForce, Color.green, 0.01f);
                    Debug.Log($"Actuator: {i} \t Linear Acceleration: {a.LinearAcceleration} \t Angular Acceleration: {a.AngularAcceleration}");
                }
                i++;
            }
        }
    }
}
