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

public class VehicleTests
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
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Test that the design validation matches the structure creation
        
        // Create basic disjoint design to test that the validity testing matches
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(-2, 0), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-3, 0), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-5, 0), new ModuleSchematic(_tank));

        var feedback = core.ValidateDesign(design);
        Assert.IsTrue(feedback.ValidDesign == core.TryBuildStructure(design));
        foreach (var disjointModule in feedback.DisjointModules)
            Assert.IsTrue(design.ContainsKey(disjointModule));

        // Make the design valid, then test that all properties match
        design.Add(new Vector2Int(-1,0), new ModuleSchematic(_chassis));

        feedback = core.ValidateDesign(design);
        Assert.IsTrue(feedback.ValidDesign == core.TryBuildStructure(design));
        Assert.IsTrue(feedback.TotalEnergyCapacity == core.EnergyCapacity);
        Assert.IsTrue(feedback.LocalCentreOfMass == core.Rigidbody.centerOfMass);
        Assert.IsTrue(feedback.TotalMass == core.Rigidbody.mass);
        Assert.IsTrue(feedback.DisjointModules.Count == 0);
        foreach (var validModule in feedback.ValidModules)
            Assert.IsTrue(design.ContainsKey(validModule));

        GameObject.Destroy(vehicle);
    }

    [Test]
    public void TestDisjoint() // WARNING - CAN TAKE A LONG TIME TO RUN
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Test the connection points between all modules, while also testing the
        // vehicle core's disjoint module detection. To do this we will create
        // a vehicle with the following design:
        //
        //  MNx   
        //
        // Where x is the vehicle core, N is module 1, and M is module 2. M and N
        // will be cycled out for each possible combination of modules. N is tested
        // for its ability to join to the flat vehicle core block, while M is tested
        // for its ability to join to the left side of the N module. 

        // These flags represent the expected connection points of a module
        // The flags are listed in counter clockwise order, so that a positive
        // 90 degree rotation can be reflected in the bit flags with a left shift.
        // Some extra logic is needed to make sure the rotation is cyclic. 
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

                        // Its possible the modules may only connect at a specific y-height
                        // so test every possible connective y offset, to make sure at least one works.
                        int m1Shift = (int)Mathf.Ceil((Mathf.Abs(m1Size.y) - 1.0f) / 2.0f);
                        int m2Shift = (int)Mathf.Ceil((Mathf.Abs(m2Size.y) - 1.0f) / 2.0f);

                        bool m1Connected = false, m2Connected = false;
                        for (int dy1 = -m1Shift; dy1 <= m1Shift; dy1++)
                        {
                            for (int dy2 = -m2Shift; dy2 <= m2Shift; dy2++)
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
                                        m1Connected |= true;
                                    if (jointPos == m2Pos)
                                        m2Connected |= true;
                                }

                                if (m1Connected && m2Connected)
                                    break;
                            }
                            if (m1Connected && m2Connected)
                                break;
                        }

                        // Rotate the connector flags to get the rotated connection points
                        var rc1 = (c1 << r1) | (c1 >> (4 - r1));
                        var rc2 = (c2 << r2) | (c2 >> (4 - r2));

                        // M1 should be joint if it has a right connection point
                        // M2 should be joint if it has a right connection point and M1 is joint with a left connection point
                        bool m1Expected = (rc1 & r) != 0;
                        bool m2Expected = ((rc2 & r) != 0) && ((rc1 & l) != 0) && m1Expected;

                        Assert.IsTrue(m1Expected == m1Connected, $"{p1.Name} ({r1 * 90} deg) failed to connect to the vehicle as expected");
                        Assert.IsTrue(m2Expected == m2Connected, $"{p2.Name} ({r2 * 90} deg) failed to connect to {p1.Name} ({r1 * 90} deg) as expected");
                    }
                }
            }
        }

        GameObject.Destroy(vehicle);
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

        Assert.IsTrue(core.TryBuildStructure(design));
        Assert.IsTrue(core.EnergyCapacity == expected);
        Assert.IsTrue(core.EnergyLevel == core.EnergyCapacity);

        // Test energy clamping
        core.EnergyLevel = core.EnergyCapacity + 100;
        Assert.IsTrue(core.EnergyLevel == core.EnergyCapacity);
        core.EnergyLevel = -1;
        Assert.IsTrue(core.EnergyLevel == 0);

        // Test vehicle reset
        core.ResetVehicle();
        Assert.IsTrue(core.EnergyLevel == core.EnergyCapacity);

        GameObject.Destroy(vehicle);
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
        Assert.IsTrue(Mathf.Abs((feedback.LocalCentreOfMass - expectedCOM).magnitude) < 0.1f);

        GameObject.Destroy(vehicle);
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

        GameObject.Destroy(vehicle);
    }


    [Test]
    public void TestAerodynamics()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Test the resulting aero dynamic drag of different blocks. The actual values
        // themselves don't matter, we are testing that the logic works as expected.
        // We will test the aerodynamic drag with normalized velocity directions of
        // right, top, and their diagonal mixture; then test that the drags differ
        // in order as expected. We will also test that the coefficient properly
        // acts as a flat multiplier. 

        // Make an L shaped vehicle from a basic set of blocks.
        // - The right and left tests should be equal but opposite,
        // - The top test should have a vertical drag of 1.0
        // - The right test should have a horizontal drag of 1.5 
        // - The right test should be the worst, followed by the digaonal test, followed by the top
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(-1, 0), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-1, 1), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-1, 2), new ModuleSchematic(_chassis));
        Assert.IsTrue(core.TryBuildStructure(design));

        var flatTop = VehicleCore.CalculateAerodynamicDrag(1, Vector2.up, core.Hull);
        var flatRight = VehicleCore.CalculateAerodynamicDrag(1, Vector2.right, core.Hull);
        var flatLeft = VehicleCore.CalculateAerodynamicDrag(1, Vector2.left, core.Hull);
        var flatDiag = VehicleCore.CalculateAerodynamicDrag(1, (Vector2.right + Vector2.up).normalized, core.Hull);

        Assert.IsTrue(flatRight == -flatLeft);
        Assert.IsTrue(flatTop.x == 0 && Mathf.Abs(flatTop.y + 1.0f) < 0.01f);
        Assert.IsTrue(flatRight.y == 0 && Mathf.Abs(flatRight.x + 1.5f) < 0.01f);
        Assert.IsTrue(flatRight.magnitude > flatDiag.magnitude);
        Assert.IsTrue(flatDiag.magnitude > flatTop.magnitude);


        // Test the improved aerodynamics of a diagonal block.
        // - The right test should be lower than the left, due to the diagonal.
        // - The top test should be lower than the top flat test, despite having the same horizontal footprint.
        design.Clear();
        design.Add(new Vector2Int(1, 0), new ModuleSchematic(_diagonal));
        Assert.IsTrue(core.TryBuildStructure(design));

        var diagTop = VehicleCore.CalculateAerodynamicDrag(1, Vector2.up, core.Hull);
        var diagRight = VehicleCore.CalculateAerodynamicDrag(1, Vector2.right, core.Hull);
        var diagLeft = VehicleCore.CalculateAerodynamicDrag(1, Vector2.left, core.Hull);

        Assert.IsTrue(diagRight.magnitude < diagLeft.magnitude);
        Assert.IsTrue(diagTop.magnitude < flatTop.magnitude);

        // Test the improved aerodynamics of a cone block.
        // - The right test should be lower than the left, due to the cone.
        // - The right test should be lower than the diagonal's right test
        // - The top test should be lower than the top diagonal test
        design.Clear();
        design.Add(new Vector2Int(1, 0), new ModuleSchematic(_cone));
        Assert.IsTrue(core.TryBuildStructure(design));

        var coneTop = VehicleCore.CalculateAerodynamicDrag(1, Vector2.up, core.Hull);
        var coneRight = VehicleCore.CalculateAerodynamicDrag(1, Vector2.right, core.Hull);
        var coneLeft = VehicleCore.CalculateAerodynamicDrag(1, Vector2.left, core.Hull);

        Assert.IsTrue(coneRight.magnitude < coneLeft.magnitude);
        Assert.IsTrue(coneRight.magnitude < diagRight.magnitude);
        Assert.IsTrue(coneTop.magnitude < flatTop.magnitude);

        // Test coefficient multiplier
        Assert.IsTrue(
            VehicleCore.CalculateAerodynamicDrag(2, Vector2.right, core.Hull) ==
            2 * VehicleCore.CalculateAerodynamicDrag(1, Vector2.right, core.Hull)
        );

        GameObject.Destroy(vehicle);
    }

    [Test]
    public void TestPregenerated()
    {
        var vehicle = GameObject.Instantiate(_vehicle, Vector3.zero, Quaternion.identity);
        var core = vehicle.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        // Create a basic vehicle design
        Dictionary<Vector2Int, ModuleSchematic> design = new();
        design.Add(new Vector2Int(1, 0), new ModuleSchematic(_chassis));
        design.Add(new Vector2Int(-2, 0), new ModuleSchematic(_tank));
        design.Add(new Vector2Int(0, 1), new ModuleSchematic(_rocket));
        design.Add(new Vector2Int(-3, 0), new ModuleSchematic(_propeller, 180));
        design.Add(new Vector2Int(2, 0), new ModuleSchematic(_thruster, 180));
        design.Add(new Vector2Int(-1, -1), new ModuleSchematic(_thruster, 90));
        design.Add(new Vector2Int(1, -1), new ModuleSchematic(_largeWheel));
        design.Add(new Vector2Int(-2, -1), new ModuleSchematic(_suspension));
        Assert.IsTrue(core.TryBuildStructure(design));

        // Clone the vehicle and pre-generate it, testing that both vehicles still match
        var clone = GameObject.Instantiate(vehicle, Vector3.zero, Quaternion.identity);
        var cloneCore = clone.GetComponent<VehicleCore>();
        Assert.IsNotNull(core);

        cloneCore.Discover();

        // Match properties
        Assert.IsTrue(core.EnergyCapacity == cloneCore.EnergyCapacity);
        Assert.IsTrue(core.EnergyLevel == cloneCore.EnergyLevel);
        Assert.IsTrue(core.Rigidbody.mass == cloneCore.Rigidbody.mass);
        Assert.IsTrue(Mathf.Abs(core.Rigidbody.centerOfMass.x - cloneCore.Rigidbody.centerOfMass.x) < 0.1f);
        Assert.IsTrue(Mathf.Abs(core.Rigidbody.centerOfMass.y - cloneCore.Rigidbody.centerOfMass.y) < 0.1f);

        // Match hulls
        Assert.IsTrue(core.LocalHull.Count == cloneCore.LocalHull.Count);
        for(int i = 0; i < core.LocalHull.Count; i++)
            Assert.IsTrue(core.LocalHull[i] == cloneCore.LocalHull[i]);

        // Match actuators
        Assert.IsTrue(core.Actuators.Count == cloneCore.Actuators.Count);
        for (int i = 0; i < core.Actuators.Count; i++)
        {
            Assert.IsTrue(core.Actuators[i].name == cloneCore.Actuators[i].name);
            Assert.IsTrue(core.Actuators[i].ActuationForce == cloneCore.Actuators[i].ActuationForce);
            Assert.IsTrue(core.Actuators[i].ActuationPosition == cloneCore.Actuators[i].ActuationPosition);
        }

        // Match attachments
        Assert.IsTrue(core.Attachments.Count == core.Attachments.Count);
        for (int i = 0; i < core.Attachments.Count; i++)
        {
            Assert.IsTrue(core.Attachments[i].name == cloneCore.Attachments[i].name);
            Assert.IsTrue(core.Attachments[i].position == cloneCore.Attachments[i].position);
            Assert.IsTrue(core.Attachments[i].rotation == cloneCore.Attachments[i].rotation);
        }

        GameObject.Destroy(vehicle);
        GameObject.Destroy(clone);
    }

}
