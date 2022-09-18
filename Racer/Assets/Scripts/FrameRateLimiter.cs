using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{

    /// <summary>
    /// The frame rate to set game to target
    /// </summary>
    public int FrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = FrameRate;
    }


}
