using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInitialiser : MonoBehaviour
{
    private Level selectedLevel;
    public List<Level> levelCollection;
    void Awake()
    {
        PlayerPrefs.SetInt("SelectedLevel", 1); // Temporarily make selected level 1
        selectedLevel = FindLevelById(PlayerPrefs.GetInt("SelectedLevel"));

        // Creating terrain
        Instantiate(selectedLevel.terrain, Vector3.zero, Quaternion.identity);
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
