using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public class Parallax : MonoBehaviour
    {
        private float _length;
        private float _startPos;
        private Vector3 _initialPos;
        private GameObject cam;
        public float parallaxEffect;
        private SimulationController _simController;

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main.gameObject;
            _startPos = transform.position.x;
            _initialPos = transform.position;
            _length = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            _simController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_simController.inBuildMode)
            {
                transform.position = _initialPos;
                return;
            }
            float offset = cam.transform.position.x * (1 - parallaxEffect);
            float dist = cam.transform.position.x * parallaxEffect;
            transform.position = new Vector3(_startPos + dist, transform.position.y, transform.position.z);

            if (offset > _startPos + _length)
            {
                _startPos += _length * 2;
            }
            else if (offset < _startPos - _length)
            {
                _startPos -= _length * 2;
            }
        }
    }
}
