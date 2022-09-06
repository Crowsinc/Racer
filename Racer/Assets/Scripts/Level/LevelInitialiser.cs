using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInitialiser : MonoBehaviour
{
    private Level selectedLevel;
    public List<Level> levelCollection;
    void Awake()
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
        foreach (Level level in levelCollection)
        {
            if (level.levelId == id)
            {
                return level;
            }
        }
        return levelCollection[0];
    }
}
