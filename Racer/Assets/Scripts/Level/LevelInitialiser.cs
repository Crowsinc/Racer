using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public class LevelInitialiser : MonoBehaviour
    {
        public global::Level.Level selectedLevel;
        public List<global::Level.Level> levelCollection;
        void Awake()
        {
            selectedLevel = FindLevelById(PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL));

            // Creating terrain
            Instantiate(selectedLevel.terrain, Vector3.zero, Quaternion.identity);

            GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>().opponentVehicle = selectedLevel.opponentVehicle;
            Physics2D.gravity = selectedLevel.gravity;
        }

        // Find level in collection that matches id
        private global::Level.Level FindLevelById(int id)
        {
            foreach (global::Level.Level level in levelCollection)
            {
                if (level.levelId == id)
                {
                    return level;
                }
            }
            return levelCollection[0];
        }
    }
}
