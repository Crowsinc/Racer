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

    // Update is called once per frame
    void Update()
    {
        if (Texture != null && ActuatorModule != null)
        {
            Texture.enabled = ActuatorModule.Activated;
        }
        else Debug.LogWarning("Actuation effect has no attached texture or actuator module");
    }
}
