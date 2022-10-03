using UnityEngine;

namespace Level
{
    public class FinishFlag : MonoBehaviour
    {
        private SimulationController _sc;
        private Transform _opponent;

        private bool _finished;
        private bool _inMainMenu;

        private void Awake()
        {
            _finished = false;
            GameObject scripts = GameObject.FindGameObjectWithTag("GameController");
            if (!scripts)
            {
                _inMainMenu = true;
                return;
            }
            _sc = scripts.GetComponent<SimulationController>();
            _sc.raceFinishPoint = transform.position;
        }

        private void Update()
        {
            if (_inMainMenu)
            {
                return;
            }

            if (_sc.inBuildMode)
            {
                _finished = false;
            }

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
