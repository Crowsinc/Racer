using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public Transform buildModeCamPos;
    public VehicleCore playerVehicle;

    public GameObject buildModeUI;
    public GameObject raceUI;

    private bool inBuildMode = true;
    private CameraFollow cameraFollow;

    private void Awake()
    {
        cameraFollow = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();
    }
    void Start()
    {
        EnterBuildMode();
    }

    public void EnterBuildMode()
    {
        inBuildMode = true;
        cameraFollow.Target = buildModeCamPos;

        buildModeUI.SetActive(true);
        raceUI.SetActive(false);
    }

    public void StartRace()
    {
        inBuildMode = false;
        cameraFollow.Target = playerVehicle.transform;

        buildModeUI.SetActive(false);
        raceUI.SetActive(true);
    }
}
