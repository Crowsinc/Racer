using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActuationEffect : MonoBehaviour
{
    /// <summary>
    /// The texture onto which the effect will be applied
    /// </summary>
    public SpriteRenderer Texture;

    /// <summary>
    /// The module whose activation will trigger the effect 
    /// </summary>
    public ActuatorModule ActuatorModule;

    /// <summary>
    /// The alpha lerp factor
    /// </summary>
    public float LerpFactor = 0.1f;

    private float _alpha = 0.0f;
    private AIController _controller = null;


    // Update is called once per frame
    void Update()
    {
        if (Texture == null || ActuatorModule == null)
        {
            Debug.LogError("Actuation effect is misconfigured!");
            return;
        }

        if (_controller == null)
        {
            _controller = GetComponentInParent<AIController>();
            return;
        }

        if(_controller.Running)
        {
            _alpha = Mathf.Lerp(_alpha, ActuatorModule.Proportion, LerpFactor);

            Texture.enabled = true;
            var c = Texture.color;
            c.a = _alpha;
            Texture.color = c;
        }
    }

    private void OnValidate()
    {
        LerpFactor = Mathf.Clamp01(LerpFactor);
    }
}
