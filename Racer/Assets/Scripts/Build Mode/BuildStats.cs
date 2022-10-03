using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<Vector2Int, ModuleSchematic> _currentDesign;

        private SimulationController _sc;

        private void Start()
        {
            var levelInitialiser = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelInitialiser>();
            _sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
            _level = levelInitialiser.selectedLevel;
            _vehicleConstructor = levelInitialiser.GetComponent<VehicleConstructor>();
            _currentDesign = new Dictionary<Vector2Int, ModuleSchematic>();
            UpdateStats();
        }

        // Update is called once per frame
        private void Update()
        {
            // if (_moduleCount == _vehicleConstructor.ModuleCount()) return;
            
            var newDesign = _vehicleConstructor.GetDesign();
            var sameDesign = 
                _currentDesign.Keys.Count == newDesign.Keys.Count &&
                _currentDesign.Keys.All(k => newDesign.ContainsKey(k) && Equals(newDesign[k], _currentDesign[k]));

            if (sameDesign) return;
            UpdateStats();
            // _moduleCount = _vehicleConstructor.ModuleCount();
            _currentDesign = new Dictionary<Vector2Int, ModuleSchematic>(newDesign);
        }

        private void UpdateStats()
        {
            var cost = _vehicleConstructor.SumVehicleCost();
           
            text.text = "Restrictions:";
            // Shows visuals of whether vehicle is below budget
            var belowBudget = cost <= _level.budget;
            
            _sc.validDesign = belowBudget;

            var costMessage = $"Vehicle Cost below ${_level.budget}";
            
            if (belowBudget) 
                text.text += $"\n<color=green><s>{costMessage}</s></color>";
            else
                text.text += $"\n<color=red><b>{costMessage}</b></color>";
            
            // Shows status of vehicle connectivity
            const string connectMessage = "Vehicle has to be connected";
            
            if (_vehicleConstructor.vehicleCore.ValidateDesign(_vehicleConstructor.GetDesign()).ValidDesign)
                text.text += $"\n<color=green><s>{connectMessage}</s></color>";
            else
                text.text += $"\n<color=red><b>{connectMessage}</b></color>";
            



            // Shows status of other restrictions
            if (_level.restrictions.Count <= 0) return;
            foreach (LevelRestrictions restriction in _level.restrictions)
            {
                var valid = restriction.PassesRestrictions(_vehicleConstructor.GetDesign());
                
                var restrictMessage = "";
                switch (restriction.restrictionType)
                {
                    case LevelRestrictions.RestrictionType.EqualTo:
                        restrictMessage = "Exactly " + restriction.amount + " " + restriction.module.Name;
                        break;
                    case LevelRestrictions.RestrictionType.Maximum:
                        restrictMessage = "No more than " + restriction.amount + " " + restriction.module.Name;
                        break;
                    case LevelRestrictions.RestrictionType.Minimum:
                        restrictMessage = "At least " + restriction.amount + " " + restriction.module.Name;
                        break;
                }
                
                if (!valid)
                    text.text += $"\n<color=red><b>{restrictMessage}</b></color>";
                else
                    text.text += $"\n<color=green><s>{restrictMessage}</s></color>";

                _sc.validDesign &= valid;
            }

        }
    }
}
