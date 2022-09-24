using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerEffect : MonoBehaviour
{

    /// <summary>
    /// The first blade's texture onto which the effect will be applied
    /// </summary>
    public SpriteRenderer Propeller1;

    /// <summary>
    /// The second blade's texture onto which the effect will be applied
    /// </summary>
    public SpriteRenderer Propeller2;


    /// <summary>
    /// The module whose activation will trigger the effect 
    /// </summary>
    public ActuatorModule ActuatorModule;


    /// <summary>
    /// The number of ticks before the active propeller switches
    /// </summary>
    public uint TickTime = 3;



    private ulong _ticks = 0;
    private bool _state = false;
    private Color _maxAlpha = new Color(1, 1, 1, 1);
    private Color _minAlpha = new Color(1, 1, 1, 0.2f);
    private AIController _controller = null;

    // Update is called once per frame
    void Update()
    {
        if (Propeller1 == null || Propeller2 == null || ActuatorModule == null)
        {
            Debug.LogError("Propeller effect is misconfigured!");
            return;
        }

        if (_controller == null)
        {
            _controller = GetComponentInParent<AIController>();
            return;
        }

        _ticks++;
        if (_controller.Running && _controller.Vehicle.EnergyLevel > 0 && _ticks % TickTime == 0)
        {
            Propeller1.color = _state ? _minAlpha : _maxAlpha;
            Propeller2.color = _state ? _maxAlpha : _minAlpha;
            _state = !_state;
        }
    }
}
