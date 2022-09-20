using UnityEngine;

namespace Level.ProgressBar
{
    public class ProgressBarUpdate : MonoBehaviour
    {
        public Transform raceProgressBar;
        public Transform opponentProgressBar;

        private float _raceDistance;
        private Vector3 _raceFinishPoint;

        private VehicleCore _playerVehicle;
        private Transform _opponentVehicle;

        private bool _startBar;
        private SimulationController _sc;

        private void Start()
        {
            var scripts = GameObject.FindWithTag("GameController");

            _sc = scripts.GetComponent<SimulationController>();
            _sc.RaceStart += StartBar;
            _sc.RaceFinish += StopBar;
            _sc.InBuildMode += StartBar;
        }

        private void Update()
        {
            if (!_startBar) return;
            var playerProgressBarDistance = Mathf.Min(Mathf.Max((_raceDistance - (_raceFinishPoint.x - _playerVehicle.transform.position.x)) / _raceDistance, 0), 1);
            var opponentProgressBarDistance = Mathf.Min(Mathf.Max((_raceDistance - (_raceFinishPoint.x - _opponentVehicle.position.x)) / _raceDistance, 0), 1);

            // Debug.Log("Player progress: " + playerProgressBarDistance.ToString());
            raceProgressBar.transform.localScale = new Vector3(playerProgressBarDistance, 1, 1);
            opponentProgressBar.transform.localScale = new Vector3(opponentProgressBarDistance, 1, 1);

            if (playerProgressBarDistance > opponentProgressBarDistance)
            {
                opponentProgressBar.transform.SetSiblingIndex(1);
                raceProgressBar.transform.SetSiblingIndex(0);
            }
            else
            {
                raceProgressBar.transform.SetSiblingIndex(1);
                opponentProgressBar.transform.SetSiblingIndex(0);
            }
        }

        private void StartBar()
        {
            _raceFinishPoint = _sc.raceFinishPoint;
            _playerVehicle = _sc.playerVehicle;
            _opponentVehicle = _sc.opponentInstanceTransform;
            _raceDistance = _raceDistance = Vector3.Distance(_playerVehicle.transform.position, _raceFinishPoint);
            _startBar = true;
        }

        private void StopBar()
        {
            _startBar = false;
        }
    }
}
