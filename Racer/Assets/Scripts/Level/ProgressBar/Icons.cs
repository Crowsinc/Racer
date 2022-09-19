using System;
using UnityEngine;

namespace Level.ProgressBar
{
    public class Icons : MonoBehaviour
    {
        public RectTransform progressBar;
        public RectTransform progressImage;

        private float _yPosition;
        private float _zPosition;
        private Transform _transform;

        private void Start()
        {
            _transform = transform;
            var position = _transform.position;
            _yPosition = position.y;
            _zPosition = position.z;
        }

        private void Update()
        {
            _transform.position = new Vector3(
                progressBar.position.x + progressBar.localScale.x * progressImage.sizeDelta.x * 1.295f,
                _yPosition, _zPosition);
        }
    }
}
