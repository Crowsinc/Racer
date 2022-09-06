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
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
     
    public void SetHighScore(float score)
    {
        PlayerPrefs.SetFloat("LevelHS" + levelId.ToString(), score);
    }
    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat("LevelHS" + levelId.ToString());
    }
}
