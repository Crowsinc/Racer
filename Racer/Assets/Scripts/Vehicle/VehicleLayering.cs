using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleLayering : MonoBehaviour
{
    public float opponentAlpha = 0.5f;
    private bool isEnemy;

    // Start is called before the first frame update
    void Start()
    {
        isEnemy = GetComponent<VehicleCore>().Pregenerated;
        if (isEnemy)
        {
            Debug.Log("Test");
            foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingLayerName = "Opponent Vehicle";
                var temp = renderer.color;
                temp.a = opponentAlpha;
                renderer.color = temp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
