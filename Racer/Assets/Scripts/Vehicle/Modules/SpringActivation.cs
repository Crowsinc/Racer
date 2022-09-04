using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringActivation : MonoBehaviour
{
    public ActuatorModule actuator;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            actuator.TryActivate(1);
    }
}
