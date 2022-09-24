using System;
using UnityEngine;
using UnityEngine.UI;

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
                // Progress image position is taken from its centre, so the difference between the 
                // progress bar and image position is the size of half the bar. Adding two halves
                // to the progress bar position then gives us the current end position of the bar. 
                progressBar.position.x + 2 * (progressImage.position.x - progressBar.position.x),
                _yPosition,
                _zPosition
            );
        }
    }
}
