using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility;
using Level;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Build_Mode
{
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
        private bool _dragging;

        /// <summary>
        /// True if the draggable has been placed, otherwise false
        /// </summary>
        private bool _placed;

        /// <summary>
        /// True if the mouse is over the module, otherwise false
        /// </summary>
        private bool _hover;

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

        /// <summary>
        /// The box collider used to drag the module around
        /// </summary>
        public BoxCollider2D dragCollider;

        public GameObject forceIndicatorPrefab;

        public TooltipTrigger feedbackTrigger;
    
        public  GameObject forceIndicatorObject;
    
        private SpriteRenderer _forceIndicatorRenderer;

        private int _moduleLayerMask; // Layer when placed onto player vehicle
        private int _listLayerMask; // Layer when in the buildmode list

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

            // Set the draggable module to an 'inert' layer that can't
            // interact with anything while its in the module list
            _moduleLayerMask = LayerMask.NameToLayer("Module");
            _listLayerMask = LayerMask.NameToLayer("Ignore Raycast");
            Algorithms.SetLayers(gameObject, _listLayerMask);

            // Offset from bottom left origin to the centre of the module
            CentreOffset = _vehicleModule.Size / 2.0f;

            // Add a hitbox that spans the size of the module
            if(dragCollider == null)
            {
                dragCollider = gameObject.AddComponent<BoxCollider2D>();
                dragCollider.size = _vehicleModule.Size;
                dragCollider.offset = CentreOffset;
            }

            // Add tooltip trigger for invalid state
            if (feedbackTrigger == null)
            {
                feedbackTrigger = gameObject.AddComponent<TooltipTrigger>();
                feedbackTrigger.header = "";
                feedbackTrigger.content = "";
                feedbackTrigger.enabled = false;
            }

            // If we are an actuator, then add a force indicator object
            if(TryGetComponent<ActuatorModule>(out var actuator))
            {
                var rotation = Mathf.Rad2Deg * Mathf.Atan2(actuator.LocalActuationForce.y, actuator.LocalActuationForce.x);

                if(forceIndicatorObject == null && forceIndicatorPrefab != null)
                    forceIndicatorObject = Instantiate(
                        forceIndicatorPrefab,
                        actuator.LocalActuationPosition + (Vector2)actuator.transform.position,
                        Quaternion.Euler(0, 0, rotation),
                        gameObject.transform
                    );

                _forceIndicatorRenderer = forceIndicatorObject.GetComponentInChildren<SpriteRenderer>();
                _forceIndicatorRenderer.enabled = false;
            }

            // Default to non-tinted state
            ResetTint();
        }

        /// <summary>
        /// Called when the module is being dragged by the mouse
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition();
            _dragging = true;

            // Test our on the grid to check if we should be tinting for errors or not
            if (!_vehicleConstructor.TestPlacement(gameObject))
                ApplyTint(Color.gray);
            else
                ResetTint();

        
            feedbackTrigger.Hide();
            feedbackTrigger.enabled = false;
        }


        /// <summary>
        /// Called whenever the mouse pointer enters the module to display the module stats
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            if (_placed || _dragging) return;
            
            //Numeric stats
            // _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(true);
            // _simulationController.moduleStatsDisplay.GetComponent<TextMeshProUGUI>().text = ;

            //Title for left panel
            _simulationController.moduleNameDisplay.GetComponent<TextMeshProUGUI>().text = _vehicleModule.Name;

            //Text stats on left panel
            _simulationController.moduleInfoDisplay.transform.parent.gameObject.SetActive(true);
            _simulationController.moduleInfoDisplay.GetComponent<TextMeshProUGUI>().text = "Cost:\n\n" + _vehicleModule.Description;

            //Numeric stats on left panel
            _simulationController.moduleExtraStatsDisplay.GetComponent<TextMeshProUGUI>().text =
                $"{_vehicleModule.Mass}kg\n" +
                $"{_vehicleModule.EnergyCapacity} J\n" +
                $"{(TryGetComponent(out ActuatorModule actuator) ? actuator.LocalActuationForce.magnitude : 0)} N\n" +
                $"{_vehicleModule.Size.x.ToString()}x{_vehicleModule.Size.y.ToString()}\n" +
                $"{(TryGetComponent(out ActuatorModule actuator2) ? actuator2.IdleCost.ToString() : "0")} J/sec\n" +
                $"{(TryGetComponent(out ActuatorModule actuator3) ? actuator3.ActivationCost.ToString() : "0")} J/sec\n";

            _simulationController.moduleCostDisplay.GetComponent<TextMeshProUGUI>().text =
                $"${_vehicleModule.Cost.ToString()}";
        }


        /// <summary>
        /// Called when the mouse pointer leaves the module, to hide the module stats
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            if (!_dragging)
            {
                // _simulationController.moduleStatsDisplay.transform.parent.gameObject.SetActive(false);
                _simulationController.moduleInfoDisplay.transform.parent.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// Called when the mouse pointer releases on the module
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
            DragOffset = Vector3.zero;

            // Clamp the module's position to the grid
            transform.position = VehicleConstructor.ClampToGrid(transform.position);

            // Test if the module can be placed into the grid
            if (_vehicleConstructor.TestPlacement(gameObject))
            {
                // The grid placement allows conditions where a module may be placed in a spot which overlaps itself.
                // In such a case, we need to remove the module first before adding it, otherwise the grid gets messed up.
                // TODO: this ^^ is really just a failure in the design and should be fixed if it becomes a problem.
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
                    // Disable thrust lines on new gameobject
                    if (_forceIndicatorRenderer != null)
                        _forceIndicatorRenderer.enabled = false;

                    Instantiate(gameObject, _spawnPosition, Quaternion.identity, transform.parent);
                
                    Algorithms.SetLayers(gameObject, _moduleLayerMask);
                    transform.parent = _moduleHolder;
                }

                _placed = true;
                _placedGridPos = position;
                _savedPosition = transform.position;
                _savedRotation = transform.rotation.eulerAngles.z;
            
                // Validate the design
                _vehicleConstructor.ValidateDesign();
            }
            // If the module was dragged outside of the grid, then delete it
            else if(!_vehicleConstructor.TestOnGrid(gameObject))
                Delete();
            else // Otherwise just reset its position
                Reset();
        }


        /// <summary>
        /// Resets the vehicle module back to its last saved position and rotation
        /// </summary>
        public void Reset()
        {
            transform.position = _savedPosition;
            transform.rotation = Quaternion.Euler(0, 0, _savedRotation);
            ResetTint();

            _vehicleConstructor.ValidateDesign();
        }


        private void Update()
        {
            // While being dragged
            if (_dragging)
            {
                // Detect rotation from scroll wheel or key input
                var scrollDelta = Input.mouseScrollDelta.y;
            
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
            }

            // Deleting module
            if (_hover && Input.GetKeyDown(KeyCode.Delete))
                Delete();

            // Draw thrust arrows if its an actuator and we are hovering over it
            if(_forceIndicatorRenderer)
                _forceIndicatorRenderer.enabled = _hover;
        }


        /// <summary>
        /// Properly deletes the module
        /// </summary>
        private void Delete()
        {
            if (!_placed)
            {
                // Disable thrust lines on new object
                if (_forceIndicatorRenderer)
                    _forceIndicatorRenderer.enabled = false;

                Instantiate(gameObject, _spawnPosition, Quaternion.identity, transform.parent);
            }
            else _vehicleConstructor.RemoveModule(_placedGridPos, _vehicleModule.Size, _savedRotation);

            // Hide any open tooltips
            feedbackTrigger.Hide();
            feedbackTrigger.enabled = false;

            Destroy(gameObject);

            _vehicleConstructor.ValidateDesign();
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
        /// Resets the tint for the module
        /// </summary>
        public void ResetTint()
        {
            ApplyTint(Color.white);
        }


        /// <summary>
        /// Applies a tint color to the entire module
        /// </summary>
        /// <param name="tintColor"> The color to apply, this action is irreversible for non-textured objects</param>
        public void ApplyTint(Color tintColor)
        {
            // Get all sprite rendering components
            var renderers = new List<SpriteRenderer>();
            GetComponentsInChildren(renderers);
            foreach (var r in renderers.Where(r => r != _forceIndicatorRenderer))
                r.color = tintColor;
        }


    }
}
