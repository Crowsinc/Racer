using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActuatorModule : MonoBehaviour
{
    /// <summary>
    /// The force the actuator applies to the world when activated at 100% proportion
    /// </summary>
    public Vector2 LocalActuationForce = Vector2.zero;


    /// <summary>
    /// The force the actuator applies to the world while NOT activated
    /// </summary>
    public Vector2 LocalIdleForce = Vector2.zero;


    /// <summary>
    /// The origin position of the actuation force, relative to the centre of the module
    /// </summary>
    public Vector2 LocalActuationPosition = Vector2.zero;


    /// <summary>
    /// The energy drain per second while the actuator is activated at 100% proportion.
    /// </summary>
    public float ActivationCost = 1.0f;


    /// <summary>
    ///  The energy drain per second while the actuator is NOT actuated. 
    /// </summary>
    public float IdleCost = 0.0f;


    /// <summary>
    /// If true, allows the actuator to be activated with a proportion from [0,1]; 
    /// otherwise, the actuator can only be completely on or off. 
    /// </summary>
    public bool ProportionalControl = true;
         

    /// <summary>
    /// If true, allows AI to control the actuator. 
    /// Changes do not take effect while the AI is already running.
    /// </summary>
    public bool AIControllable = true;


    /// <summary>
    /// Set to true to disable the use of the actuator.
    /// </summary>
    public bool Disabled = false;


    /// <summary>
    /// The force the actuator applies on the world when activated at 100%
    /// </summary>
    public Vector2 ActuationForce { get; private set; }


    /// <summary>
    /// The force the actuator applies on the vehicle when activated at 100%
    /// </summary>
    public Vector2 ReactionForce { get; private set; }


    /// <summary>
    /// The force the actuator applies on the world when not activated
    /// </summary>
    public Vector2 IdleForce { get; private set; }


    /// <summary>
    /// The force the actuator applies on the vehicle when not activated
    /// </summary>
    public Vector2 IdleReactionForce { get; private set; }


    /// <summary>
    /// The origin position of the actuation force in the world 
    /// </summary>
    public Vector2 ActuationPosition { get; private set; }


    /// <summary>
    /// The vehicle that this actuator is linked/attached to
    /// </summary>
    [HideInInspector]
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
    /// True if the actuator has been activated, false otherwise.
    /// </summary>
    public bool Activated { get; private set; }
    private bool _locked = false;

    /// <summary>
    /// The proportion [0,1] that the actuator is activated with
    /// </summary>
    public float Proportion { get; private set; }
    private float _proportion = 0.0f;

    /// <summary>
    /// Attempts to generate a force on the linked vehicle by activating the actuator. 
    /// The linked vehicle must supply the required energy from its energy stores.
    /// An actuator may only be updated once per FixedUpdate (physics tick).
    /// </summary>
    /// <param name="proportion"> 
    /// The proportion [0.0,1.0] of the actuation force which will be applied. 
    /// The energy consumption is adjusted proportionally as well. 
    /// If proportional control is set to false for this actuator, 
    /// then a proportion of 1.0f will always be applied.
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
        if (Disabled)
            return false;

        if(_locked)
        {
            Debug.LogWarning("Actuator has already been activated this fixed update");
            return false;
        }

        if(LinkedVehicle == null)
        {
            Debug.LogError("Actuator was activated with no linked vehicle");
            return false;
        }

        if(ProportionalControl)
            proportion = Mathf.Clamp01(proportion);
        else
            proportion = 1.0f;

        var requiredEnergy = proportion * ActivationCost * Time.fixedDeltaTime;

        // Fail to activate the actuator if we don't have enough energy. If force is set to true,
        // then always activate the actuator as long as the vehicle has any energy left. This can
        // help simplify the AI around the lower boundary condition
        if((!forced && LinkedVehicle.EnergyLevel < requiredEnergy) || LinkedVehicle.EnergyLevel <= 0.0f)
        {
            Debug.LogWarning("Actuator was activated with insufficient energy");
            return false;
        }

        LinkedVehicle.EnergyLevel = Mathf.Max(0, LinkedVehicle.EnergyLevel - requiredEnergy);
        LinkedVehicle.Rigidbody.AddForceAtPosition(ReactionForce * proportion, ActuationPosition);

        _proportion = proportion;
        _locked = true;
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
        momentArm = ActuationPosition - referencePoint;

        // Angular force is everything normal to the moment arm so 
        // project the actuation force onto the normal of the moment arm.
        Vector2 tangent = Vector2.Perpendicular(momentArm).normalized;
        var scalarAngularForce = Vector2.Dot(ReactionForce, tangent);
        angularForce = scalarAngularForce * tangent;

        var torque = scalarAngularForce * momentArm.magnitude;
        angularAcceleration = torque / rotationalInertia;

        // Linear force is anything left over, not normal to the moment arm
        linearForce = ReactionForce - angularForce;
        linearAcceleration = linearForce / mass;
    }

    void FixedUpdate()
    {
        Activated = _locked;
        _locked = false;

        Proportion = _proportion;
        _proportion = 0.0f;

        ActuationPosition = transform.TransformPoint(LocalActuationPosition);
        
        ActuationForce = transform.TransformDirection(LocalActuationForce);
        ReactionForce = -ActuationForce;
        
        IdleForce = transform.TransformDirection(LocalIdleForce);
        IdleReactionForce = -IdleForce;

        // Update physical effects of the actuator
        if (LinkedVehicle != null)
        {
            // NOTE: we apply the reaction force onto the vehicle (Newton's Third Law)
            if(!Activated)
            {
                LinkedVehicle.Rigidbody.AddForceAtPosition(IdleReactionForce, ActuationPosition);
                LinkedVehicle.EnergyLevel -= IdleCost * Time.fixedDeltaTime;
            }

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
        ActivationCost = Mathf.Max(ActivationCost, 0);
        IdleCost = Mathf.Max(IdleCost, 0);
    }

}
