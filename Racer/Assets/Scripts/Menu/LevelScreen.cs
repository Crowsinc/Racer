using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text levelTitle;
    public List<Level> levelCollection = new List<Level>();
    private Level level;
    
    void Start()
    {
        
    }

    public void initLevelScreen(int levelNum)
    {
        levelTitle.text = "Level " + levelNum.ToString();
        level = FindLevelById(levelNum);
    }
    
    public void LoadGameScene()
    {
        PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, level.levelId);
        SceneManager.LoadSceneAsync(GameConstants.SIMULATION_SCENE_ID);
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
