using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableModule : MonoBehaviour
    , IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Transform _moduleHolder;
    private VehicleConstructor _vehicleConstructor;
    private VehicleModule _vehicleModule;
    private bool _dragging = false;
    private bool _placed = false;
    public GameObject originalPrefab; // Original prefab of the module
    private int _rotation = 0;
    private Vector2Int _localPos; // Position of the module relative to the vehicle core
    private Vector3 _origPos; // Position of the module before moving
    private SimulationController _simulationController;

    private void Awake()
    {
        _origPos = transform.position;
        _vehicleModule = GetComponent<VehicleModule>();
        _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        _vehicleConstructor = _simulationController.GetComponent<VehicleConstructor>();
        _moduleHolder = _simulationController.buildModeModuleHolder.transform;
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
        if (!successful.Item1 || !MouseOverGrid())
        {
            // Return to original position
            transform.position = _origPos;
            return;
        }

        // Removing the module's old position from design if placed
        if (_placed)
        {
            _vehicleConstructor.RemoveModule(_localPos, _vehicleModule.Size);
        }
        // Replacing module in menu
        else
        {
            Instantiate(gameObject, _origPos, Quaternion.identity, transform.parent);
            transform.parent = _moduleHolder;
        }
        _placed = true;
        _origPos = transform.position;
        _localPos = successful.Item2;
    }

    private void Update()
    {
        // While being dragged
        if (_dragging)
        {
            // Detect rotation from scroll wheel or key input
            float scrollDelta = Input.mouseScrollDelta.y;
            
            // Clockwise rotation
            if (scrollDelta < 0 || Input.GetKeyDown(KeyCode.R))
            {
                _rotation -= 90;
            }

            // Anticlockwise rotation
            else if (scrollDelta > 0 || Input.GetKeyDown(KeyCode.E))
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

            // Deleting module
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_placed)
                {
                    _vehicleConstructor.RemoveModule(_localPos, _vehicleModule.Size);
                }
                else
                {
                    Instantiate(gameObject, _origPos, Quaternion.identity, transform.parent);
                }
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Sets the position of the module to the mouse position
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (MouseOverGrid())
        {
            transform.position = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), 0) + CalculateRotationOffset();
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0) + CalculateRotationOffset();
        }
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

    private bool MouseOverGrid()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // TODO: fix up to work with any grid size
        if (-6 < mousePos.x && mousePos.x < 7 && 0 < mousePos.y && mousePos.y < 7)
        {
            return true;
        }
        return false;
    }

    public void SetLocalPos(Vector2Int localPos)
    {
        _localPos = localPos;
    }

    public int GetRotation()
    {
        return _rotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(true);
        _simulationController.moduleStatsDisplay.GetComponent<TextMeshProUGUI>().text =
            $"{_vehicleModule.Mass}\n" +
            $"{_vehicleModule.EnergyCapacity}\n" +
            $"{(TryGetComponent(out ActuatorModule actuator) ? actuator.LocalActuationForce.magnitude : 0)}";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_dragging)
        {
            _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(false);
        }
    }
}
