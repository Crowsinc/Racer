using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public Transform buildModeCamPos;
    public VehicleCore playerVehicle;

    public GameObject buildModeUI;
    public GameObject buildModeGrid;
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

    private void Awake()
    {
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
    }
    void Start()
    {
        EnterBuildMode();
    }

    private void Update()
    {
        raceProgressBar.transform.localScale = new Vector3(Mathf.Max((raceDistance - Vector3.Distance(playerVehicle.transform.position, raceFinishPoint)) / raceDistance,0), 1, 1);
    }

    /// <summary>
    /// Locks the camera into the build mode area and activates the build mode UI
    /// </summary>
    public void EnterBuildMode()
    {
        inBuildMode = true;
        cameraFollow.Target = buildModeCamPos;

        buildModeUI.SetActive(true);
        buildModeGrid.SetActive(true);
        raceUI.SetActive(false);

        Destroy(opponentVehicle);
    }

    /// <summary>
    /// Hides the build mode UI and begins the simulation
    /// </summary>
    public void StartRace()
    {
        inBuildMode = false;
        cameraFollow.Target = playerVehicle.transform;

        buildModeUI.SetActive(false);
        buildModeGrid.SetActive(true);
        raceUI.SetActive(true);

        raceDistance = Vector3.Distance(playerVehicle.transform.position, raceFinishPoint);

        opponentInstance = Instantiate(opponentVehicle, buildModeCamPos.position, Quaternion.identity);
    }

    /// <summary>
    /// Halts the simulation and displays the 'Win' UI
    /// </summary>
    public void WinRace()
    {
        raceUI.SetActive(false);
        winUI.SetActive(true);
    }

    public void LoseRace()
    {
        //TODO
    }
}
