using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public int levelId;
    public GameObject terrain;
    public GameObject opponentVehicle;
    public float budget;
    public int highScore;
     
    public void SetHighScore(int score)
    {
        if (score < highScore) return;

        highScore = score;
        PlayerPrefs.SetInt("LevelHS" + levelId.ToString(), score);
    }

    private void OnEnable()
    {
        highScore = PlayerPrefs.GetInt("LevelHS" + levelId.ToString(), 0);
    }
}
