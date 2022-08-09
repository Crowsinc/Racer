using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(
            new Vector3(transform.position.x, transform.position.y, -10.0f), 
            new Vector3(Target.position.x, Target.position.y, -10.0f),
            0.3f
        );

    }
}
