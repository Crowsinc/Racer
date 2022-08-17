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

    public Dictionary<Vector2Int, ModuleSchematic> GetDesign()
    {
        return _design;
    }

    public bool TryAddModule(GameObject module, GameObject original)
    {
        Vector2Int localPos = ModuleWorldPosToLocalPos(module.transform.position);
        Debug.Log(localPos);
        // Overlapping other module
        if (localPos == Vector2Int.zero || takenCoords.Contains(localPos)){
            return false;
        }

        _design.Add(localPos, new ModuleSchematic(original));
        for (int i = 0; i < module.GetComponent<VehicleModule>().Size.x; i++)
        {
            for (int j = 0; j < module.GetComponent<VehicleModule>().Size.y; j++)
            {
                takenCoords.Add(new Vector2Int(localPos.x + i, localPos.y + j));
            }
        }
        return true;
    }
    
    private Vector2Int ModuleWorldPosToLocalPos(Vector2 worldPos)
    {
        return new Vector2Int((int)Mathf.Floor(worldPos.x - _coreWorldPos.x), (int)Mathf.Floor(worldPos.y - _coreWorldPos.y));
    }
}
