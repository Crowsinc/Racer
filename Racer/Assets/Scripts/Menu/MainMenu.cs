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
            //If the selected level is 0, that means we are coming back from the simulation screen.
            //We also need to reset the flag.
            PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, 1);
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
