using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVehicle : MonoBehaviour
{
    public GameObject ChassisPrefab;
    public GameObject HeavyChassisPrefab;
    public GameObject EnergyTankPrefab;
    public GameObject PropellerPrefab;
    public GameObject ThrusterPrefab;
    public GameObject JetPrefab;
    public GameObject SolidWheelPrefab;
    public GameObject SuspensionWheelPrefab;

    private VehicleCore _core;

    void Start()
    {
        if (_core == null && !TryGetComponent<VehicleCore>(out _core))
            Debug.LogError("Vehicle does not have a core");

        if (!_core.IsBuilt)
            Build();
    }

    public void Build()
    {
        if (!TryGetComponent<VehicleCore>(out _core))
            Debug.LogError("Vehicle does not have a core");

        var design = new Dictionary<Vector2Int, ModuleSchematic>();
        design.Add(new Vector2Int(-1, 0), new ModuleSchematic(HeavyChassisPrefab));
        design.Add(new Vector2Int(-2, 0), new ModuleSchematic(EnergyTankPrefab));
        design.Add(new Vector2Int(-3, 0), new ModuleSchematic(ChassisPrefab));
        design.Add(new Vector2Int(-4, 0), new ModuleSchematic(ChassisPrefab));
        design.Add(new Vector2Int(-1, 1), new ModuleSchematic(JetPrefab));
        design.Add(new Vector2Int(-5, 0), new ModuleSchematic(ThrusterPrefab));
        design.Add(new Vector2Int(-2, -1), new ModuleSchematic(SuspensionWheelPrefab));
        design.Add(new Vector2Int(0, -1), new ModuleSchematic(SuspensionWheelPrefab));
        design.Add(new Vector2Int(-4, 1), new ModuleSchematic(PropellerPrefab, 90));
        design.Add(new Vector2Int(1, 0), new ModuleSchematic(SolidWheelPrefab, 90));
        design.Add(new Vector2Int(-4, -1), new ModuleSchematic(SolidWheelPrefab));

        if (!_core.TryBuildStructure(design))
            Debug.LogError("Vehicle design was invalid");
    }

    void FixedUpdate()
    {
        
        if(Input.GetMouseButton(1)) // right click pressed
        {
            Debug.Log($"Energy Left: {_core.EnergyLevel}/{_core.EnergyCapacity}");
            foreach (var a in _core.Actuators)
                if (a.TryActivate(1.0f, true))
                    Debug.DrawLine(a.ActuationForcePosition, a.ActuationForcePosition + a.ActuationForce * 0.1f, Color.green);
        }

        // Draw the vehicle hull for debug purposes
        if (_core.Hull?.Count > 0)
        {
            Vector2 prev = _core.Hull[^1];
            foreach (var point in _core.Hull)
            {
                Debug.DrawLine(prev, point, Color.magenta);
                prev = point;
            }
        }

        // Draw a cross at the centre of gravity
        if (_core.Rigidbody != null)
        {
            float size = 0.25f;
            Debug.DrawLine(
                _core.Rigidbody.worldCenterOfMass + new Vector2(size, 0),
                _core.Rigidbody.worldCenterOfMass + new Vector2(-size, 0),
                Color.red
            );
            Debug.DrawLine(
                _core.Rigidbody.worldCenterOfMass + new Vector2(0, size),
                _core.Rigidbody.worldCenterOfMass + new Vector2(0, -size),
                Color.red
            );
            Debug.DrawLine(
               _core.Rigidbody.worldCenterOfMass + new Vector2(0, size),
               _core.Rigidbody.worldCenterOfMass + new Vector2(size, 0),
               Color.red
           );
            Debug.DrawLine(
               _core.Rigidbody.worldCenterOfMass + new Vector2(-size, 0),
               _core.Rigidbody.worldCenterOfMass + new Vector2(0, -size),
               Color.red
           );
        }
    }
}
