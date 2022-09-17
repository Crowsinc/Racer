using System;
using UnityEngine;

namespace Vehicle.Modules
{
    public class SpringActivation : MonoBehaviour
    {
        public ActuatorModule actuator;
        public float upForce = 500;
        
        private float _mass;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_mass == 0f)
            {   
                Debug.Log("test");
                _mass = GetComponentInParent<Rigidbody2D>().mass;
                actuator.LocalActuationForce = Vector2.down * _mass * upForce;
            }

            if (!collision.CompareTag("Ground")) return;

            actuator.TryActivate();
        }
    }
}
