using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/New Level")]
public class Level : ScriptableObject
{
    public string name;
    public int levelId;
    public GameObject terrain;
    public GameObject opponentVehicle;
    public float budget;
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    
    public List<LevelRestrictions> restritions = new List<LevelRestrictions>();
    public void SetHighScore(float score)
    {
        PlayerPrefs.SetFloat("LevelHS" + levelId.ToString(), score);
    }
    public float GetHighScore()
    {
        return PlayerPrefs.GetFloat("LevelHS" + levelId.ToString());
    }
}

[System.Serializable]
public class LevelRestrictions
{
    public enum RestrictionType
    {
        LessThan,
        EqualTo,
        AtLeast
    }
    public RestrictionType restrictionType;
    public int amount;
    public VehicleModule module;
}
