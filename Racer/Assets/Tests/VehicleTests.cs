using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine.TextCore.Text;

public class VehicleCoreTests
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
    public void TestValidationFeedback()
    {
        //TODO: test the ValidateVehicle function
    }


    [Test]
    public void TestDisjoint() // WARNING - TAKES A LONG TIME TO RUN
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        //TODO: document this whole function...

        // These flags represent an expected connection point of a module
        // The value increases in a counter clockwise rotation order, so
        // 3 rotations of top is the same as right e.g. t << 3. 
        const uint t = 0b1 << 0; // Top
        const uint l = 0b1 << 1; // Left
        const uint b = 0b1 << 2; // Bottom
        const uint r = 0b1 << 3; // Right
        const uint all = t | b | l | r; // All sides

        List<(GameObject /* Module */, uint /* Connection Points */)> modules = new();
        modules.Add((_chassis, all));
        modules.Add((_glider, b));
        modules.Add((_lChassis, l | r));
        modules.Add((_cone, l));
        modules.Add((_diagonal, b | l));
        modules.Add((_shelf, t | l));
        modules.Add((_tube, l | r));
        modules.Add((_barrel, b));
        modules.Add((_tank, all));
        modules.Add((_flat, b));
        modules.Add((_largeTank, all));
        modules.Add((_largePropeller, l));
        modules.Add((_propeller,l));
        modules.Add((_reaction,r));
        modules.Add((_spring, t));
        modules.Add((_thruster, r));
        modules.Add((_suspension, t));
        modules.Add((_shortSuspension,t));
        modules.Add((_wheel, t));
        modules.Add((_largeWheel,t));
        modules.Add((_sled,t));
        modules.Add((_rocket, b));

        foreach (var (m1, c1) in modules)
        {
            var p1 = m1.GetComponent<VehicleModule>();

            foreach (var (m2, c2) in modules)
            {
                var p2 = m2.GetComponent<VehicleModule>();

                for (int r1 = 0; r1 < 4; r1++)
                {
                    for (int r2 = 0; r2 < 4; r2++)
                    {
                        var rotation1 = Quaternion.Euler(0, 0, r1 * 90);
                        var rotation2 = Quaternion.Euler(0, 0, r2 * 90);

                        Vector2 m1Size = VehicleModule.RotateSize(p1.Size, rotation1);
                        Vector2 m2Size = VehicleModule.RotateSize(p2.Size, rotation2);

                        int m1Radius = (int)Mathf.Ceil((Mathf.Abs(m1Size.y) - 1.0f) / 2.0f);
                        int m2Radius = (int)Mathf.Ceil((Mathf.Abs(m2Size.y) - 1.0f) / 2.0f);

                        // Its possible the modules may only connect at a specific y-height
                        // so test every possible y offset, to make sure at least one works.
                        bool m1Tested = false, m2Tested = false;
                        for (int dy1 = -m1Radius; dy1 <= m1Radius; dy1++)
                        {
                            for (int dy2 = -m2Radius; dy2 <= m2Radius; dy2++)
                            {
                                Dictionary<Vector2Int, ModuleSchematic> design = new();

                                var m1Pos = new Vector2Int(-Mathf.Abs((int)m1Size.x), dy1);
                                var m2Pos = new Vector2Int(m1Pos.x - Mathf.Abs((int)m2Size.x), dy2);

                                design.Add(
                                    m1Pos,
                                    new ModuleSchematic(m1, rotation1.eulerAngles.z)
                                );

                                design.Add(
                                    m2Pos,
                                    new ModuleSchematic(m2, rotation2.eulerAngles.z)
                                );

                                var feedback = core.ValidateDesign(design);
                                foreach (var jointPos in feedback.ValidModules)
                                {
                                    if (jointPos == m1Pos)
                                        m1Tested |= true;
                                    if (jointPos == m2Pos)
                                        m2Tested |= true;
                                }

                                if (m1Tested && m2Tested)
                                    break;
                            }
                            if (m1Tested && m2Tested)
                                break;
                        }

                        // Rotate the connector flags to get the correct connection points
                        var rc1 = (c1 << r1) | (c1 >> (4 - r1));
                        var rc2 = (c2 << r2) | (c2 >> (4 - r2));

                        // M1 should be joint if it has a right connection point
                        // M2 should be joint if it has a right connection point and M1 is joint with a left connection point
                        bool m1Expected = (rc1 & r) != 0;
                        bool m2Expected = ((rc2 & r) != 0) && ((rc1 & l) != 0) && m1Expected;

                        Assert.IsTrue(m1Expected == m1Tested, $"{p1.Name} ({r1 * 90} deg) failed to connect to the vehicle as expected");
                        Assert.IsTrue(m2Expected == m2Tested, $"{p2.Name} ({r2 * 90} deg) failed to connect to {p1.Name} ({r1 * 90} deg) as expected");
                    }
                }
            }
        }
    }


    [Test]
    public void TestEnergyCapacity()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Test all energy storage modules
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(-1, 0), new ModuleSchematic(_tank));
        design.Add(new Vector2Int(-3, 0), new ModuleSchematic(_largeTank));
        design.Add(new Vector2Int(-1, 1), new ModuleSchematic(_barrel));
        design.Add(new Vector2Int(-3, 2), new ModuleSchematic(_flat));

        float expected = core.GetComponent<VehicleModule>().EnergyCapacity
                       + _tank.GetComponent<VehicleModule>().EnergyCapacity
                       + _largeTank.GetComponent<VehicleModule>().EnergyCapacity
                       + _barrel.GetComponent<VehicleModule>().EnergyCapacity
                       + _flat.GetComponent<VehicleModule>().EnergyCapacity;

        var feedback = core.ValidateDesign(design);
        Assert.IsTrue(feedback.TotalEnergyCapacity == expected);
    }


    [Test]
    public void TestMassProperties()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // To test whether the mass and centre of mass is correctly calculated, 
        // we will create a complex, but symmetrical design. The symmetry should
        // cause the centre of mass to stay centered on the vehicle core. 
        //
        // Design:
        // X = core
        // TT = standard tank
        // SS = large suspension wheel
        // LL = sled
        //
        //       S
        //    LL SL
        //   SSTTTL
        //     TXT
        //    LTTTSS
        //    LS LL
        //     S

        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int( 0, 1), new ModuleSchematic(_tank));
        design.Add(new Vector2Int( 1,-1), new ModuleSchematic(_tank, 90));
        design.Add(new Vector2Int(-1,-1), new ModuleSchematic(_tank));
        design.Add(new Vector2Int(-1, 0), new ModuleSchematic(_tank, 90));
        
        design.Add(new Vector2Int( 1, 3), new ModuleSchematic(_suspension, 180));
        design.Add(new Vector2Int( 2,-1), new ModuleSchematic(_suspension, 90));
        design.Add(new Vector2Int(-1,-3), new ModuleSchematic(_suspension));
        design.Add(new Vector2Int(-3, 1), new ModuleSchematic(_suspension, 270));

        design.Add(new Vector2Int(-2, 2), new ModuleSchematic(_sled, 180));
        design.Add(new Vector2Int( 2, 1), new ModuleSchematic(_sled, 90));
        design.Add(new Vector2Int( 1,-2), new ModuleSchematic(_sled));
        design.Add(new Vector2Int(-2,-2), new ModuleSchematic(_sled, 270));

        var feedback = core.ValidateDesign(design);

        var expectedCOM = new Vector2(0.5f, 0.5f); // The vehicle core
        var expectedMass = core.GetComponent<VehicleModule>().Mass
                    + 4 * _tank.GetComponent<VehicleModule>().Mass
                    + 4 * _sled.GetComponent<VehicleModule>().Mass
                    + 4 * _suspension.GetComponent<VehicleModule>().Mass;

        Assert.IsTrue(feedback.TotalMass == expectedMass);
        Assert.IsTrue(Mathf.Abs((feedback.LocalCentreOfMass - new Vector2(0.5f, 0.5f)).magnitude) < 0.1f);
    }


    [Test]
    public void TestHull()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Make the following vehicle design and test that the hull follows the expected 
        // outer shape, and the other disjoint sections are ignored. 
        //
        // Design:
        // x = core
        // b = chassis block
        // d = diagonal
        //
        //          bb
        //   dbd    bb
        //   b b    
        //   dbx
        //
        //   b
        //  bbb
        //   b
        
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(0, 1), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(0, 2), new ModuleSchematic(_diagonal));
        design.Add(new Vector2Int(-1, 2), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-2, 2), new ModuleSchematic(_diagonal, 90));
        design.Add(new Vector2Int(-2, 1), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-2, 0), new ModuleSchematic(_diagonal, 180));
        design.Add(new Vector2Int(-1, 0), new ModuleSchematic(_chassis));

        design.Add(new Vector2Int(5, 4), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(5, 3), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(6, 4), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(6, 3), new ModuleSchematic(_chassis));

        design.Add(new Vector2Int(-4, -1), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-4, -2), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-4, -3), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-5, -2), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-6, -2), new ModuleSchematic(_chassis));

        // Test that hull has the expected shape
        bool disjoint = !core.TryBuildStructure(design, false);
        Assert.IsTrue(disjoint);

        // Test vertices
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2( 1.0f, 0.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2( 1.0f, 2.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2( 0.0f, 3.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2(-1.0f, 3.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2(-2.0f, 2.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2(-2.0f, 1.0f), core.LocalHull));
        Assert.IsTrue(Algorithms.PointInPolygon(new Vector2(-1.0f, 0.0f), core.LocalHull));
    }


    [Test]
    public void TestAerodynamics()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Test that the 






        //TODO:...
    }

    [Test]
    public void TestPregenerated()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);







        //TODO:...
    }


    [Test]
    public void TestLineOfSight()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);







        //TODO:...
    }

    [Test]
    public void TestActuatorDynamics()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);







        //TODO:...
    }


    [Test]
    public void TestActuatorActivation()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);







        //TODO:...
    }

}
