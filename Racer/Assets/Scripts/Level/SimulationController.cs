using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
    public TMP_Text timer;

    public GameObject raceUI;
    public GameObject pauseUI;
    public GameObject resetPrompt;
    public GameObject winUI;

    public Transform raceProgressBar;
    [HideInInspector]
    public GameObject opponentVehicle;
    [HideInInspector]
    public Vector3 raceFinishPoint;

    public bool inBuildMode = true;
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
        Time.timeScale = 0;
        inBuildMode = true;
        cameraFollow.Target = buildModeCamPos;

        // Change UI
        buildModeUI.SetActive(true);
        buildModeGrid.SetActive(true);
        buildModeModuleHolder.SetActive(true);
        raceUI.SetActive(false);
        pauseUI.SetActive(false);
        resetPrompt.SetActive(false);

        // Resetting player vehicle
        if(playerVehicle.IsBuilt)
        {
            playerVehicle.transform.position = buildModeCamPos.position - Vector3.back * 10;
            playerVehicle.transform.rotation = Quaternion.identity;

            Rigidbody2D rb = playerVehicle.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;

            playerVehicle.ClearStructure();
            _playerAI?.Stop();
        }
        DestroyImmediate(opponentInstance, true);
    }

    /// <summary>
    /// Hides the build mode UI and begins the simulation
    /// </summary>
    public void StartRace()
    {
        if (playerVehicle.TryBuildStructure(_vehicleConstructor.GetDesign()))
        {
            Time.timeScale = 1;
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
                _opponentAI.Start();
            else
                Debug.LogError("Opponent vehicle has no AI");

            if (playerVehicle.gameObject.TryGetComponent<AIController>(out _playerAI))
                _playerAI.Start();
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
        raceUI.SetActive(false);
        winUI.SetActive(true);

        _playerAI.Stop();
        _opponentAI.Stop();
        isFinished = true;
    }

    public void LoseRace()
    {
        isFinished = true;
        //TODO
    }
}
