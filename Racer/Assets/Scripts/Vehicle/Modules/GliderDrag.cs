using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderDrag : MonoBehaviour
{
    public float drag = 3f;
    public Rigidbody2D rb;

    void FixedUpdate()
    {
        if (rb == null)
        {
            var core = GetComponentInParent<VehicleCore>();
            if (core != null)
                rb = core.Rigidbody;
        }
        else
        {
            var rotation = Mathf.Abs(rb.transform.rotation.z);
            if (rotation < 0.5)
                rotation = 1 - rotation;
            rb.AddForce(new Vector2(0, -rb.velocity.y * drag * (2 * rotation - 1)));
        }
    }
}
