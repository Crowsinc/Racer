using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildStats : MonoBehaviour
{
    private Level _level;
    private VehicleConstructor vehicleConstructor;
    public TextMeshProUGUI text;
    private int _moduleCount;

    void Start()
    {
        LevelInitialiser _levelInitialiser = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelInitialiser>();
        _level = _levelInitialiser.selectedLevel;
        vehicleConstructor = _levelInitialiser.GetComponent<VehicleConstructor>();
        UpdateStats();
    }

    // Update is called once per frame
    void Update()
    {
        if (_moduleCount != vehicleConstructor.ModuleCount())
        {
            UpdateStats();
            _moduleCount = vehicleConstructor.ModuleCount();
        }
    }

    private void UpdateStats()
    {
        text.text = _level.name + "\n" +
            "Budget: $" + _level.budget + "\n" +
            "Total Vehicle Cost: $" + vehicleConstructor.SumVehicleCost().ToString();
    }
}
