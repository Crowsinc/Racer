using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
    
public class MainMenu : MonoBehaviour
{
    public string levelSelect;

    public GameObject optionsScreen;
    public GameObject levelSelectScreen;
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 1) == 0)
        {
            //Level Select button leads here
            
            //If the selected level is 0, that means we are coming back from the simulation screen and need to load the
            //level selection screen.
            //We also need to reset the flag.
            PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, 1);
            PlayGame();
        }
        
        if (PlayerPrefs.GetInt(GameConstants.LOAD_LEVEL_SCREEN, 0) == 1)
        {
            // Next Level button leads here
            
            //If the LOAD LEVEL SCREEN is 1, that means we are coming back from the simulation screen and need to load
            //the level selection screen AND the level screen for the next selected level.
            //We don't need to reset the flag yet.
            PlayGame();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PlayGame()
    {
        levelSelectScreen.SetActive(true);
    }
    
    public void GoBack()
    {
        levelSelectScreen.SetActive(false);
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }
}
