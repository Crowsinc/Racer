using System;
using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;

public class VehicleConstructor : MonoBehaviour
{

    /// <summary>
    /// Centre of gravity indicator prefab
    /// </summary>
    public GameObject COGIndicatorPrefab;
    private GameObject _cogIndicator;

    public VehicleCore vehicleCore;
    private Vector2Int _coreWorldPos;

    private Dictionary<Vector2Int, ModuleSchematic> _design = new Dictionary<Vector2Int, ModuleSchematic>();
    private Dictionary<Vector2Int, GameObject> _occupancy = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        _coreWorldPos = new Vector2Int((int)Mathf.Floor(vehicleCore.transform.position.x), (int)Mathf.Floor(vehicleCore.transform.position.y));
        vehicleCore.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        // Add the vehicle core to the occupancy list so that nothing can ever overlap it. 
        _occupancy[new Vector2Int(0, 0)] = vehicleCore.gameObject;

        _cogIndicator = Instantiate(COGIndicatorPrefab, new Vector3(-1000, -1000, 0), Quaternion.identity);
    }



    /// <summary>
    /// Transforms the given module's position and rotation to a grid location 
    /// with the origin in the bottom left 
    /// </summary>
    /// <param name="module"> The VehicleModule in question </param>
    /// <param name="position"> The position of the module </param>
    /// <param name="rotation"> The rotation of the module </param>
    /// <returns> The grid position of the module, whose grid coordinates are in the bottom left of the module </returns>
    public Vector2Int TransformToGrid(GameObject module, Vector3 position, Quaternion rotation)
    {
        VehicleModule properties = module.GetComponent<VehicleModule>();

        var gridPos = ClampToGrid(position) - _coreWorldPos;
        var gridSize = VehicleModule.RotateSize(properties.Size, rotation);

        // Reset the grid position back to the bottom left in the case of any rotation
        // We can do this by subtracting any negative grid sizes from the grid position
        gridPos.x += (int)Math.Min(gridSize.x, 0);
        gridPos.y += (int)Math.Min(gridSize.y, 0);

        return new Vector2Int((int)gridPos.x, (int)gridPos.y);
    }


    /// <summary>
    /// Overload of TransformToGrid which gathers all information from the module itself
    /// </summary>
    /// <param name="module"> the VehicleModule in question </param>
    /// <returns> The grid position of the module, whose grid coordinates are in the bottom left of the module </returns>
    public Vector2Int TransformToGrid(GameObject module)
    {
        return TransformToGrid(module, module.transform.position, module.transform.rotation);
    }


    /// <summary>
    /// Tests the placement of a module using the given position and rotation
    /// </summary>
    /// <param name="module"> the module to be placed </param>
    /// <param name="position"> the position of the modules origin </param>
    /// <param name="rotation"> the rotation of the module </param>
    /// <returns> true if it can be placed, the placement may overlap its old position </returns>
    public bool TestPlacement(GameObject module, Vector2 position, Quaternion rotation)
    {
        var properties = module.GetComponent<VehicleModule>();
        var gridSize = VehicleModule.RotateSize(properties.Size, module.transform.rotation);
        var gridPos = TransformToGrid(module, position, rotation);

        for (int dx = 0; dx < Mathf.Abs(gridSize.x); dx++)
        {
            for (int dy = 0; dy < Mathf.Abs(gridSize.y); dy++)
            {
                var coord = new Vector2Int(gridPos.x + dx, gridPos.y + dy);

                // If the coord is taken by a different object, or is outside the grid
                if (!TestOnGrid(coord))
                    return false;
                if (_occupancy.ContainsKey(coord) && _occupancy[coord] != module)
                    return false;
            }
        }
        return true;
    }


    /// <summary>
    /// Tests the placement of a module given its current transform
    /// </summary>
    /// <param name="module"> the module to be placed </param>
    /// <returns> true if it can be placed, the placement may overlap its old position</returns>
    public bool TestPlacement(GameObject module)
    {
        return TestPlacement(module, module.transform.position, module.transform.rotation);
    }

    
    /// <summary>
    /// Tries to add a module to the design
    /// </summary>
    /// <param name="module">draggable module to be added</param>
    /// <param name="original">original version of the module</param>
    /// <returns>true if the module placement was valid, otherwise false</returns>
    public (bool, Vector2Int) TryAddModule(GameObject module, GameObject original)
    {
        if (!TestPlacement(module))
            return (false, Vector2Int.zero);

        var properties = module.GetComponent<VehicleModule>();
        var gridSize = VehicleModule.RotateSize(properties.Size, module.transform.rotation);
        var gridPos = TransformToGrid(module);

        // Add original prefab to design
        _design[gridPos] = new ModuleSchematic(
            original,
            module.GetComponent<DraggableModule>().transform.rotation.eulerAngles.z
        );

        // Set size as unavailable module positions
        for (int dx = 0; dx < Mathf.Abs(gridSize.x); dx++)
        {
            for (int dy = 0; dy < Mathf.Abs(gridSize.y); dy++)
            {
                var coord = new Vector2Int(gridPos.x + dx, gridPos.y + dy);
                _occupancy[coord] = module;
            }
        }
        return (true, gridPos);
    }

    /// <summary>
    /// Removes the module at that position from the design
    /// </summary>
    /// <param name="gridPos">position of the draggable module in the grid, taken from its bottom left</param>
    /// <param name="size">size of the module</param>
    public void RemoveModule(Vector2Int gridPos, Vector2 size, float rotation)
    {
        var gridSize = VehicleModule.RotateSize(size, Quaternion.Euler(0, 0, rotation));

        _design.Remove(gridPos);
        for (int dx = 0; dx < Mathf.Abs(gridSize.x); dx++)
        {
            for (int dy = 0; dy < Mathf.Abs(gridSize.y); dy++)
            {
                var coord = new Vector2Int(gridPos.x + dx, gridPos.y + dy);
                _occupancy.Remove(coord);
            }
        }
    }


    /// <summary>
    /// Clamps the position of a module to a grid cell position
    /// </summary>
    /// <param name="worldPos">The position of the module in the world</param>
    /// <returns>the clamped position</returns>
    public static Vector2 ClampToGrid(Vector2 worldPos)
    {
        return new Vector2(
            Mathf.Round(worldPos.x),
            Mathf.Round(worldPos.y)
        );
    }


    /// <summary>
    /// Tests whether the given grid point is on the grid space
    /// </summary>
    /// <param name="worldPos">the grid coord to test</param>
    /// <returns>true, if on grid</returns>
    public static bool TestOnGrid(Vector2 gridPos)
    {
        return gridPos.x > -7 && gridPos.x < 7 && gridPos.y > -4 && gridPos.y < 4;
    }


    /// <summary>
    /// Tests whether the given module is on the grid space)
    /// </summary>
    /// <param name="module">the module to test</param>
    /// <returns>true, if on grid</returns>
    public bool TestOnGrid(GameObject module)
    {
        return TestOnGrid(TransformToGrid(module));
    }


    /// <summary>
    /// Validates the current vehicle design, showing the feedback on the builder grid
    /// </summary>
    public void ValidateDesign()
    {
        var feedback = vehicleCore.ValidateDesign(GetDesign());
        if(feedback.ValidDesign)
            Debug.Log("Design Valid: Yes");
        else
            Debug.Log("Design Valid: No");

        // Move centre of mass prefab
        _cogIndicator.transform.position = feedback.LocalCentreOfMass + _coreWorldPos;

        // Remove error feedback from valid modules
        foreach(var offset in feedback.ValidModules)
        {
            var module = _occupancy[offset];

            if (module.TryGetComponent<DraggableModule>(out var draggable))
            {
                draggable.FeedbackTrigger.Hide();
                draggable.FeedbackTrigger.enabled = false;
                draggable.ResetTint();
                
                // Test if the valid component is a blocked actuator,
                // if it is, then add error feedback for it. Note that
                // we only do this for valid modules because disjoint
                // errors take priority.
                if(module.TryGetComponent<ActuatorModule>(out var actuator) &&
                   module.TryGetComponent<ActuatorLineOfSightTester>(out var tester))
                {
                    actuator.UpdateDynamics();
                    if(!tester.TestLineOfSight())
                    {
                        draggable.FeedbackTrigger.enabled = true;
                        draggable.FeedbackTrigger.header = "Actuator output is blocked";
                        draggable.ApplyTint(Color.yellow);
                    }
                }
            }
        }

        // Add error feedback to bad disjoint modules
        foreach (var offset in feedback.DisjointModules)
        {
            var module = _occupancy[offset];

            if (module.TryGetComponent<DraggableModule>(out var draggable))
            {
                draggable.FeedbackTrigger.enabled = true;
                draggable.FeedbackTrigger.header = "Module is not connected";
                draggable.ApplyTint(Color.red);
            }
        }
    }


    /// <summary>
    /// Hides vehicle construction UI elements such as the centre of gravity indicator
    /// </summary>
    public void HideUIElements()
    {
        if (_cogIndicator != null && _cogIndicator.TryGetComponent<SpriteRenderer>(out var r))
            r.enabled = false;
    }


    /// <summary>
    /// Shows vehicle construction UI elements such as the centre of gravity indicator
    /// </summary>
    public void ShowUIElements()
    {
        if (_cogIndicator != null && _cogIndicator.TryGetComponent<SpriteRenderer>(out var r))
            r.enabled = true;
    }


    /// <summary>
    /// Calculates the sum cost of the vehicle in the design
    /// </summary>
    /// <returns>total sum</returns>
    public float SumVehicleCost()
    {
        float total = 0;
        foreach (KeyValuePair<Vector2Int, ModuleSchematic> pair in _design)
        {
            total += pair.Value.Prefab.GetComponent<VehicleModule>().Cost;
        }
        
        return total;
    }

    public bool ValidateRestrictions(List<LevelRestrictions> restrictions)
    {
        foreach (LevelRestrictions restriction in restrictions)
        {
            if (!restriction.PassesRestrictions(_design)) return false;
        }
        return true;
    }

    public int ModuleCount()
    {
        return _design.Count;
    }


    public Dictionary<Vector2Int, ModuleSchematic> GetDesign()
    {
        return _design;
    }

}
