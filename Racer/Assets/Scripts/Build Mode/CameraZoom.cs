using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera _camera;
    private bool _unZoom;
    public float unZoomSize = 10f;
    public float zoomSize = 5f;
    public float unZoomTime = 2f;
    private float _f;

    public void Start()
    {
        _unZoom = false;
        _camera = Camera.main;
    }

    public void UnZoom()
    {
        _unZoom = true;
    }

    public void Update()
    {
        if (!_unZoom) return;

        if (_camera.orthographicSize >= unZoomSize)
        {
            _unZoom = false;
            _camera.orthographicSize = 10f;
        }
        
        _f += Time.deltaTime / unZoomTime;
        _camera.orthographicSize = Mathf.Lerp(zoomSize, unZoomSize, _f);
    }
}
