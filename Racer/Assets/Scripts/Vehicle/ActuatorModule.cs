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


    /// <summary>
    /// True if the actuator has already been activated for this fixed update tick.
    /// False otherwise.
    /// </summary>
    public bool Activated { get; private set; }


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
        if(Activated)
        {
            Debug.LogWarning("Actuator has already been activated this fixed update");
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

        Activated = true;

        return true;
    }


    /// <summary>
    /// Finds all acceleration effects of the actuator onto the linked vehicle at a particular reference point.
    /// The acceleration values may not be accurate unless the reference point is the centre of mass or is a
    /// valid point to take moments about (contact points etc.).
    /// </summary>
    /// <param name="referencePoint">
    /// The point of reference on the linked vehicle, from which all relative accelerations are calculated.
    /// </param>
    /// <returns></returns>
    public (float angularAcceleration, Vector2 linearAcceleration) FindKinematicEffects(Vector2 referencePoint)
    {
        if (LinkedVehicle == null)
        {
            Debug.LogError("Trying to find kinematic effects with no linked vehicle.");
            return (0, Vector2.zero);
        }

        FindPhysicsCharacteristics(
            LinkedVehicle.Rigidbody.mass,
            LinkedVehicle.Rigidbody.inertia,
            referencePoint,
            out _,
            out _,
            out float angularAcceleration,
            out _,
            out Vector2 linearAcceleration
        );

        return (angularAcceleration, linearAcceleration);
    }


    /// <summary>
    /// Find all physical effects and characteristics of an actuator
    /// </summary>
    /// <param name="mass">
    /// The mass of the structure onto which the actuator is attached.
    /// </param>
    /// <param name="rotationalInertia">
    /// The rotational inertia of the structure onto which the actuator is attached.
    /// </param>
    /// <param name="referencePoint">
    /// The point of reference, on the structure, from which all relative forces 
    /// and accelerations are calculated.
    /// </param>
    /// <param name="momentArm">
    /// The moment arm from the reference point to the actuator.
    /// </param>
    /// <param name="angularForce">
    /// The anuglar force applied by the actuator at 100%. 
    /// This is the force that is normal to the moment arm.
    /// </param>
    /// <param name="angularAcceleration">
    /// The angular acceleration applied by the actuator at 100%.
    /// </param>
    /// <param name="linearForce">
    /// The linear force applied by the actuator at 100%. 
    /// This is all left over non-angular forces.
    /// </param>
    /// <param name="linearAcceleration">
    /// The linear acceleration applied by the actuator at 100%.
    /// </param>
    private void FindPhysicsCharacteristics(
        float mass,
        float rotationalInertia,
        Vector2 referencePoint,
        out Vector2 momentArm,
        out Vector2 angularForce,
        out float angularAcceleration,
        out Vector2 linearForce,
        out Vector2 linearAcceleration
    )
    {
        momentArm = ActuationForcePosition - referencePoint;

        // Angular force is everything normal to the moment arm so 
        // project the actuation force onto the normal of the moment arm.
        Vector2 tangent = Vector2.Perpendicular(momentArm).normalized;
        var scalarAngularForce = Vector2.Dot(-ActuationForce, tangent);
        angularForce = scalarAngularForce * tangent;

        var torque = scalarAngularForce * momentArm.magnitude;
        angularAcceleration = torque / rotationalInertia;

        // Linear force is anything left over, not normal to the moment arm
        linearForce = (-ActuationForce) - angularForce;
        linearAcceleration = linearForce / mass;
    }


    void FixedUpdate()
    {
        Activated = false;
        ActuationForce = transform.TransformDirection(LocalActuationForce);
        ActuationForcePosition = transform.position + transform.TransformDirection(LocalActuationPosition);

        // Update physical effects of the actuator
        if (LinkedVehicle != null)
        {
            FindPhysicsCharacteristics(
                LinkedVehicle.Rigidbody.mass,
                LinkedVehicle.Rigidbody.inertia,
                LinkedVehicle.Rigidbody.worldCenterOfMass,
                out Vector2 momentArm,
                out Vector2 angularForce,
                out float angularAcceleration,
                out Vector2 linearForce,
                out Vector2 linearAcceleration
            );

            MomentArm = momentArm;
            AngularForce = angularForce;
            AngularAcceleration = angularAcceleration;
            LinearForce = linearForce;
            LinearAcceleration = linearAcceleration;
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
