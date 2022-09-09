using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public Transform buildModeCamPos;
    public VehicleCore playerVehicle;

    public GameObject buildModeUI;
    public GameObject buildModeGrid;
    public GameObject buildModeModuleHolder;
    public GameObject moduleStatsDisplay;
    public GameObject moduleInfoDisplay;
    public TMP_Text timer;

    public GameObject raceUI;
    public GameObject winUI;

    public Transform raceProgressBar;
    [HideInInspector]
    public GameObject opponentVehicle;
    [HideInInspector]
    public Vector3 raceFinishPoint;

    private bool inBuildMode = true;
    private bool isFinished = false;
    private CameraFollow cameraFollow;
    private float raceDistance;
    private GameObject opponentInstance;
    private VehicleConstructor _vehicleConstructor;

    private AIController _playerAI;
    private AIController _opponentAI;

    private float _totalTime;

    private void Awake()
    {
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
        _vehicleConstructor = GetComponent<VehicleConstructor>();
        _vehicleConstructor.vehicleCore = playerVehicle;
        _totalTime = 0;
        Time.timeScale = 1;
    }
    void Start()
    {
        EnterBuildMode();
    }

    private void Update()
    {
        if (inBuildMode) return;
        if (isFinished) return;
        _totalTime += Time.deltaTime;
        timer.text = (Mathf.Round(_totalTime * 100) / 100.0).ToString();
        raceProgressBar.transform.localScale = new Vector3(Mathf.Max((raceDistance - Vector3.Distance(playerVehicle.transform.position, raceFinishPoint)) / raceDistance, 0), 1, 1);
    }

    /// <summary>
    /// Locks the camera into the build mode area and activates the build mode UI
    /// </summary>
    public void EnterBuildMode()
    {
        inBuildMode = true;
        cameraFollow.Target = buildModeCamPos;

        // Change UI
        buildModeUI.SetActive(true);
        buildModeGrid.SetActive(true);
        buildModeModuleHolder.SetActive(true);
        raceUI.SetActive(false);

        DestroyImmediate(opponentVehicle, true);
    }

    /// <summary>
    /// Hides the build mode UI and begins the simulation
    /// </summary>
    public void StartRace()
    {
        if (playerVehicle.TryBuildStructure(_vehicleConstructor.GetDesign()))
        {
            inBuildMode = false;
            cameraFollow.Target = playerVehicle.transform;


            // Change UI
            buildModeUI.SetActive(false);
            buildModeGrid.SetActive(false);
            buildModeModuleHolder.SetActive(false);
            raceUI.SetActive(true);

            raceDistance = Vector3.Distance(playerVehicle.transform.position, raceFinishPoint);
            
            // Build the opponent
            opponentInstance = Instantiate(opponentVehicle, new Vector3(0, 3, 0), Quaternion.identity);
            
            // Unfreeze player vehicle
            playerVehicle.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

            // Start the AI simulation
            _opponentAI = opponentInstance.GetComponentInChildren<AIController>();
            if (_opponentAI != null)
                _opponentAI.Simulate = true;
            else
                Debug.LogError("Opponent vehicle has no AI");

            if (playerVehicle.gameObject.TryGetComponent<AIController>(out _playerAI))
                _playerAI.Simulate = true;
            else
                Debug.LogError("Player vehicle has no AI");
        }
        else Debug.LogWarning("Player Vehicle Failed Validation");
    }

    /// <summary>
    /// Halts the simulation and displays the 'Win' UI
    /// </summary>
    public void WinRace()
    {
        foreach (var level in GetComponent<LevelInitialiser>().levelCollection)
        {
            if (PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL) == level.levelId)
            {
                level.SetHighScore(Mathf.RoundToInt(CalculateScore()));
            }
        }
        raceUI.SetActive(false);
        winUI.SetActive(true);

        _playerAI.Simulate = false;
        _opponentAI.Simulate = false;
        isFinished = true;
    }

    public void LoseRace()
    {
        isFinished = true;
        //TODO
    }

    private int CalculateScore()
    {
        return ((int)_totalTime);
    }
}
