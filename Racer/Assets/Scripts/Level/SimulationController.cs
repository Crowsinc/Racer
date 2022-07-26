using System.Collections.Generic;
using System.Linq;
using Build_Mode;
using TMPro;
using UnityEditor;
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
        public GameObject countdownUI;
        public GameObject buildModeBackground;

        [Header("Progress Bar")]
        public Transform playerProgressBar;
        public Transform opponentProgressBar;
        
        [HideInInspector]
        public GameObject opponentVehicle;
        [HideInInspector]
        public Vector3 raceFinishPoint;
        [HideInInspector]
        public bool validDesign;

        [Header("Misc")] 
        public Transform fuelBar;
        
        public bool inBuildMode = true;
        public GameObject opponentInstance;
        public Transform opponentInstanceTransform;
        public LevelCompleteScreen levelCompleteScreen;
        
        private Level _level;
        private bool _isFinished;
        public bool countdownFinish;
        private CameraFollow _cameraFollow;
        private float _raceDistance;
        
        private VehicleConstructor _vehicleConstructor;
        private AIController _playerAI;
        private AIController _opponentAI;

        private Timer time;
        private float count;

        public PolygonCollider2D mapCollider;

        public delegate void SimulationDelegates();
        public SimulationDelegates InBuildMode;
        public SimulationDelegates RaceStart;
        public SimulationDelegates RaceCountdown;
        public SimulationDelegates RaceWin;
        public SimulationDelegates RaceLoose;
        public SimulationDelegates RaceFinish;

        private void Awake()
        {
            _cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
            _vehicleConstructor = GetComponent<VehicleConstructor>();
            _vehicleConstructor.vehicleCore = playerVehicle;
            
            time = new Timer();

            Time.timeScale = 1;
        }

        private void Start()
        {
            var lvl = gameObject.GetComponent<LevelInitialiser>();
            _level = lvl.selectedLevel;
            mapCollider = lvl.currentLevel.GetComponent<PolygonCollider2D>();
            EnterBuildMode();
        }

        private void Update()
        {
            if (inBuildMode) return;
            if (_isFinished) return;
            if (!countdownFinish)
            {
                count += Time.deltaTime;
                if (count > 4)
                    return;
                countdownUI.transform.GetChild(Mathf.FloorToInt(count)).gameObject.SetActive(true);
                return;
            }
            time.Tick(Time.deltaTime);
            timer.text = Timer.TimeToString(time.GetTime());
            UpdateFuelBar();
        }

        /// <summary>
        /// Locks the camera into the build mode area and activates the build mode UI
        /// </summary>
        public void EnterBuildMode()
        {
            inBuildMode = true;
            time.Reset();
            _isFinished = false;
            countdownFinish = false;
            InBuildMode?.Invoke();
            mapCollider.enabled = false;
            count = 0;

            // Change UI
            buildModeUI.SetActive(true);
            buildModeGrid.SetActive(true);
            buildModeModuleHolder.SetActive(true);
            raceUI.SetActive(false);
            pauseUI.SetActive(false);
            resetPrompt.SetActive(false);
            countdownUI.SetActive(false);
            buildModeBackground.SetActive(true);

            // Update feedback UI
            _vehicleConstructor.ShowUIElements();
            _vehicleConstructor.ValidateDesign(); 

            // Reset camera position
            _cameraFollow.EnterBuildMode();

            // Resetting player vehicle
            if (playerVehicle.IsBuilt)
            {
                var playerVehicleTransform = playerVehicle.transform;
                playerVehicleTransform.position = new Vector3(0, 3, 0);
                playerVehicleTransform.rotation = Quaternion.identity;

                Rigidbody2D rb = playerVehicle.GetComponent<Rigidbody2D>();
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
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
            // var validDesign = _level.budget >= _vehicleConstructor.SumVehicleCost();
            //validDesign &= _vehicleConstructor.ValidateRestrictions(_level.restrictions);
            validDesign &= playerVehicle.TryBuildStructure(_vehicleConstructor.GetDesign());

            if (validDesign)
            {
                inBuildMode = false;
                _cameraFollow.target = playerVehicle.transform;
                mapCollider.enabled = true;
                RaceCountdown?.Invoke();

                // Change UI
                _vehicleConstructor.HideUIElements();
                buildModeUI.SetActive(false);
                buildModeGrid.SetActive(false);
                buildModeModuleHolder.SetActive(false);
                countdownUI.SetActive(true);
                buildModeBackground.SetActive(false);

                // Reset countdown UI
                for (var i = 0; i < countdownUI.transform.childCount; i++)
                {
                    countdownUI.transform.GetChild(i).gameObject.SetActive(false);
                }

                _raceDistance = Vector3.Distance(playerVehicle.transform.position, raceFinishPoint);

                // Build the opponent, ensuring its on the opponent vehicle layer
                opponentInstance = Instantiate(opponentVehicle, new Vector3(0, 3, 0), Quaternion.identity);
                opponentInstance.layer = LayerMask.NameToLayer("Opponent Vehicle");
                opponentInstanceTransform = opponentInstance.transform.Find("Vehicle");
            
                // Freeze player and opponent x coord, so they can't move until countdown is done
                playerVehicle.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
                opponentInstanceTransform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;

                Invoke(nameof(Unfreeze), 4.0f);

                
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
                level.SetNewTime(CalculateScore());
                level.SetNewCost((int)_vehicleConstructor.SumVehicleCost());
            }
            raceUI.SetActive(false);
            winUI.SetActive(true);
            levelCompleteScreen.initLevelCompleteScreen(true, CalculateScore(), (int)_vehicleConstructor.SumVehicleCost());

            _playerAI.StopSimulating();
            _opponentAI.StopSimulating();
            _isFinished = true;
            RaceWin?.Invoke();
            RaceFinish?.Invoke();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void LoseRace()
        {
            _isFinished = true;
            raceUI.SetActive(false);
            winUI.SetActive(true);
            levelCompleteScreen.initLevelCompleteScreen(false, CalculateScore(), (int)_vehicleConstructor.SumVehicleCost());
            RaceLoose?.Invoke();
            RaceFinish?.Invoke();
        }

        private void UpdateFuelBar()
        {
            var percentage = playerVehicle.EnergyLevel / playerVehicle.EnergyCapacity;
            fuelBar.transform.localScale = new Vector3(1, percentage, 1);

            // Debug.Log("Energy: " + playerVehicle.EnergyLevel + ", " + percentage.ToString() + "%");
        }

        private float CalculateScore()
        {
            //return  (int)(playerVehicle.EnergyLevel / (_vehicleConstructor.SumVehicleCost() * _totalTime) * 100000);
            return time.GetTime();
        }

        private void Unfreeze()
        {
            countdownFinish = true;

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

            // Unfreeze vehicles
            _playerAI.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            _opponentAI.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

            countdownUI.SetActive(false);
            raceUI.SetActive(true);
            RaceStart?.Invoke();
        }
    }
}
