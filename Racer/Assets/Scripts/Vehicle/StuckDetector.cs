using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;

public class StuckDetector : MonoBehaviour
{
    private List<float> movementChanges = new List<float>();
    private SimulationController _simulationController;
    private Vector3 prevPosition;
    private GameObject resetPrompt;

    void Awake()
    {
        _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        resetPrompt = _simulationController.resetPrompt;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_simulationController.inBuildMode)
            return;
        movementChanges.Add(Vector3.Distance(prevPosition, transform.position));
        if (movementChanges.Count > 150)
        {
            movementChanges.RemoveAt(0);
            float sum = 0;
            foreach (float movementChange in movementChanges)
            {
                sum += movementChange;
            }
            //Debug.Log("sum:" + sum.ToString());
            if (sum < 2 && !resetPrompt.activeSelf)
            {
                OpenPrompt();
            }
        }
        prevPosition = transform.position;
    }

    private void OpenPrompt()
    {
        resetPrompt.SetActive(true);
        resetPrompt.transform.position = Camera.main.WorldToScreenPoint(_simulationController.playerVehicle.transform.position + 0.5f * Vector3.right) + Vector3.up * 100f;
    }
}
