using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeUIController : MonoBehaviour
{ 
    public GameObject wheelsUi;
    public GameObject energyUi;
    public GameObject chassisUi;
    public GameObject actuatorUi;

    private GameObject _currentTab;

    private void Awake()
    {
        // Initial tab
        _currentTab = wheelsUi;
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
