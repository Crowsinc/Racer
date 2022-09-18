using UnityEngine;

namespace Level
{
    public class FinishFlag : MonoBehaviour
    {
        private SimulationController _sc;
        private Transform _opponent;

        private bool _finished;

        private void Awake()
        {
            _finished = false;
            GameObject scripts = GameObject.FindGameObjectWithTag("GameController");
            if (!scripts)
            {
                _finished = true;
                return;
            }
            _sc = scripts.GetComponent<SimulationController>();
            _sc.raceFinishPoint = transform.position;
        }

        private void Update()
        {
            if (_finished) return;
            if (!_opponent)
            {
                if (!_sc.opponentInstance)
                    return;

                _opponent = _sc.opponentInstanceTransform;
            }
            if (_opponent.position.x > _sc.raceFinishPoint.x)
            {
                _sc.LoseRace();
                _finished = true;
            } 
            else if (_sc.playerVehicle.transform.position.x > _sc.raceFinishPoint.x)
            {
                _sc.WinRace();
                _finished = true;
            }
        }
    }
}
