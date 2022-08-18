using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableModule : MonoBehaviour
    , IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private VehicleConstructor _vehicleConstructor;
    private bool _dragging = false;
    public GameObject originalPrefab; // Original prefab of the module
    private int _rotation = 0;
    private Vector2Int _localPos; // Position of the module relative to the vehicle core
    private Vector3 _origPos; // Position of the module before moving

    private void Awake()
    {
        _vehicleConstructor = GameObject.FindGameObjectWithTag("GameController").GetComponent<VehicleConstructor>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdatePosition();
        _dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;

        // Try to add the module to design
        (bool, Vector2Int) successful = _vehicleConstructor.TryAddModule(gameObject, originalPrefab);
        if (!successful.Item1)
        {
            // If module hasn't been placed, destroy it
            if (_localPos == null)
            {
                Destroy(gameObject);
                return;
            }
            // Return to original position
            transform.position = _origPos;
            return;
        }

        // Removing the module's old position from design
        if (_localPos != null)
        {
            _vehicleConstructor.RemoveModule(_localPos, GetComponent<VehicleModule>().Size);
        }
        _origPos = transform.position;
        _localPos = successful.Item2;
    }

    private void Update()
    {
        // Detect rotation
        if (_dragging)
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            
            // Anticlockwise rotation
            if (scrollDelta < 0 || Input.GetKeyDown(KeyCode.E))
            {
                _rotation -= 90;
            }

            // Clockwise rotation
            else if (scrollDelta > 0 || Input.GetKeyDown(KeyCode.R))
            {
                _rotation += 90;
            }

            // Constraining rotation to be between 0 and 360
            if (_rotation < 0)
            {
                _rotation += 360;
            }
            else if (_rotation >= 360)
            {
                _rotation -= 360;
            }

            transform.rotation = Quaternion.Euler(0, 0, _rotation);
            UpdatePosition();
        }
        Debug.Log(_rotation);
    }

    /// <summary>
    /// Sets the position of the module to the mouse position
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), 0) + CalculateRotationOffset();
    }

    /// <summary>
    /// Required for OnPointerUp() to be called
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData) { }

    /// <summary>
    /// Calculates the position offset caused by the module rotation
    /// </summary>
    /// <returns></returns>
    public Vector3 CalculateRotationOffset()
    {
        switch (_rotation)
        {
            case 0:
                return Vector3.zero;
            case 90:
                return Vector3.right;
            case 180:
                return Vector3.one;
            case 270:
                return Vector3.up;
            default:
                return Vector3.zero;
        }
    }

    public void SetLocalPos(Vector2Int localPos)
    {
        _localPos = localPos;
    }

    public int GetRotation()
    {
        return _rotation;
    }
}
