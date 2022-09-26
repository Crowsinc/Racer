using Level;
using TMPro;
using UnityEngine;

namespace Build_Mode
{
    public class BuildStats : MonoBehaviour
    {
        private Level.Level _level;
        private VehicleConstructor _vehicleConstructor;
        public TextMeshProUGUI text;
        private int _moduleCount;

        private SimulationController _sc;

        private void Start()
        {
            var levelInitialiser = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelInitialiser>();
            _sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
            _level = levelInitialiser.selectedLevel;
            _vehicleConstructor = levelInitialiser.GetComponent<VehicleConstructor>();
            UpdateStats();
        }

        // Update is called once per frame
        private void Update()
        {
            if (_moduleCount == _vehicleConstructor.ModuleCount()) return;
            
            UpdateStats();
            _moduleCount = _vehicleConstructor.ModuleCount();
        }

        private void UpdateStats()
        {
            text.text = _level.levelName + "\n" +
                        "Budget: $" + _level.budget + "\n" +
                        "Total Vehicle Cost: $" + _vehicleConstructor.SumVehicleCost();

            _sc.validDesign = _vehicleConstructor.SumVehicleCost() <= _level.budget;

            if (_level.restrictions.Count > 0)
            {
                text.text += "\n\nRestrictions:";
                foreach (LevelRestrictions restriction in _level.restrictions)
                {
                    var valid = restriction.PassesRestrictions(_vehicleConstructor.GetDesign());
                    if (!valid)
                    {
                        text.text += "\n<color=red>";
                    }
                    else
                    {
                        text.text += "\n<color=green>";
                        text.text += "<s>";
                    }
                    switch (restriction.restrictionType)
                    {
                        case LevelRestrictions.RestrictionType.EqualTo:
                            text.text += "Exactly " + restriction.amount + " " + restriction.module.Name;
                            break;
                        case LevelRestrictions.RestrictionType.Maximum:
                            text.text += "No more than " + restriction.amount + " " + restriction.module.Name;
                            break;
                        case LevelRestrictions.RestrictionType.Minimum:
                            text.text += "At least " + restriction.amount + " " + restriction.module.Name;
                            break;
                    }
                    if (valid)
                        text.text += "</s>";
                    text.text += "</color>";

                    _sc.validDesign &= valid;
                }
            }
            
        }
    }
}
