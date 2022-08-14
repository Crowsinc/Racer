using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int levelId;
    public GameObject terrain;
    public GameObject opponentVehicle;
    public float budget;

    public void SetHighScore(float score)
    {
        PlayerPrefs.SetFloat("LevelHS" + levelId.ToString(), score);
    }
    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat("LevelHS" + levelId.ToString());
    }
}
