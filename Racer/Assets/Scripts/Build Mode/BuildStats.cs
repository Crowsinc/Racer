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

        private void Start()
        {
            var levelInitialiser = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelInitialiser>();
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

            if (_level.restrictions.Count > 0)
            {
                text.text += "\n\nRestrictions:";
            }
            foreach (LevelRestrictions restriction in _level.restrictions)
            {
                switch (restriction.restrictionType)
                {
                    case LevelRestrictions.RestrictionType.EqualTo:
                        text.text += "\nExactly " + restriction.amount + " " + restriction.module.Name;
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
}
