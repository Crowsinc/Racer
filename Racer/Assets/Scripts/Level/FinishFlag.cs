using UnityEngine;

namespace Level
{
    public class FinishFlag : MonoBehaviour
    {
        private SimulationController sc;
        private Transform opponent;

        private void Awake()
        {
            sc = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
            sc.raceFinishPoint = transform.position;
        }

        private void Update()
        {
            if (opponent == null)
            {
                if (sc.opponentInstance == null)
                    return;

                opponent = sc.opponentInstance.transform.Find("Vehicle");
            }
            if (opponent.position.x > sc.raceFinishPoint.x)
            {
                sc.LoseRace();
            } 
            else if (sc.playerVehicle.transform.position.x > sc.raceFinishPoint.x)
            {
                sc.WinRace();
            }
        }

        /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sc.WinRace();
        }
        else if (collision.CompareTag("Opponent"))
        {
            sc.LoseRace();
        }
    }*/
    }
}
