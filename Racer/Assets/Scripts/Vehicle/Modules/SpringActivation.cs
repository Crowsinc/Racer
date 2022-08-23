using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringActivation : MonoBehaviour
{
    public ActuatorModule actuator;
    private bool canActivate;

    // Start is called before the first frame update
    void Start()
    {
        canActivate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canActivate) return;
        if (Input.GetKeyDown(KeyCode.Space))
            actuator.TryActivate(1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            canActivate = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
            canActivate = false;
    }
}
