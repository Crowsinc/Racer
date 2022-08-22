using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LevelTerrain : MonoBehaviour
{
    [Header("TerrainPoints")]
    public SpriteShapeController spriteShapeController;
    public GameObject startPoint;
    public GameObject endPoint;

    [Header("Friction Values")]
    public float grass;
    public float ice;

    private int startIndex;
    private int endIndex;

    private bool isGrounded;

    public void Start()
    {
        startIndex = startPoint.GetComponent<NodeAttach>().index;
        endIndex = endPoint.GetComponent<NodeAttach>().index;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var parent = collision.transform.root;
        // Return if collided object is not a vehicle
        if (parent.gameObject.layer != 30) return;

        // Only run for player for now;
        if (parent.name != "PlayerVehicle") return;

        switch (CheckWhichTerrain(parent.position))
        {
            case 0:
                // What happens if vehicle is touching Grass
                break;
            case 1:
                // What happens if vehicle is touching Mud
                break;
        }

    }

    private int CheckWhichTerrain(Vector2 position)
    {
        for (var i = startIndex; i < endIndex; i++)
        {
            if (spriteShapeController.spline.GetPosition(i).x < position.x) continue;
            return spriteShapeController.spline.GetSpriteIndex(i - 1);
        }
        return 0;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collision is with vehicle component

        // Change component rb drag
    }
}
