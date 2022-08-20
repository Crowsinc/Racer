using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class LevelScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text levelTitle;
    
    void Start()
    {
        
    }

    public void initLevelScreen(int level)
    {
        levelTitle.text = "Level " + level.ToString();
        Debug.Log(level.ToString());
    }
    
    public void LoadGameScene()
    {
        //SceneManager.LoadScene("TestScene");
        //untested to see if it works or not
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
