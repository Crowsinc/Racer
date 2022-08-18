using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleConstructor : MonoBehaviour
{
    public VehicleCore vehicleCore;
    private Dictionary<Vector2Int, ModuleSchematic> _design = new Dictionary<Vector2Int, ModuleSchematic>();
    private Vector2 _coreWorldPos;
    private List<Vector2Int> takenCoords = new List<Vector2Int>();

    void Start()
    {
        _coreWorldPos = new Vector2((int)Mathf.Floor(vehicleCore.transform.position.x), (int)Mathf.Floor(vehicleCore.transform.position.y));
        vehicleCore.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    /// <summary>
    /// Tries to add a module to the design
    /// </summary>
    /// <param name="module">draggable module to be added</param>
    /// <param name="original">original version of the module</param>
    /// <returns>true if the module placement was valid, otherwise false</returns>
    public (bool, Vector2Int) TryAddModule(GameObject module, GameObject original)
    {
        Vector2Int localPos = ModuleWorldPosToLocalPos(module.transform.position - module.GetComponent<DraggableModule>().CalculateRotationOffset());

        // Overlapping other module
        if (localPos == Vector2Int.zero || takenCoords.Contains(localPos)){
            return (false, Vector2Int.zero);
        }

        // Add original prefab to design
        _design.Add(localPos, new ModuleSchematic(original, module.GetComponent<DraggableModule>().GetRotation()));

        // Set size as unavaiable module positions
        for (int i = 0; i < module.GetComponent<VehicleModule>().Size.x; i++)
        {
            for (int j = 0; j < module.GetComponent<VehicleModule>().Size.y; j++)
            {
                takenCoords.Add(new Vector2Int(localPos.x + i, localPos.y + j));
            }
        }
        return (true, localPos);
    }

    /// <summary>
    /// Removes the module at that position from the design
    /// </summary>
    /// <param name="localPos">position of the draggable module</param>
    /// <param name="size">size of the module</param>
    public void RemoveModule(Vector2Int localPos, Vector2 size)
    {
        _design.Remove(localPos);
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                takenCoords.Remove(new Vector2Int(localPos.x + i, localPos.y + j));
            }
        }
    }

    /// <summary>
    /// Converts the world position of the module to position relative to the vehicle core
    /// </summary>
    /// <param name="worldPos">position of the module</param>
    /// <returns>position of the module relative to the vehicle core</returns>
    private Vector2Int ModuleWorldPosToLocalPos(Vector2 worldPos)
    {
        return new Vector2Int((int)Mathf.Floor(worldPos.x - _coreWorldPos.x), (int)Mathf.Floor(worldPos.y - _coreWorldPos.y));
    }

    public Dictionary<Vector2Int, ModuleSchematic> GetDesign()
    {
        return _design;
    }
}
