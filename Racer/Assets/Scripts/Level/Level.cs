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
        public int highScore;
        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        public List<LevelRestrictions> restrictions = new List<LevelRestrictions>();
        public List<ParallaxBackground> backgrounds = new List<ParallaxBackground>();

        public void SetHighScore(int score)
        {
            if (score < highScore) return;

            highScore = score;
            PlayerPrefs.SetInt("LevelHS" + levelId.ToString(), score);
        }

        private void OnEnable()
        {
            highScore = PlayerPrefs.GetInt("LevelHS" + levelId.ToString(), 0);
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
