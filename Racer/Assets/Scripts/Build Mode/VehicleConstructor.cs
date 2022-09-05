using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleConstructor : MonoBehaviour
{
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
    }


    /// <summary>
    /// Determines the grid position and rotated size of a module on the grid
    /// </summary>
    /// <param name="module"> The VehicleModule being placed </param>
    /// <param name="position"> The position of the module </param>
    /// <param name="rotation"> The rotation of the module </param>
    /// <returns> A tuple containing the rotated size and resulting integer grid coordinates </returns>
    private (Vector2, Vector2Int) GatherGridSpaceInfo(GameObject module, Vector3 position, Quaternion rotation)
    {
        VehicleModule properties = module.GetComponent<VehicleModule>();
        DraggableModule draggableModule = module.GetComponent<DraggableModule>();

        var gridPos = TransformToGrid(position - draggableModule.CalculateRotationOffset());
        var gridSize = TransformGridSize(properties.Size, rotation);

        return (gridSize, gridPos);
    }


    /// <summary>
    /// Overload of GatherGridSpaceInfo which gathers all information from the module itself
    /// </summary>
    /// <param name="module"> the VehicleModule being placed </param>
    /// <returns> A tuple containing the rotated size and resulting integer grid coordinates </returns>
    private (Vector2, Vector2Int) GatherGridSpaceInfo(GameObject module)
    {
        return GatherGridSpaceInfo(module, module.transform.position, module.transform.rotation);
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
        var (gridSize, gridPos) = GatherGridSpaceInfo(module, position, rotation);

        for (int dx = 0; dx != gridSize.x; dx += Math.Sign(gridSize.x))
        {
            for (int dy = 0; dy != gridSize.y; dy += Math.Sign(gridSize.y))
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

        var (gridSize, gridPos) = GatherGridSpaceInfo(module);

        // Add original prefab to design
        _design[gridPos] = new ModuleSchematic(
            original,
            module.GetComponent<DraggableModule>().transform.rotation.eulerAngles.z
        );

        // Set size as unavailable module positions
        for (int dx = 0; dx != gridSize.x; dx += Math.Sign(gridSize.x))
        {
            for (int dy = 0; dy != gridSize.y; dy += Math.Sign(gridSize.y))
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
    /// <param name="gridPos">position of the draggable module in the grid</param>
    /// <param name="size">size of the module</param>
    public void RemoveModule(Vector2Int gridPos, Vector2 size, float rotation)
    {
        var gridSize = TransformGridSize(size, Quaternion.Euler(0, 0, rotation));
        _design.Remove(gridPos);
        for (int dx = 0; dx != gridSize.x; dx += Math.Sign(gridSize.x))
        {
            for (int dy = 0; dy != gridSize.y; dy += Math.Sign(gridSize.y))
            {
                var coord = new Vector2Int(gridPos.x + dx, gridPos.y + dy);
                _occupancy.Remove(coord);
            }
        }
    }


    /// <summary>
    /// Converts the world position of the module to a grid position relative to the vehicle core
    /// </summary>
    /// <param name="worldPos">position of the module</param>
    /// <returns>position of the module in the grid</returns>
    public Vector2Int TransformToGrid(Vector2 worldPos)
    {
        var gridPos = ClampToGrid(worldPos);
        return new Vector2Int((int)gridPos.x, (int)gridPos.y) - _coreWorldPos;

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
    /// Tests whether the given grid point is valid (on the grid space)
    /// </summary>
    /// <param name="worldPos">the grid coord to test</param>
    /// <returns>true, if on grid</returns>
    public static bool TestOnGrid(Vector2 gridPos)
    {
        return gridPos.x > -7 && gridPos.x < 7 && gridPos.y > -4 && gridPos.y < 4;
    }


    /// <summary>
    /// Re-orients the given module size based on the direction of the module rotation.
    /// The width and height has directionality from the origin point of the module, 
    /// meaning they can be negative depending on the rotation. 
    /// </summary>
    /// <param name="size"> the size of the module </param>
    /// <param name="moduleRotation"> a Quaternion representing the rotation of the module </param>
    /// <returns> The transformed grid size </returns>
    public static Vector2 TransformGridSize(Vector2 size, Quaternion moduleRotation)
    {
        var rotSize = moduleRotation * size;
        var gridSize = new Vector2(
           Mathf.Round(rotSize.x),
           Mathf.Round(rotSize.y)
        );
        return gridSize;
    }

    public void ValidateDesign()
    {
        var feedback = vehicleCore.ValidateDesign(GetDesign());
        Debug.Log("NEW DESIGN");
        if(feedback.ValidDesign)
            Debug.Log("Valid: Yes");
        else
            Debug.Log("Valid: No");

        Debug.Log($"Capacity: {feedback.TotalEnergyCapacity}");
        Debug.Log($"Mass: {feedback.TotalMass}");
        Debug.Log($"Centre of Mass: {feedback.LocalCentreOfMass}");

        // Remove error feedback from valid modules
        foreach(var offset in feedback.ValidModules)
        {
            var module = _occupancy[offset];

            module.GetComponent<DraggableModule>().ApplyTint(false);
            if(module.TryGetComponent<TooltipTrigger>(out var trigger))
            {
                trigger.Hide();
                trigger.enabled = false;
            }
        }

        // Add error feedback to bad disjoint modules
        foreach (var offset in feedback.DisjointModules)
        {
            var module = _occupancy[offset];

            module.GetComponent<DraggableModule>().ApplyTint(true);
            if (module.TryGetComponent<TooltipTrigger>(out var trigger))
                trigger.enabled = true;
        }
    }

    public Dictionary<Vector2Int, ModuleSchematic> GetDesign()
    {
        return _design;
    }
}
