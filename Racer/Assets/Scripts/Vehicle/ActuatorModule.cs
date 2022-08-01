using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActuatorModule : MonoBehaviour
{

    /// <summary>
    /// The force the actuator applies, local to the module
    /// </summary>
    public Vector2 LocalActuationForce = Vector2.zero;


    /// <summary>
    /// The position of the actuation force, local to the centre of the module
    /// </summary>
    public Vector2 LocalActuationPosition = Vector2.zero;


    /// <summary>
    /// The force the actuator applies on the world
    /// </summary>
    public Vector2 ActuationForce { get; private set; }


    /// <summary>
    /// The position of the actuation force in the world 
    /// </summary>
    public Vector2 ActuationForcePosition { get; private set; }


    /// <summary>
    /// The vehicle that this actuator is linked/attached to
    /// </summary>
    public VehicleCore LinkedVehicle = null;


    /// <summary>
    /// The moment arm from the linked vehicles centre of mass to the actuators force position
    /// </summary>
    public Vector2 MomentArm { get; private set; }


    /// <summary>
    /// The angular force generated on the linked vehicle by this actuator.
    /// Mathematically, it is the force that is applied tangentially to the
    /// centre of mass and ultimately generates a torque force. It is the F
    /// in 'Torque = F x r', where r is the moment arm vector.
    /// </summary>
    public Vector2 AngularForce { get; private set; }


    /// <summary>
    /// The angular acceleration generated on the linked vehicle by this actuator,
    /// as a result of the applied AngularForce. Counter-clockwise is positive. 
    /// </summary>
    public float AngularAcceleration { get; private set; }


    /// <summary>   
    /// The linear force generated on the linked vehicle by this actuator
    /// </summary>
    public Vector2 LinearForce { get; private set; }


    /// <summary>
    /// The linear acceleration generated on the linked vehicle by this actuator,
    /// as a result of the applied LinearForce.
    /// </summary>
    public Vector2 LinearAcceleration { get; private set; }


    /// <summary>
    /// Amount of energy consumed by the actuator (at 100%) per second
    /// </summary>
    public float EnergyPerSecond = 1.0f;


    private bool _activated = false;


    /// <summary>
    /// Attempts to generate a force on the linked vehicle by activating the actuator. 
    /// The linked vehicle must supply the required energy from its energy stores.
    /// An actuator may only be updated once per FixedUpdate (physics tick).
    /// </summary>
    /// <param name="proportion"> 
    /// The proportion [0.0,1.0] of the actuation force which will be applied. 
    /// The energy consumption is adjusted proportionally as well. 
    /// </param>
    /// <param name="forced">
    /// If set to true, the actuator will always run regardless as along 
    /// as there is some energy left in the linked vehicle.
    /// </param>
    /// <returns>
    /// True if the actuator was successfuly activated, false if the actuator failed to run.
    /// </returns>
    public bool TryActivate(float proportion = 1.0f, bool forced = false)
    {
        if(_activated)
        {
            Debug.LogWarning("Actuator has already been activated this update");
            return false;
        }

        if(LinkedVehicle == null)
        {
            Debug.LogError("Actuator was activated with no linked vehicle");
            return false;
        }

        proportion = Mathf.Clamp01(proportion);
        var requiredEnergy = proportion * EnergyPerSecond * Time.deltaTime;

        // Fail to activate the actuator if we don't have enough energy. If force is set to true,
        // then always activate the actuator as long as the vehicle has any energy left. This can
        // help simplify the AI around the lower boundary condition
        if((!forced && LinkedVehicle.EnergyLevel < requiredEnergy) || LinkedVehicle.EnergyLevel <= 0.0f)
        {
            Debug.LogWarning("Actuator was activated with insufficient energy");
            return false;
        }

        LinkedVehicle.EnergyLevel = Mathf.Max(0, LinkedVehicle.EnergyLevel - requiredEnergy);

        // NOTE: we apply the opposite force onto the vehicle (Newton's Third Law)
        LinkedVehicle.Rigidbody.AddForceAtPosition(-ActuationForce, ActuationForcePosition); 

        _activated = true;

        return true;
    }


    void FixedUpdate()
    {
        _activated = false;
        ActuationForce = transform.TransformDirection(LocalActuationForce);
        ActuationForcePosition = transform.position + transform.TransformDirection(LocalActuationPosition);

        // Update 
        if (LinkedVehicle != null)
        {
            MomentArm = ActuationForcePosition - LinkedVehicle.Rigidbody.worldCenterOfMass;
            
            // Angular force is everything normal to the moment arm so 
            // project the actuation force onto the normal of the moment arm.
            Vector2 tangent = Vector2.Perpendicular(MomentArm).normalized;
            var scalarAngularForce = Vector2.Dot(-ActuationForce, tangent);
            AngularForce = scalarAngularForce * tangent;
            
            var torque = scalarAngularForce * MomentArm.magnitude;
            AngularAcceleration = torque / LinkedVehicle.Rigidbody.inertia;

            // Linear force is anything left over, not normal to the moment arm
            LinearForce = (-ActuationForce) - AngularForce;
            LinearAcceleration = LinearForce / LinkedVehicle.Rigidbody.mass;
            
        }
        else
        {
            MomentArm = Vector2.zero;
            AngularForce = Vector2.zero;
            LinearForce = Vector2.zero;
            AngularAcceleration = 0.0f;
            LinearAcceleration = Vector2.zero;
        }
    }


    void OnValidate()
    {
        EnergyPerSecond = Mathf.Max(EnergyPerSecond, 0);
    }


}
