using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelSelect : MonoBehaviour
{
    // Start is called before the first frame update

    public Button[] levelButtons;
    void Start()
    {
        initLevelSelect();
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
        int currentLevel = PlayerPrefs.GetInt("Level", 1);
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
    
    public void lockLevel()
    {
        PlayerPrefs.SetInt("Level", 1);
    }

    public void unlockLevel()
    {
        PlayerPrefs.SetInt("Level", 10);
    }
}
