using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailParticles : MonoBehaviour
{
    ParticleSystem _particleSystem;
    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (_particleSystem.isStopped)
        {
            Destroy(gameObject);
        }
    }
}
