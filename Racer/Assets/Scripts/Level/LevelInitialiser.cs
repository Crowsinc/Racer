using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInitialiser : MonoBehaviour
{
    private Level selectedLevel;
    public int select = 1;
    public List<Level> levelCollection;
    void Awake()
    {
        PlayerPrefs.SetInt("SelectedLevel", select); // Temporarily make selected level 1
        selectedLevel = FindLevelById(PlayerPrefs.GetInt("SelectedLevel"));

        // Creating terrain
        Instantiate(selectedLevel.terrain, Vector3.zero, Quaternion.identity);

        GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>().opponentVehicle = selectedLevel.opponentVehicle;
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
