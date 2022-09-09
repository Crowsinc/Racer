using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Codice.Client.BaseCommands;

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
    /// Position of the module relative to the vehicle core
    /// </summary>
    private Vector2Int _placedGridPos;

    /// <summary>
    /// The last valid placed position of the module
    /// </summary>
    private Vector2 _savedPosition;

    /// <summary>
    /// The last valid placed rotation of the module
    /// </summary>
    private float _savedRotation;


    /// <summary>
    /// The spawn position of the module.
    /// </summary>
    private Vector2 _spawnPosition;


    /// <summary>
    /// Original prefab of the module
    /// </summary>
    public GameObject originalPrefab;


    /// <summary>
    ///  Offset from bottom left origin to the centre of the module
    /// </summary>
    public Vector3 CentreOffset { get; private set; }

    /// <summary>
    /// Offset from bottom left origin to the position grabbed by the user
    /// </summary>
    public Vector3 DragOffset { get; private set; }

    private void Awake()
    {
        // Getting necessary components and game objects
        _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        _vehicleModule = GetComponent<VehicleModule>();
        _vehicleConstructor = _simulationController.GetComponent<VehicleConstructor>();
        _moduleHolder = _simulationController.buildModeModuleHolder.transform;
        
        _spawnPosition = transform.position;
        _savedPosition = transform.position;
        _savedRotation = 0.0f;

        // Offset from bottom left origin to the centre of the module
        CentreOffset = _vehicleModule.Size / 2.0f;

        // Adding hitbox that spans the size of the module
        BoxCollider2D _draggableCollider = gameObject.AddComponent<BoxCollider2D>();
        _draggableCollider.size = _vehicleModule.Size;
        _draggableCollider.offset = CentreOffset;

        // Initialise tooltip
        TooltipTrigger tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
        tooltipTrigger.content = _vehicleModule.Cost.ToString() + "$";
        tooltipTrigger.header = _vehicleModule.Name;
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
            _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when the mouse pointer releases on the module
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
        DragOffset = Vector3.zero;

        // If the mouse is let go outside of the grid, then delete the module
        if (!MouseOverGrid())
        {
            Delete();
            return;
        }

        // Clamp the module to the grid
        transform.position = VehicleConstructor.ClampToGrid(transform.position);

        // Test if the module can be placed into the grid
        if (_vehicleConstructor.TestPlacement(gameObject))
        {
            // The grid placement allows conditions where a module may be placed in a spot which overlaps itself.
            // In such a case, we need to remove the module first before adding it, otherwise the grid gets messed up.
            if (_placed)
                _vehicleConstructor.RemoveModule(_placedGridPos, _vehicleModule.Size, _savedRotation);
            
            // Place the module 
            var (pass, position) = _vehicleConstructor.TryAddModule(gameObject, originalPrefab);
            if (!pass)
            {
                // This should never happen because we are already doing a placement test
                Reset();
                return;
            }

            // If the wasn't previously placed, then instantiate a new one
            if (!_placed)
            {
                Instantiate(gameObject, _spawnPosition, Quaternion.identity, transform.parent);
                transform.parent = _moduleHolder;
            }

            _placed = true;
            _placedGridPos = position;
            _savedPosition = transform.position;
            _savedRotation = transform.rotation.eulerAngles.z;
        }
        else Reset();
    }


    /// <summary>
    /// Resets the vehicle module back to its previous position & rotation
    /// </summary>
    public void Reset()
    {
        transform.position = _savedPosition;
        transform.rotation = Quaternion.Euler(0, 0, _savedRotation);
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
                transform.RotateAround(transform.position + DragOffset, Vector3.forward, -90);
                DragOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }

            // Anticlockwise rotation
            else if (scrollDelta > 0 || Input.GetKeyDown(KeyCode.E))
            {
                transform.RotateAround(transform.position + DragOffset, Vector3.forward, 90);
                DragOffset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }

            UpdatePosition();

            // Deleting module
            if (Input.GetKeyDown(KeyCode.Delete))
                Delete();
        }
    }


    /// <summary>
    /// Properly deletes the module
    /// </summary>
    private void Delete()
    {
        if (_placed)
            _vehicleConstructor.RemoveModule(_placedGridPos, _vehicleModule.Size, _savedRotation);
        else
            Instantiate(gameObject, _spawnPosition, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }

    /// <summary>
    /// Sets the position of the module to the mouse position
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) - DragOffset;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }


    /// <summary>
    /// Required for OnPointerUp() to be called
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData) 
    {
        var worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DragOffset = worldPosition - transform.position;
    }


    /// <summary>
    /// Returns whether or not the mouse is over the build grid
    /// </summary>
    /// <returns>true if the mouse is over the grid, otherwise false</returns>
    private bool MouseOverGrid()
    {
        var gridPos = _vehicleConstructor.TransformToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        return VehicleConstructor.TestOnGrid(gridPos);
    }

    /// <summary>
    /// Calculates the position offset caused by the module rotation
    /// </summary>
    /// <returns></returns>
    public Vector3 CalculateRotationOffset()
    {
        int rotation = (int)(transform.rotation.eulerAngles.z / 90.0f) * 90;
        switch (rotation)
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


  
}
