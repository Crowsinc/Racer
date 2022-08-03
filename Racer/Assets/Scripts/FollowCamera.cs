using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private GameObject car;

    private void LateUpdate()
    {
        var position = car.transform.position;
        var newXPosition = position.x;
        var newYPosition = position.y;
 
        transform.position = new Vector3(newXPosition, newYPosition, transform.position.z);
    }
}
