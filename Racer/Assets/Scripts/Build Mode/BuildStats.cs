using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildStats : MonoBehaviour
{
    private Level.Level _level;
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
        text.text = _level.levelName + "\n" +
            "Budget: $" + _level.budget + "\n" +
            "Total Vehicle Cost: $" + vehicleConstructor.SumVehicleCost().ToString();

        if (_level.restrictions.Count > 0)
        {
            text.text += "\n\nRestrictions:";
        }
        foreach (LevelRestrictions restriction in _level.restrictions)
        {
            switch (restriction.restrictionType)
            {
                case LevelRestrictions.RestrictionType.EqualTo:
                    text.text += "\nExcatly " + restriction.amount + " " + restriction.module.Name;
                    break;
                case LevelRestrictions.RestrictionType.Maximum:
                    text.text += "\nNo more than " + restriction.amount + " " + restriction.module.Name;
                    break;
                case LevelRestrictions.RestrictionType.Minimum:
                    text.text += "\nAt least " + restriction.amount + " " + restriction.module.Name;
                    break;
            }
        }
    }
}
