using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine.TextCore.Text;
using UnityEngine.Assertions.Must;

public class ActuatorTests
{

    private GameObject _vehicle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Vehicle.prefab");
    
    // Load all module prefabs
    private GameObject _chassis = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Chassis.prefab");
    private GameObject _lChassis = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/LChassis.prefab");
    private GameObject _cone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/ConeChassis.prefab");
    private GameObject _diagonal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/DiagonalChassis.prefab");
    private GameObject _shelf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/ShelfChassis.prefab");
    private GameObject _tube = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/TubeChassis.prefab");
    private GameObject _glider = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Glider.prefab");
    private GameObject _barrel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/EnergyBarrel.prefab");
    private GameObject _tank = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/EnergyTank.prefab");
    private GameObject _flat = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/FlatEnergyTank.prefab");
    private GameObject _largeTank = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/LargeEnergyTank.prefab");
    private GameObject _largePropeller = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/LargePropeller.prefab");
    private GameObject _propeller = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Propeller.prefab");
    private GameObject _reaction = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/ReactionThruster.prefab");
    private GameObject _rocket = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/RocketThruster.prefab");
    private GameObject _spring = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Spring.prefab");
    private GameObject _thruster = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Thruster.prefab");
    private GameObject _suspension = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/SuspensionWheel.prefab");
    private GameObject _shortSuspension = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/ShortSuspensionWheel.prefab");
    private GameObject _wheel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/Wheel.prefab");
    private GameObject _largeWheel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/LargeWheel.prefab");
    private GameObject _sled = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Vehicle/SledWheel.prefab");


    [Test]
    public void TestLineOfSight()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);


        // Create two rockets, one horizontal and one vertical,
        // with no blockage. They should not be disabled by the LOS test. 
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(-1, 1), new ModuleSchematic(_rocket));
        design.Add(new Vector2Int(-5, 4), new ModuleSchematic(_rocket, 90));

        // NOTE: The design is disjoint, but it doesn't matter here
        core.TryBuildStructure(design, false);

        foreach (var actuator in core.Actuators)
        {
            actuator.UpdateDynamics();
            Assert.IsTrue(actuator.GetComponent<ActuatorLineOfSightTester>().TestLineOfSight());
        }

        // Add a chassis module to block both rockets and retest
        design.Add(new Vector2Int(-5, 1), new ModuleSchematic(_chassis));

        core.TryBuildStructure(design, false);

        foreach (var actuator in core.Actuators)
        {
            actuator.UpdateDynamics();
            Assert.IsFalse(actuator.GetComponent<ActuatorLineOfSightTester>().TestLineOfSight());
        }
    }

    //[Test]
    //public void TestActuatorDynamics()
    //{
    // TODO: implement me
    //}


    //[Test]
    //public void TestActuatorActivation()
    //{
    // TODO: implement me
    //}

}
