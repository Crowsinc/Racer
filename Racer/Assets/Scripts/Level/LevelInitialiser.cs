using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    public class LevelInitialiser : MonoBehaviour
    {
        public Level selectedLevel;
        public GameObject currentLevel;
        public List<Level> levelCollection;
        public GameObject backgroundTemplate;

        private void Awake()
        {
            selectedLevel = FindLevelById(PlayerPrefs.GetInt(GameConstants.PPKEY_SELECTED_LEVEL));
            // Creating terrain
            currentLevel = Instantiate(selectedLevel.terrain, Vector3.zero, Quaternion.identity);

            GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>().opponentVehicle = selectedLevel.opponentVehicle;
            Physics2D.gravity = selectedLevel.gravity;

            // Initialise parallax backgrounds
            foreach (var background in selectedLevel.backgrounds)
            {
                PlaceBackground(background, 0);

                // Create left background
                PlaceBackground(background, background.image.bounds.size.x);

                // Create right background
                PlaceBackground(background, -background.image.bounds.size.x);

            }
        }
        private void PlaceBackground(ParallaxBackground background, float offset)
        {
            if (Camera.main == null) return;
            var bg = Instantiate(backgroundTemplate, Camera.main.transform);
            bg.transform.localPosition = new Vector3(offset, 0, 10);

            var parallax = bg.GetComponent<Parallax>();
            parallax.parallaxEffect = background.parallaxDegree;

            var spriteRenderer = bg.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = background.layerOrder;
            spriteRenderer.sprite = background.image;
        }

        // Find level in collection that matches id
        private Level FindLevelById(int id)
        {
            foreach (var level in levelCollection.Where(level => level.levelId == id))
            {
                return level;
            }

            return levelCollection[0];
        }
    }
}
