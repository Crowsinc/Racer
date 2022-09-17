using System.Linq;
using TMPro;
using UnityEngine;

namespace Level
{
    public class SimulationController : MonoBehaviour
    {
        public VehicleCore playerVehicle;

        [Header("BuildMode")]
        public GameObject buildModeUI;
        public GameObject buildModeGrid;
        public GameObject buildModeModuleHolder;
        public GameObject moduleInfoDisplay;
        public GameObject moduleExtraStatsDisplay;
        public GameObject moduleNameDisplay;
        public GameObject moduleCostDisplay;
        
        [Header("UI")]
        public TMP_Text timer;
        public GameObject raceUI;
        public GameObject pauseUI;
        public GameObject resetPrompt;
        public GameObject winUI;

        [Header("Progress Bar")]
        public Transform raceProgressBar;
        public Transform opponentProgressBar;
        
        [HideInInspector]
        public GameObject opponentVehicle;
        [HideInInspector]
        public Vector3 raceFinishPoint;

        [Header("Misc")]
        public bool inBuildMode = true;
        public GameObject opponentInstance;
        public Transform opponentInstanceTransform;
        public LevelCompleteScreen levelCompleteScreen;
        
        private Level _level;
        private bool _isFinished;
        private CameraFollow _cameraFollow;
        private float _raceDistance;
        
        private VehicleConstructor _vehicleConstructor;
        private AIController _playerAI;
        private AIController _opponentAI;

        private float _totalTime;

        public delegate void OnRestart();
        public OnRestart LevelRestart;

        private void Awake()
        {
            _cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
            _vehicleConstructor = GetComponent<VehicleConstructor>();
            _vehicleConstructor.vehicleCore = playerVehicle;
            _totalTime = 0;
            Time.timeScale = 1;
        }

        private void Start()
        {
            _level = gameObject.GetComponent<LevelInitialiser>().selectedLevel;
            EnterBuildMode();
        }

        private void Update()
        {
            if (inBuildMode) return;
            if (_isFinished) return;
            _totalTime += Time.deltaTime;
            timer.text = (Mathf.Round(_totalTime * 100) / 100.0).ToString("#.00");
            UpdateProgressBar();
        }

        /// <summary>
        /// Locks the camera into the build mode area and activates the build mode UI
        /// </summary>
        public void EnterBuildMode()
        {
            Time.timeScale = 0;
            inBuildMode = true;
            _totalTime = 0;
            _isFinished = false;
            LevelRestart?.Invoke();

            // Change UI
            buildModeUI.SetActive(true);
            buildModeGrid.SetActive(true);
            buildModeModuleHolder.SetActive(true);
            raceUI.SetActive(false);
            pauseUI.SetActive(false);
            resetPrompt.SetActive(false);
            _vehicleConstructor.ShowUIElements();


            // Resetting player vehicle
            if(playerVehicle.IsBuilt)
            {
                var playerVehicleTransform = playerVehicle.transform;
                playerVehicleTransform.position = new Vector3(0, 3, 0);
                playerVehicleTransform.rotation = Quaternion.identity;

                Rigidbody2D rb = playerVehicle.GetComponent<Rigidbody2D>();
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;

                playerVehicle.ClearStructure();
                _playerAI.StopSimulating();
            }
            DestroyImmediate(opponentInstance, true);
        }

        /// <summary>
        /// Hides the build mode UI and begins the simulation
        /// </summary>
        public void StartRace()
        {
            // Check that design meets cost and module restrictions, and is valid
            var validDesign = _level.budget >= _vehicleConstructor.SumVehicleCost();
            //validDesign &= _vehicleConstructor.ValidateRestrictions(_level.restrictions);
            validDesign &= playerVehicle.TryBuildStructure(_vehicleConstructor.GetDesign());

            if (validDesign)
            {
                Time.timeScale = 1;
                inBuildMode = false;
                _cameraFollow.target = playerVehicle.transform;

                // Change UI
                _vehicleConstructor.HideUIElements();
                buildModeUI.SetActive(false);
                buildModeGrid.SetActive(false);
                buildModeModuleHolder.SetActive(false);
                raceUI.SetActive(true);

                _raceDistance = Vector3.Distance(playerVehicle.transform.position, raceFinishPoint);

                // Build the opponent, ensuring its on the opponent vehicle layer
                opponentInstance = Instantiate(opponentVehicle, new Vector3(0, 3, 0), Quaternion.identity);
                opponentInstance.layer = LayerMask.NameToLayer("Opponent Vehicle");
                opponentInstanceTransform = opponentInstance.transform.Find("Vehicle");
            
                // Unfreeze player vehicle
                playerVehicle.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

                // Start the AI simulation
                _opponentAI = opponentInstance.GetComponentInChildren<AIController>();
                if (_opponentAI != null)
                    _opponentAI.StartSimulating();
                else
                    Debug.LogError("Opponent vehicle has no AI");

                if (playerVehicle.gameObject.TryGetComponent(out _playerAI))
                    _playerAI.StartSimulating();
                else
                    Debug.LogError("Player vehicle has no AI");
        
            }
            else {
                Debug.LogWarning("Player Vehicle Failed Validation");
                playerVehicle.ClearStructure();
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Halts the simulation and displays the 'Win' UI
        /// </summary>
        public void WinRace()
        {
            foreach (var level in GetComponent<LevelInitialiser>().levelCollection.Where(level => PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL) == level.levelId))
            {
                level.SetHighScore(Mathf.RoundToInt(CalculateScore()));
            }
            raceUI.SetActive(false);
            winUI.SetActive(true);
            levelCompleteScreen.initLevelCompleteScreen(true, CalculateScore(), (int)_vehicleConstructor.SumVehicleCost());

            _playerAI.StopSimulating();
            _opponentAI.StopSimulating();
            _isFinished = true;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void LoseRace()
        {
            _isFinished = true;
            raceUI.SetActive(false);
            winUI.SetActive(true);
            levelCompleteScreen.initLevelCompleteScreen(false, CalculateScore(), (int)_vehicleConstructor.SumVehicleCost());
        }

        private void UpdateProgressBar()
        {
            var playerProgressBarDistance = Mathf.Min(Mathf.Max((_raceDistance - (raceFinishPoint.x - playerVehicle.transform.position.x)) / _raceDistance, 0), 1);
            var opponentProgressBarDistance = Mathf.Min(Mathf.Max((_raceDistance - (raceFinishPoint.x - opponentInstanceTransform.position.x)) / _raceDistance, 0), 1);

            raceProgressBar.transform.localScale = new Vector3(playerProgressBarDistance, 1, 1);
            opponentProgressBar.transform.localScale = new Vector3(opponentProgressBarDistance, 1, 1);

            if (playerProgressBarDistance > opponentProgressBarDistance)
                opponentProgressBar.transform.SetAsLastSibling();
            else
                raceProgressBar.transform.SetAsLastSibling();
        }

        private int CalculateScore()
        {
            return ((int)_totalTime);
        }
    }
}
