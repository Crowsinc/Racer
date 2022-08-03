using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }


    [SerializeField] private float pushForce = 1.5f;
    [SerializeField] private bool enable = false;
    [SerializeField] private Direction direction;
    [SerializeField] private Rigidbody2D rb;

    private SpriteRenderer _sp;

    private void Start()
    {
        _sp = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        UpdateColours();
        if (!enable) return;

        switch (direction)
        {
          case Direction.Left:
              rb.AddForce(transform.right * pushForce);
              break;
          
          default:
              break;
        }
    }

    private void UpdateColours()
    {
        if (enable)
        {
            _sp.color = Color.red;
            return;
        }
        _sp.color = Color.white;
    }

    public void EnableThrusters()
    {
        enable = true;
    }

    public void DisableThrusters()
    {
        enable = false;
    }
}   
