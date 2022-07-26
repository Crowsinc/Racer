using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Build_Mode
{
    public class BuildModeUIController : MonoBehaviour
    { 
        // Containers for menu modules
        public GameObject wheelsHolder;
        public GameObject energyHolder;
        public GameObject chassisHolder;
        public GameObject actuatorHolder;
        public GameObject controlsPanel;
        public Toggle controlsButton;
        public ButtonSounds buttonSounds;
        
        public GameObject forceIndicatorPrefab;

        public RectTransform initialPlacement; // Rect in UI space for the initial placement


        private GameObject _currentTab;
        private ModuleCollection _moduleCollection;
        private float _moduleYDisplacement;

        private void Awake()
        {
            // Initial tab
            _currentTab = wheelsHolder;
            EnableUI(_currentTab);

            // Get list of modules
            _moduleCollection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ModuleCollection>();
        }

        private void Start()
        {
            // Create menu modules
            CreateMenuModule(chassisHolder.transform, _moduleCollection.chassis);
            CreateMenuModule(energyHolder.transform, _moduleCollection.energy);
            CreateMenuModule(wheelsHolder.transform, _moduleCollection.wheels);
            CreateMenuModule(actuatorHolder.transform, _moduleCollection.actuators);
        }

        /// <summary>
        /// Creates a menu modules for each module in modulesList
        /// </summary>
        /// <param name="moduleHolder">Parent holder for the menu modules</param>
        /// <param name="modulesList">List of modules to be created</param>
        private void CreateMenuModule(Transform moduleHolder, List<GameObject> modulesList)
        {
            _moduleYDisplacement = initialPlacement.position.y;
            foreach (var moduleObject in modulesList)
            {
                var scale = Camera.main.scaledPixelHeight / 20;
                var module = moduleObject.GetComponent<VehicleModule>();

                // Calculate y displacement from half of the module's height
                _moduleYDisplacement -= module.Size.y * scale;

                // Instantiate menu module into world space from canvas space
                var spawnPoint = new Vector3(initialPlacement.position.x, _moduleYDisplacement, 0);
                var menuModuleObject = Instantiate(moduleObject, Camera.main.ScreenToWorldPoint(spawnPoint), Quaternion.identity, moduleHolder);
                menuModuleObject.transform.localScale /= 2;

                // Translate object so that its on the correct plane and is centered in the list
                menuModuleObject.transform.Translate(-module.Size.x / 4.0f, 0, -menuModuleObject.transform.position.z);

                // Adding small gap for next module
                _moduleYDisplacement -= scale;

                // Add menu module component
                var draggable = menuModuleObject.AddComponent<DraggableModule>();
                draggable.originalPrefab = moduleObject;
                draggable.forceIndicatorPrefab = forceIndicatorPrefab;
                draggable.buttonSounds = buttonSounds;

                // TODO: disable vehicle module functions
                menuModuleObject.GetComponent<VehicleModule>().Freeze();
            }
        }

        /// <summary>
        /// Function called by UI buttons to change module tab
        /// </summary>
        /// <param name="tabTypes">String of tab to change to</param>
        public void SwitchTab(string tabTypes)
        {
            switch (tabTypes)
            {
                case "Wheels":
                    EnableUI(wheelsHolder);
                    break;
                case "Energy":
                    EnableUI(energyHolder);
                    break;
                case "Chassis":
                    EnableUI(chassisHolder);
                    break;
                case "Actuator":
                    EnableUI(actuatorHolder);
                    break;
            }
        }
    
        /// <summary>
        /// Function called by UI button to turn on and off the controls panel
        /// </summary>
        public void ToggleControls()
        {
            if (controlsButton.isOn)
                controlsPanel.SetActive(true);
            else
                controlsPanel.SetActive(false);
        }

        /// <summary>
        /// Disables the current module menu and enables the new one
        /// </summary>
        /// <param name="menu">Menu to be enabled</param>
        private void EnableUI(GameObject menu)
        {
            _currentTab.SetActive(false);
            menu.SetActive(true);
            _currentTab = menu;
        }
    }
}
