using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeUIController : MonoBehaviour
{ 
    // Containers for menu modules
    public GameObject wheelsHolder;
    public GameObject energyHolder;
    public GameObject chassisHolder;
    public GameObject actuatorHolder;
    public Transform draggableHolder;

    public RectTransform initalPlacement; // Rect in UI space for the initial placement

    private GameObject _currentTab;
    private ModuleCollection _moduleCollection;
    private float moduleYDisplacement;

    private void Awake()
    {
        // Initial tab
        _currentTab = wheelsHolder;
        EnableUI(_currentTab);

        // Get list of modules
        _moduleCollection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ModuleCollection>();

        // Create menu modules
        CreateMenuModule(wheelsHolder.transform, _moduleCollection.wheels);
        CreateMenuModule(energyHolder.transform, _moduleCollection.energy);
        CreateMenuModule(chassisHolder.transform, _moduleCollection.chassis);
        CreateMenuModule(actuatorHolder.transform, _moduleCollection.actuators);
        
    }

    /// <summary>
    /// Creates a menu modules for each module in modulesList
    /// </summary>
    /// <param name="moduleHolder">Parent holder for the menu modules</param>
    /// <param name="modulesList">List of modules to be created</param>
    private void CreateMenuModule(Transform moduleHolder, List<GameObject> modulesList)
    {
        moduleYDisplacement = initalPlacement.position.y;
        for (int i = 0; i < modulesList.Count; i++)
        {
            // Get module from list
            GameObject module = modulesList[i];

            // Calculate y displacement from half of the module's height
            moduleYDisplacement -= (modulesList[i].GetComponent<VehicleModule>().Size.y / 2);

            // Instantiate menu module into world space from canvas space
            Vector3 spawnPoint = new Vector3(initalPlacement.position.x, moduleYDisplacement, 0);
            GameObject menuModule = Instantiate(module, Camera.main.ScreenToWorldPoint(spawnPoint), Quaternion.identity, moduleHolder);
            menuModule.transform.Translate(Vector3.back * menuModule.transform.position.z);
            moduleYDisplacement -= (module.GetComponent<VehicleModule>().Size.y / 2) + 50f;

            // Add menu module component
            menuModule.AddComponent<MenuModule>();
            menuModule.GetComponent<MenuModule>().SetModuleHolder(draggableHolder);
            
            // TODO: disable vehicle module functions
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
