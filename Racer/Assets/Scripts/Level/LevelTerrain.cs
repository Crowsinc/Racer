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

        [Header("Particle Materials")]
        public List<Material> terrainMaterials = new List<Material>();

        public GameObject particleEffect;

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

            // Get points of collision
            ContactPoint2D[] contactPoints = collision.contacts;
            if (_rbs[vehicleName].velocity.x > 1)
            {
                foreach (ContactPoint2D point in contactPoints)
                {
                    GameObject newParticle = Instantiate(particleEffect, new Vector3(point.point.x, point.point.y), Quaternion.Euler(-52.9f, -90, 90));
                    Debug.Log(terrainMaterials[CheckWhichTerrain(point.point)].name);
                    newParticle.GetComponent<ParticleSystemRenderer>().material = terrainMaterials[CheckWhichTerrain(point.point)];
                }
            }

            switch (CheckWhichTerrain(parent.position))
            {
                // Grass
                case 0:
                    material.friction = 0.6f;
                    break;
                // Mud
                case 1:
                    material.friction = 0.8f;
                    break;
                // Bouncy Gel
                case 2:
                    _rbs[vehicleName].AddForce(new Vector2(0, 10000));
                    break;
                // Speedy Gel
                case 3:
                    _rbs[vehicleName].AddForce(new Vector2(_rbs[vehicleName].velocity.x * 100, 0));
                    break;
                // Snow
                case 4:
                    material.friction = 1;
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
