using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeUIController : MonoBehaviour
{ 
    public GameObject wheelsUi;
    public GameObject energyUi;
    public GameObject chassisUi;
    public GameObject actuatorUi;
    public GameObject draggablePrefab;

    private GameObject _currentTab;
    private ModuleCollection _moduleCollection;

    private void Awake()
    {
        // Initial tab
        _currentTab = wheelsUi;

        _moduleCollection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ModuleCollection>();

        for (int i = 0; i < _moduleCollection.wheels.Count; i++)
        {
            CreateDraggable(i, wheelsUi.transform, _moduleCollection.wheels[i].GetComponent<VehicleModule>().sprite);
        }

        for (int i = 0; i < _moduleCollection.energy.Count; i++)
        {
            CreateDraggable(i, energyUi.transform, _moduleCollection.energy[i].GetComponent<VehicleModule>().sprite);
        }

        for (int i = 0; i < _moduleCollection.chassis.Count; i++)
        {
            CreateDraggable(i, chassisUi.transform, _moduleCollection.chassis[i].GetComponent<VehicleModule>().sprite);
        }

        for (int i = 0; i < _moduleCollection.actuators.Count; i++)
        {
            CreateDraggable(i, actuatorUi.transform, _moduleCollection.actuators[i].GetComponent<VehicleModule>().sprite);
        }
    }

    private void CreateDraggable(int i, Transform uiParent, Sprite sprite)
    {
        GameObject draggable = Instantiate(draggablePrefab, uiParent);
        draggable.transform.Translate(Vector3.down * i * 50f);
        Image draggableImage = draggable.GetComponent<Image>();
        draggableImage.sprite = sprite;
        draggableImage.preserveAspect = true;
        draggableImage.SetNativeSize();
    }

    public void SwitchTab(string tabTypes)
    {
        switch (tabTypes)
        {
            case "Wheels":
                EnableUI(wheelsUi);
                break;
            case "Energy":
                EnableUI(energyUi);
                break;
            case "Chassis":
                EnableUI(chassisUi);
                break;
            case "Actuator":
                EnableUI(actuatorUi);
                break;
        }
    }

    private void EnableUI(GameObject ui)
    {
        _currentTab.SetActive(false);
        ui.SetActive(true);
        _currentTab = ui;
    }
}
