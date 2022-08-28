using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableModule : MonoBehaviour
    , IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Parent that all dragged modules are children of
    /// </summary>
    private Transform _moduleHolder;

    /// <summary>
    /// Vehicle constructor which validates module placement
    /// </summary>
    private VehicleConstructor _vehicleConstructor;

    /// <summary>
    /// Global simulation controller
    /// </summary>
    private SimulationController _simulationController;

    /// <summary>
    /// VehicleModule component of this draggable
    /// </summary>
    private VehicleModule _vehicleModule;

    /// <summary>
    /// True if the module is being dragged, otherwise false
    /// </summary>
    private bool _dragging = false;

    /// <summary>
    /// True if the draggable has been placed, otherwise false
    /// </summary>
    private bool _placed = false;

    /// <summary>
    /// The number of degrees the draggable has rotated
    /// </summary>
    private int _rotation = 0;

    /// <summary>
    /// Position of the module relative to the vehicle core
    /// </summary>
    private Vector2Int _localPos;

    /// <summary>
    /// Position of the module before moving
    /// </summary>
    private Vector3 _origPos;

    /// <summary>
    /// Rotation of the module before moving
    /// </summary>
    private int _origRotation;

    /// <summary>
    /// Original prefab of the module
    /// </summary>
    public GameObject originalPrefab;

    private void Awake()
    {
        // Getting necessary components and game objects
        _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        _vehicleModule = GetComponent<VehicleModule>();
        _vehicleConstructor = _simulationController.GetComponent<VehicleConstructor>();
        _moduleHolder = _simulationController.buildModeModuleHolder.transform;
        
        // Setting original position & rotation
        _origPos = transform.position;
        _origRotation = (int)transform.rotation.eulerAngles.z;

        // Adding hitbox that spans the size of the module
        BoxCollider2D _draggableCollider = gameObject.AddComponent<BoxCollider2D>();
        _draggableCollider.size = _vehicleModule.Size;
    }

    /// <summary>
    /// Called when the module is being dragged by the mouse
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        UpdatePosition();
        _dragging = true;
    }

    /// <summary>
    /// Called when the mouse pointer releases on the module
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;

        // Check that mouse is being let go above the grid
        if (!MouseOverGrid())
        {
            Reset();
            return;
        }

        // Try to add the module to design
        (bool, Vector2Int) successful = _vehicleConstructor.TryAddModule(gameObject, originalPrefab);
        if (!successful.Item1)
        {
            Reset();
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
        _origRotation = (int)transform.rotation.eulerAngles.z;
        _localPos = successful.Item2;
    }


    /// <summary>
    /// Resets the vehicle module back to its previous position & rotation
    /// </summary>
    public void Reset()
    {
        transform.position = _origPos;

        _rotation = _origRotation;
        transform.rotation = Quaternion.Euler(0, 0, _origRotation);
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

    /// <summary>
    /// Returns whether or not the mouse is over the build grid
    /// </summary>
    /// <returns>true if the mouse is over the grid, otherwise false</returns>
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

    /// <summary>
    /// Sets the local position
    /// </summary>
    /// <param name="localPos">local position to be set</param>
    public void SetLocalPos(Vector2Int localPos)
    {
        _localPos = localPos;
    }

    /// <summary>
    /// Gets the rotation of the module
    /// </summary>
    /// <returns>rotation of the module</returns>
    public int GetRotation()
    {
        return _rotation;
    }

    /// <summary>
    /// Called whenever the mouse pointer enters the module to display the module stats
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(true);
        _simulationController.moduleStatsDisplay.GetComponent<TextMeshProUGUI>().text =
            $"{_vehicleModule.Mass}\n" +
            $"{_vehicleModule.EnergyCapacity}\n" +
            $"{(TryGetComponent(out ActuatorModule actuator) ? actuator.LocalActuationForce.magnitude : 0)}";
    }

    /// <summary>
    /// Called when the mouse pointer leaves the module, to hide the module stats
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_dragging)
        {
            _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(false);
        }
    }
}
