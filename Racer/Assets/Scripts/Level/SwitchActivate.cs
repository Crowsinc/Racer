using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchActivate : MonoBehaviour
{
    public GameObject item;

    [Header("Rotation")]
    public float startRotation;
    public float finalRotation;

    [Header("Position")]
    public Vector2 startPosition;
    public Vector2 finalPosition;

    public float timeToDesitnation;

    private float t;
    private Vector3 startRot;
    private Vector3 finalRot;

    private bool isActivated;
    private bool stop;


    // Start is called before the first frame update
    void Start()
    {
        isActivated = stop = false;
        item.transform.position = startPosition;
        item.transform.eulerAngles = startRot = new Vector3(0, 0, startRotation);

        finalRot = new Vector3(0, 0, finalRotation);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isActivated) return;
        if (stop) return;
        t += Time.deltaTime / timeToDesitnation;
        item.transform.position = Vector2.Lerp(startPosition, finalPosition, t);
        item.transform.eulerAngles = Vector3.Lerp(startRot, finalRot, t);
        if (t >= 1)
            stop = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated) return;
        isActivated = true;
        var switchScale = transform.localScale;
        transform.localScale = new Vector3(switchScale.x * -1, switchScale.y, switchScale.z);
        t = 0;
    }
}


