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

    // Update is called once per frame
    void Update()
    {
        if (Texture != null && ActuatorModule != null)
        {
            _alpha = Mathf.Lerp(_alpha, ActuatorModule.Proportion, LerpFactor);

            Texture.enabled = true;
            var c = Texture.color;
            c.a = _alpha;
            Texture.color = c;
        }
        else Debug.LogWarning("Actuation effect has no attached texture or actuator module");
    }

    private void OnValidate()
    {
        LerpFactor = Mathf.Clamp01(LerpFactor);
    }
}
