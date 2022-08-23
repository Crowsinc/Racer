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

    private Dictionary<string, Rigidbody2D> rbs;

    public void Start()
    {
        startIndex = startPoint.GetComponent<NodeAttach>().index;
        endIndex = endPoint.GetComponent<NodeAttach>().index;

        rbs = new Dictionary<string, Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        var parent = collision.transform.root;
        // Return if collided object is not a vehicle
        if (parent.gameObject.layer != 30) return;

        // Only run the player for now;
        if (parent.name != "PlayerVehicle") return;

        if (!rbs.ContainsKey(parent.name))
        {
            rbs[parent.name] = parent.GetComponent<Rigidbody2D>();
        }

        switch (CheckWhichTerrain(parent.position))
        {
            case 0:
                // What happens if vehicle is touching Grass
                break;
            case 1:
                // What happens if vehicle is touching Mud
                break;
            case 2:
                rbs[parent.name].AddForce(new Vector2(0, 10000));
                break;
            case 3:
                rbs[parent.name].AddForce(new Vector2(rbs[parent.name].velocity.x * 100, 0));
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
