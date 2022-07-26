using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.U2D;
using Level;

public class LevelScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text levelTitle;
    public TMP_Text levelScore;
    public TMP_Text levelDesc;
    public List<Level.Level> levelCollection = new List<Level.Level>();
    public Transform terrainRenderer;
    public GameObject impedimentList;

    private Level.Level level;
    private GameObject levelPreview;

    private void Start()
    {
        // Calling initLevelScreen() causes the first chosen map to be created twice,
        // Going back would delete only one map, the other was loaded forever.
        if (PlayerPrefs.GetInt(GameConstants.LOAD_LEVEL_SCREEN, 0) == 1)
        {
            PlayerPrefs.SetInt(GameConstants.LOAD_LEVEL_SCREEN, 0);
            initLevelScreen();
        }
    }

    public void initLevelScreen()
    {
        int levelNum = PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        levelTitle.text = "Level " + levelNum.ToString();
        level = FindLevelById(levelNum);
        
        //This method works better because there is a default assignable value even though level.highScore is reachable
        // var score = PlayerPrefs.GetFloat("LevelHS" + levelNum.ToString(), 0);
        var score = level.bestTime;
        var cost = level.bestCost;
        if (score > 0)
        {
            levelScore.text = "Best Time: \n" + Timer.TimeToString(score) + "\n \n Lowest Cost:\n $" + cost;
        }
        else
        {
            levelScore.text = "Best Time: \n" + "N/A" + "\n \n Lowest Cost:\n N/A";
        }
        
        //Can add description to level screen here
        levelDesc.text = level.levelDescription;
        CreateLevelPreview();
        SetImpedimentList();
    }
    
    public void LoadGameScene()
    {
        //PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, level.levelId);
        SceneManager.LoadSceneAsync(GameConstants.SIMULATION_SCENE_ID);
    }

    // Find level in collection that matches id
    private Level.Level FindLevelById(int id)
    {
        foreach (Level.Level level in levelCollection)
        {
            if (level.levelId == id)
            {
                return level;
            }
        }
        return levelCollection[0];
    }

    private void CreateLevelPreview()
    {
        // Debug.Log("HMMMM");
        levelPreview = Instantiate(level.terrain, new Vector3(-50, -50, 0), Quaternion.identity);   
        Vector3 startPos = levelPreview.transform.Find("Start").localPosition;
        Vector3 endPos = levelPreview.transform.Find("Flag").localPosition;
        terrainRenderer.position = new Vector3(-50 + startPos.x + endPos.x / 2, -50, -10);
        terrainRenderer.GetComponent<Camera>().orthographicSize = endPos.x / 2;
    }

    private void SetImpedimentList()
    {
        var impediments = level.terrain.GetComponent<LevelTerrain>().GetImpedimentList();
        for (var i = 0; i < impedimentList.transform.childCount; i++)
        {
            impedimentList.transform.GetChild(i).gameObject.SetActive(impediments.Contains(i));
        }
    }

    public void RemoveLevelPreview()
    {
        Destroy(levelPreview);
    }
}
