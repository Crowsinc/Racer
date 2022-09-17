using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    public class LevelInitialiser : MonoBehaviour
    {
        public Level selectedLevel;
        public List<Level> levelCollection;

        private void Awake()
        {
            selectedLevel = FindLevelById(PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL));

            // Creating terrain
            Instantiate(selectedLevel.terrain, Vector3.zero, Quaternion.identity);

            GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>().opponentVehicle = selectedLevel.opponentVehicle;
            Physics2D.gravity = selectedLevel.gravity;
        }

        // Find level in collection that matches id
        private Level FindLevelById(int id)
        {
            foreach (var level in levelCollection.Where(level => level.levelId == id))
            {
                return level;
            }

            return levelCollection[0];
        }
    }
}
