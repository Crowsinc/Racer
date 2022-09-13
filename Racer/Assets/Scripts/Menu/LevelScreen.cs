using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text levelTitle;
    public TMP_Text levelScore;
    public TMP_Text levelDesc;
    public List<Level> levelCollection = new List<Level>();
    public Transform terrainRenderer;
    private Level level;
    private GameObject levelPreview;

    public void initLevelScreen()
    {
        int levelNum = PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        levelTitle.text = "Level " + levelNum.ToString();
        level = FindLevelById(levelNum);
        
        //This method works better because there is a default assignable value even though level.highScore is reachable
        int score = PlayerPrefs.GetInt("LevelHS" + levelNum.ToString(), 0);
        if (score > 0)
        {
            levelScore.text = "Stats:\n \n Best Time: \n" + score + "\n \n Lowest Cost:\n $800";
        }
        else
        {
            levelScore.text = "Stats:\n \n Best Time: \n" + "N/A" + "\n \n Lowest Cost:\n $800";
        }
        
        //Can add description to level screen here
        levelDesc.text = "Level Description goes here. We can include the lore of the level, etc!";
        CreateLevelPreview();
    }
    
    public void LoadGameScene()
    {
        //PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, level.levelId);
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

    private void CreateLevelPreview()
    {
        Debug.Log("HMMMM");
        levelPreview = Instantiate(level.terrain, new Vector3(-50, -50, 0), Quaternion.identity);   
        Vector3 startPos = levelPreview.transform.Find("Start").localPosition;
        Vector3 endPos = levelPreview.transform.Find("Flag").localPosition;
        terrainRenderer.position = new Vector3(-50 + startPos.x + endPos.x / 2, -50, -10);
        terrainRenderer.GetComponent<Camera>().orthographicSize = endPos.x / 2;
    }

    public void RemoveLevelPreview()
    {
        Destroy(levelPreview);
    }
}
