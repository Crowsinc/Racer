using UnityEngine;

namespace Level
{
    public class Parallax : MonoBehaviour
    {
        private float _length;
        private float _startPos;
        private Vector3 _initialPos;
        private GameObject _cam;
        public float parallaxEffect;
        private SimulationController _simController;

        // Start is called before the first frame update
        private void Start()
        {
            if (Camera.main != null) _cam = Camera.main.gameObject;
            var pos = transform.position;
            _startPos = pos.x;
            _initialPos = pos;
            _length = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            _simController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (_simController.inBuildMode)
            {
                transform.position = _initialPos;
                return;
            }

            var camPos = _cam.transform.position;
            var offset = camPos.x * (1 - parallaxEffect);
            var dist = camPos.x * parallaxEffect;
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
