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
    private float draggableYDisplacement;

    private void Awake()
    {
        // Initial tab
        _currentTab = wheelsUi;

        _moduleCollection = GameObject.FindGameObjectWithTag("GameController").GetComponent<ModuleCollection>();

        CreateDraggable(wheelsUi.transform, _moduleCollection.wheels);
        CreateDraggable(energyUi.transform, _moduleCollection.energy);
        CreateDraggable(chassisUi.transform, _moduleCollection.chassis);
        CreateDraggable(actuatorUi.transform, _moduleCollection.actuators);
        
    }

    private void CreateDraggable(Transform uiParent, List<GameObject> modulesList)
    {
        draggableYDisplacement = 0f;
        for (int i = 0; i < modulesList.Count; i++)
        {
            GameObject draggable = Instantiate(draggablePrefab, uiParent);
            //draggableYDisplacement += (modulesList[i].GetComponent<VehicleModule>().Size.y) * 100f;
            //draggable.transform.Translate(Vector3.down * draggableYDisplacement + Vector3.down * (modulesList[i].GetComponent<VehicleModule>().Size.y/2) * 100f);

            draggable.transform.Translate(Vector3.down * i * 100f);
            Image draggableImage = draggable.GetComponent<Image>();
            draggableImage.sprite = modulesList[i].GetComponent<VehicleModule>().sprite;
            draggable.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, draggableImage.sprite.rect.height / 2f);
            draggable.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, draggableImage.sprite.rect.width / 2f);
            draggableImage.preserveAspect = true;
            //draggableImage.SetNativeSize();
        }
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
