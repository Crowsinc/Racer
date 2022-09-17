using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Level
{
    [ExecuteInEditMode]
    public class NodeAttach : MonoBehaviour
    {
        public SpriteShapeController spriteShapeController;
        public int index;
        public bool useNormals;
        public bool runtimeUpdate;
        [Header("Offset")]
        public float yOffset;
        public bool localOffset;
        private Spline _spline;
        private int _lastSpritePointCount;
        private bool _lastUseNormals;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _spline = spriteShapeController.spline;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!EditorApplication.isPlaying || runtimeUpdate)
            {
                _spline = spriteShapeController.spline;
                if ((_spline.GetPointCount() != 0) && (_lastSpritePointCount != 0))
                {
                    index = Mathf.Clamp(index, 0, _spline.GetPointCount() - 1);
                    if (_spline.GetPointCount() != _lastSpritePointCount)
                    {
                        if (_spline.GetPosition(index) != _lastPosition)
                        {
                            index += _spline.GetPointCount() - _lastSpritePointCount;
                        }
                    }
                    if ((index <= _spline.GetPointCount() - 1) && (index >= 0))
                    {
                        if (useNormals)
                        {
                            if (_spline.GetTangentMode(index) != ShapeTangentMode.Linear)
                            {
                                Vector3 lt = Vector3.Normalize(_spline.GetLeftTangent(index) - _spline.GetRightTangent(index));
                                Vector3 rt = Vector3.Normalize(_spline.GetLeftTangent(index) - _spline.GetRightTangent(index));
                                float a = Angle(Vector3.left, lt);
                                float b = Angle(lt, rt);
                                float c = a + (b * 0.5f);
                                if (b > 0)
                                    c = (180 + c);
                                transform.rotation = Quaternion.Euler(0, 0, c);
                            }
                        }
                        else
                        {
                            transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        Vector3 offsetVector;
                        if (localOffset)
                        {
                            offsetVector = (Vector3)Rotate(Vector2.up, transform.localEulerAngles.z) * yOffset;
                        }
                        else
                        {
                            offsetVector = Vector2.up * yOffset;
                        }
                        transform.position = spriteShapeController.transform.position + _spline.GetPosition(index) + offsetVector;
                        _lastPosition = _spline.GetPosition(index);
                    }
                }
            }
            _lastSpritePointCount = _spline.GetPointCount();
        }

        private float Angle(Vector3 a, Vector3 b)
        {
            float dot = Vector3.Dot(a, b);
            float det = (a.x * b.y) - (b.x * a.y);
            return Mathf.Atan2(det, dot) * Mathf.Rad2Deg;
        }

        private Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            float tx = v.x;
            float ty = v.y;
            return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
        }
    
#endif
    }
}