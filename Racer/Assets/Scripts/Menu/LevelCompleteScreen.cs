using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public class LevelCompleteScreen : MonoBehaviour
{

    public TMP_Text levelCompleteTitle, levelCompleteStats;

    public GameObject retryButton, nextLevelButton, levelCompleteScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void initLevelCompleteScreen(bool passed, float time, int cost)
    {
        levelCompleteStats.text = "\n" +
                                 $"Time: {Timer.TimeToString(time)}\n" +
                                 $"Cost: {cost}";
            
        if (passed)
        {
            drawPassed();
        }
        else
        {
            drawFailed();
        }
    }

    public void drawPassed()
    {
        int currentLevel = PlayerPrefs.GetInt(GameConstants.LEVEL_UNLOCKED, 0);
        PlayerPrefs.SetInt(GameConstants.LEVEL_UNLOCKED, currentLevel + 1);
        
        levelCompleteTitle.text = "Level Complete!";
        retryButton.SetActive(false);
        if (PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0) == 2)
        {
            nextLevelButton.SetActive(false);
        }
        else
        {
            nextLevelButton.SetActive(true); 
        }
        
        //Debug.Log("Passed");
    }

    public void drawFailed()
    {
        levelCompleteTitle.text = "Level Failed...";
        retryButton.SetActive(true);
        nextLevelButton.SetActive(false);
        //Debug.Log("Failed");
    }
    
    public void OpenLevelCompleteScreen()
    {
        levelCompleteScreen.SetActive(true);
    }

    public void CloseLevelCompleteScreen()
    {
        levelCompleteScreen.SetActive(false);
    }
    
    public void LevelSelectScreen()
    {
        //quick hack to detect when main menu is returned to
        PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        SceneManager.LoadSceneAsync(GameConstants.MAIN_MENU_SCENE_ID);
    }
    
    public void NextLevelScreen()
    {
        //quick hack to detect when main menu is returned to
        int currentLevel = PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, currentLevel + 1);
        
        PlayerPrefs.SetInt(GameConstants.LOAD_LEVEL_SCREEN, 1);
        SceneManager.LoadSceneAsync(GameConstants.MAIN_MENU_SCENE_ID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
