using UnityEngine;

namespace Level
{
    public class SwitchActivate : MonoBehaviour
    {
        public GameObject item;

        [Header("Rotation")]
        public float startRotation;
        public float finalRotation;

        [Header("Position")]
        public Vector2 startPosition;
        public Vector2 finalPosition;

        public float timeToDestination;

        private float _f;
        private Vector3 _startRot;
        private Vector3 _finalRot;

        private bool _isActivated;
        private bool _stop;


        // Start is called before the first frame update

        private void Awake()
        {
            GameObject scripts = GameObject.FindWithTag("GameController");
            if (!scripts)
            {
                _isActivated = true;
                return;
            }
            scripts.GetComponent<SimulationController>().LevelRestart += Restart;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!_isActivated) return;
            if (_stop) return;
            _f += Time.deltaTime / timeToDestination;
            item.transform.position = Vector2.Lerp(startPosition, finalPosition, _f);
            item.transform.eulerAngles = Vector3.Lerp(_startRot, _finalRot, _f);
            if (_f >= 1)
                _stop = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isActivated) return;
            _isActivated = true;
            var switchTransform = transform;
            var switchScale = switchTransform.localScale;
            switchTransform.localScale = new Vector3(switchScale.x * -1, switchScale.y, switchScale.z);
            _f = 0;
        }

        private void Restart()
        {
            if (_isActivated)
            {
                var switchTransform = transform;
                var switchScale = switchTransform.localScale;
                switchTransform.localScale = new Vector3(switchScale.x * -1, switchScale.y, switchScale.z);
            }

            _isActivated = _stop = false;
            item.transform.position = startPosition;
            item.transform.eulerAngles = _startRot = new Vector3(0, 0, startRotation);

            _finalRot = new Vector3(0, 0, finalRotation);
        }
    
    }
}


