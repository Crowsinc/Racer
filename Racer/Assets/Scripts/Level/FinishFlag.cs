using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishFlag : MonoBehaviour
{
    private SimulationController sc;

    private void Awake()
    {
        sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        sc.raceFinishPoint = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sc.WinRace();
        }
        else if (collision.CompareTag("Opponent"))
        {
            sc.LoseRace();
        }
    }
}
