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

    private float _alpha = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (Texture != null && ActuatorModule != null)
        {
            _alpha = Mathf.Lerp(_alpha, ActuatorModule.Proportion, 0.01f);

            var c = Texture.color;
            c.a = _alpha;
            Texture.color = c;
        }
        else Debug.LogWarning("Actuation effect has no attached texture or actuator module");
    }
}
