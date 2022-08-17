using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public Transform buildModeCamPos;
    public VehicleCore playerVehicle;

    public GameObject buildModeUI;
    public GameObject buildModeGrid;
    public GameObject buildModeModuleHolder;
    public GameObject raceUI;
    public GameObject winUI;

    public Transform raceProgressBar;
    [HideInInspector]
    public GameObject opponentVehicle;
    [HideInInspector]
    public Vector3 raceFinishPoint;

    private bool inBuildMode = true;
    private CameraFollow cameraFollow;
    private float raceDistance;
    private GameObject opponentInstance;
    private VehicleConstructor _vehicleConstructor;

    private AIController _playerAI;
    private AIController _opponentAI;

    private void Awake()
    {
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
        _vehicleConstructor = GetComponent<VehicleConstructor>();
        _vehicleConstructor.vehicleCore = playerVehicle;
    }
    void Start()
    {
        EnterBuildMode();
    }

    private void Update()
    {
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

        // Freeze player vehicle
        playerVehicle.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        // Destroy opponent vehicle
        Destroy(opponentVehicle);
    }

    /// <summary>
    /// Hides the build mode UI and begins the simulation
    /// </summary>
    public void StartRace()
    {
        inBuildMode = false;
        playerVehicle.TryBuildStructure(_vehicleConstructor.GetDesign());
        cameraFollow.Target = playerVehicle.transform;

        // Change UI
        buildModeUI.SetActive(false);
        buildModeGrid.SetActive(false);
        buildModeModuleHolder.SetActive(false);
        raceUI.SetActive(true);

        // Unfreeze player vehicle
        playerVehicle.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        raceDistance = Vector3.Distance(playerVehicle.transform.position, raceFinishPoint);

        opponentInstance = Instantiate(opponentVehicle, new Vector3(3,-4,0), Quaternion.identity);

        // Build the opponent
        if (opponentInstance.TryGetComponent<TestVehicle>(out var test))
            test.Build();

        // Start the AI simulation
        // TODO: Luke feel free to change this to whatever fits your code better!
        if (opponentInstance.TryGetComponent<AIController>(out _opponentAI))
            _opponentAI.Simulate = true;
        else
            Debug.LogError("Opponent vehicle has no AI");

        if (playerVehicle.gameObject.TryGetComponent<AIController>(out _playerAI))
            _playerAI.Simulate = true;
        else
            Debug.LogError("Player vehicle has no AI");
    }

    /// <summary>
    /// Halts the simulation and displays the 'Win' UI
    /// </summary>
    public void WinRace()
    {
        raceUI.SetActive(false);
        winUI.SetActive(true);

        _playerAI.Simulate = false;
        _opponentAI.Simulate = false;
    }

    public void LoseRace()
    {
        //TODO
    }
}
