using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform buildModeCamPos;
    public Transform Target;
    public Transform MapStart;
    public Transform MapEnd;

    private void Awake()
    {
        Target = buildModeCamPos;
        var map = GameObject.FindGameObjectWithTag("Ground");
        MapStart = map.transform.Find("Start");
        MapEnd = map.transform.Find("Flag");

        // Call update once to initialise in correct position
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        var half = (Camera.main.orthographicSize * Camera.main.aspect);

        // NOTE: old lerp system causes very annoying skipping when the vehicle moves too fast
        transform.position = new Vector3(
            Target.position.x,
            Target.position.y,
            -10f
        );

        var yPos = transform.position.y;
        if (transform.position.x - half < MapStart.position.x)
        {
            if (yPos < MapStart.position.y && Target.position.x < MapStart.position.x)
                yPos = MapStart.position.y;
            transform.position = new Vector3(MapStart.position.x + half, yPos, transform.position.z);
        }
        else if (Target.position.x + half > MapEnd.position.x)
        {
            if (yPos < MapEnd.position.y && Target.position.x > MapEnd.position.x)
                yPos = MapEnd.position.y;
            transform.position = new Vector3(MapEnd.position.x - half, yPos, transform.position.z);
        }

    }
}
