using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    [CreateAssetMenu(menuName = "Level/New Level")]
    public class Level : ScriptableObject
    {
        public string levelName;
        public int levelId;
        public GameObject terrain;
        public GameObject opponentVehicle;
        public float budget;
        public float bestTime;
        public int bestCost;
        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        public List<LevelRestrictions> restrictions = new List<LevelRestrictions>();
        public List<ParallaxBackground> backgrounds = new List<ParallaxBackground>();

        public void SetNewTime(float time)
        {
            if (time > bestTime && bestTime != 0) return;

            bestTime = time;
            PlayerPrefs.SetFloat("LevelHS" + levelId.ToString(), time);
        }

        public void SetNewCost(int cost)
        {
            if (cost > bestCost && bestCost != (int)budget) return;

            bestCost = cost;
            PlayerPrefs.SetInt("LevelBC" + levelId, cost);
        }

        private void OnEnable()
        {
            bestTime = PlayerPrefs.GetFloat("LevelHS" + levelId, 0);
            bestCost = PlayerPrefs.GetInt("LevelBC" + levelId, (int)budget);
        }
    }

    [System.Serializable]
    public class LevelRestrictions
    {
        public enum RestrictionType
        {
            Maximum,
            EqualTo,
            Minimum
        }
        public RestrictionType restrictionType;
        public int amount;
        public VehicleModule module;

        public bool PassesRestrictions(Dictionary<Vector2Int, ModuleSchematic> design)
        {
            int moduleCount = 0;
            foreach (KeyValuePair<Vector2Int, ModuleSchematic> key in design)
            {
                if (key.Value.Prefab.GetComponent<VehicleModule>().Name == module.Name)
                    moduleCount++;
            }
            switch (restrictionType)
            {
                case RestrictionType.Maximum: return moduleCount <= amount;
                case RestrictionType.EqualTo: return moduleCount == amount;
                case RestrictionType.Minimum: return moduleCount >= amount;
                default:
                    return false;
            }
        }
    }

    [System.Serializable]
    public class ParallaxBackground
    {
        public float parallaxDegree;
        public Sprite image;
        public int layerOrder;
    }
}
