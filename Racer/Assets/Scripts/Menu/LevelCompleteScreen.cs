using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
public class LevelCompleteScreen : MonoBehaviour
{

    public TMP_Text levelCompleteTitle;

    public GameObject retryButton, nextLevelButton, levelCompleteScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public void initLevelCompleteScreen(bool passed)
    {
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
        levelCompleteTitle.text = "Level Complete!";
        retryButton.SetActive(false);
        nextLevelButton.SetActive(true);
        Debug.Log("Passed");
    }

    public void drawFailed()
    {
        levelCompleteTitle.text = "Level Failed...";
        retryButton.SetActive(true);
        nextLevelButton.SetActive(false);
        Debug.Log("Failed");
    }
    
    public void OpenLevelCompleteScreen()
    {
        levelCompleteScreen.SetActive(true);
    }

    public void CloseLevelCompleteScreen()
    {
        levelCompleteScreen.SetActive(false);
    }
    
    public void LoadMenuScene()
    {
        //quick hack to detect when main menu is returned to
        PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        SceneManager.LoadSceneAsync(GameConstants.MAIN_MENU_SCENE_ID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
