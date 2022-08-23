using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderDrag : MonoBehaviour
{
    public float drag = 3f;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var rotation = Mathf.Abs(rb.transform.rotation.z);
        if (rotation < 0.5)
            rotation = 1 - rotation;
        rb.AddForce(new Vector2(0, -rb.velocity.y * drag * (2 * rotation - 1)));
    }
}
