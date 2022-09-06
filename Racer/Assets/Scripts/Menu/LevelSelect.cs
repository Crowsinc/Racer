using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject levelScreen;
    
    public Button[] levelButtons;
    void Start()
    {
        initLevelSelect();
        
        if (PlayerPrefs.GetInt(GameConstants.LOAD_LEVEL_SCREEN, 0) == 1)
        {
            //If the LOAD LEVEL SCREEN is 1, that means we are coming back from the simulation screen and need to load
            //the level selection screen AND the level screen for the next selected level.
            //We also need to reset the flag.
            PlayerPrefs.SetInt(GameConstants.LOAD_LEVEL_SCREEN, 0);
            OpenLevelScreen();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void initLevelSelect()
    {

        //Resetting the level availability
        foreach (Button button in levelButtons)
        {
            button.interactable = false;
        }
        levelButtons[0].interactable = true;
        
        //Get the current player progression
        int currentLevel = PlayerPrefs.GetInt(GameConstants.LEVEL_UNLOCKED, 1);
        if (currentLevel >= 2)
        {
            int i = 0;
            while (i < currentLevel && i < levelButtons.Length)
            {
                levelButtons[i].interactable = true;
                i += 1;
            }
        }
    }

    public void ChooseLevel(int level)
    {
        PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, level);
        OpenLevelScreen();
    }

    public void OpenLevelScreen()
    {
        levelScreen.SetActive(true);
    }

    public void CloseLevelScreen()
    {
        levelScreen.GetComponent<LevelScreen>().RemoveLevelPreview();
        levelScreen.SetActive(false);
    }

    public void LockLevel()
    {
        PlayerPrefs.SetInt(GameConstants.LEVEL_UNLOCKED, 1);
    }

    public void UnlockLevel()
    {
        PlayerPrefs.SetInt(GameConstants.LEVEL_UNLOCKED, 10);
    }
}
