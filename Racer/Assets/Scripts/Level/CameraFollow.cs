using UnityEngine;

namespace Level
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform buildModeCamPos;
        public Transform target;
        public Transform mapStart;
        public Transform mapEnd;
        private Camera _camera;

        private void Awake()
        {
            if(!_camera) 
                _camera = Camera.main;
            target = buildModeCamPos;
            var map = GameObject.FindGameObjectWithTag("Ground");
            mapStart = map.transform.Find("Start");
            mapEnd = map.transform.Find("Flag");

            // Call update once to initialise in correct position
            Update();
        }


        public void EnterBuildMode()
        {
            target = buildModeCamPos;
        }

        // Update is called once per frame
        private void Update()
        {
            var half = (_camera.orthographicSize * _camera.aspect);

            // NOTE: old lerp system causes very annoying skipping when the vehicle moves too fast
            var position = target.position;
            var camTransform = transform;
            
            camTransform.position = new Vector3(
                position.x,
                position.y,
                -10f
            );

            var yPos = camTransform.position.y;
            if (transform.position.x - half < mapStart.position.x)
            {
                if (yPos < mapStart.position.y && target.position.x < mapStart.position.x)
                    yPos = mapStart.position.y;
                camTransform.position = new Vector3(mapStart.position.x + half, yPos, transform.position.z);
            }
            else if (target.position.x + half > mapEnd.position.x)
            {
                if (yPos < mapEnd.position.y && target.position.x > mapEnd.position.x)
                    yPos = mapEnd.position.y;
                camTransform.position = new Vector3(mapEnd.position.x - half, yPos, transform.position.z);
            }

        }
    }
}
