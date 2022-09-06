using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Transform MapStart;
    public Transform MapEnd;

    private void Start()
    {
        var map = GameObject.FindGameObjectWithTag("Ground");
        MapStart = map.transform.Find("Start");
        MapEnd = map.transform.Find("Flag");

        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        var half = (Camera.main.orthographicSize * Camera.main.aspect);
        if (transform.position.x - half < MapStart.position.x)
        {
            //transform.position = new Vector3(MapStart.position.x + half, transform.position.y, transform.position.z);
        }
        else if (transform.position.x + half > MapEnd.position.x)
        {
            transform.position = new Vector3(MapEnd.position.x - half + 0.001f, transform.position.y, transform.position.z);
            return;
        }

        transform.position = Vector3.Lerp(
            new Vector3(transform.position.x, transform.position.y, -10.0f),
            new Vector3(Target.position.x, Target.position.y, -10.0f),
            0.1f
        );
        
    }
}
