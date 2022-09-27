using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Level
{
    public class LevelTerrain : MonoBehaviour
    {
        [Header("TerrainPoints")]
        public SpriteShapeController spriteShapeController;
        public GameObject startPoint;
        public GameObject endPoint;

        [Header("TerrainPhysics")]
        public PhysicsMaterial2D material;

        private int _startIndex;
        private int _endIndex;

        private Dictionary<string, Rigidbody2D> _rbs;

        public void Awake()
        {
            _startIndex = startPoint.GetComponent<NodeAttach>().index;
            _endIndex = endPoint.GetComponent<NodeAttach>().index;

            _rbs = new Dictionary<string, Rigidbody2D>();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            var parent = collision.transform.root.Find("Vehicle");
            // Return if collided object is not a vehicle
            if (parent == null || parent.gameObject.layer != 30) return;

            var vehicleName = parent.root.GetInstanceID().ToString();
            if (!_rbs.ContainsKey(vehicleName))
            {
                _rbs[vehicleName] = parent.GetComponent<Rigidbody2D>();
            }

            switch (CheckWhichTerrain(parent.position))
            {
                case 0:
                    // What happens if vehicle is touching Grass
                    break;
                case 1:
                    // What happens if vehicle is touching Mud
                    break;
                case 2:
                    _rbs[vehicleName].AddForce(new Vector2(0, 10000));
                    break;
                case 3:
                    _rbs[vehicleName].AddForce(new Vector2(_rbs[vehicleName].velocity.x * 100, 0));
                    break;
            }

        }

        private int CheckWhichTerrain(Vector2 position)
        {
            for (var i = _startIndex; i < _endIndex; i++)
            {
                if (spriteShapeController.spline.GetPosition(i).x < position.x) continue;
                return spriteShapeController.spline.GetSpriteIndex(i - 1);
            }
            return 0;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            collision.collider.sharedMaterial = material;
        }

        public List<int> GetImpedimentList()
        {
            _startIndex = startPoint.GetComponent<NodeAttach>().index;
            _endIndex = endPoint.GetComponent<NodeAttach>().index;
            var list = new List<int>();
            for (var i = _startIndex; i < _endIndex; i++)
            {
                var newImpediment = spriteShapeController.spline.GetSpriteIndex(i);
                if (newImpediment == 0)
                    continue;

                newImpediment -= 1;
                if (!list.Contains(newImpediment))
                {
                    list.Add(newImpediment);
                }
            }

            return list;
        }
    }
}
