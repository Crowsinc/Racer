using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyTank : MonoBehaviour
{
    /// <summary>
    /// Transform of the energy sprite to be shrunk
    /// </summary>
    public Transform tankBlock;

    /// <summary>
    /// Initial vehicle energy capacity
    /// </summary>
    private float maxFuel;

    /// <summary>
    /// The initial y value of the local scale of the energy tank sprite
    /// </summary>
    private float initalYScale;

    /// <summary>
    /// The vehicle that the energy tank belongs to
    /// </summary>
    private VehicleCore vehicle;

    void Awake()
    {
        // Getting initialYScale
        
        initalYScale = transform.localScale.y;
    }

    void Update()
    {
        if (vehicle != null)
        {
            // Getting max fuel
            maxFuel = vehicle.EnergyCapacity;

            // Changing y component of the scale to lower the energy tank
            tankBlock.localScale = new Vector3(tankBlock.localScale.x, initalYScale * (vehicle.EnergyLevel / maxFuel), tankBlock.localScale.z);
        }

        // Trying to get vehicle core
        else
        {
            vehicle = GetComponent<VehicleModule>().LinkedVehicle;
        }
    }
}
