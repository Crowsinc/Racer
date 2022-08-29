using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text levelTitle;
    public List<Level> allLevels = new List<Level>();
    private Level level;
    
    void Start()
    {
        initLevelScreen();
    }

    public void initLevelScreen()
    {
        int levelNum = PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL, 0);
        levelTitle.text = "Level " + levelNum.ToString();
        level = allLevels[levelNum];
    }
    
    public void LoadGameScene()
    {
        //PlayerPrefs.SetInt(GameConstants.PPKEY_SELECTED_LEVEL, level.levelId);
        SceneManager.LoadSceneAsync(GameConstants.SIMULATION_SCENE_ID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
