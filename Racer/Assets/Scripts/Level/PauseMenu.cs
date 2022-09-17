using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    public class PauseMenu : MonoBehaviour
    {
        private bool _menuOpen;
        private bool _buttonDown;
        public GameObject pauseMenu;
        private SimulationController _simulationController;

        private void Awake()
        {
            _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
        }

        private void Update()
        {
            //Debug.Log(Input.GetAxisRaw("Cancel"));
            if (Input.GetAxisRaw("Cancel") > 0 && !_buttonDown && !_simulationController.inBuildMode)
            {
                _buttonDown = true;
                if (_menuOpen)
                    ClosePauseMenu();

                else
                    OpenPauseMenu();
            }
            else if (!(Input.GetAxisRaw("Cancel") > 0))
            {
                _buttonDown = false;
            }
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadSceneAsync(GameConstants.MAIN_MENU_SCENE_ID);
        }

        public void OpenPauseMenu()
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            _menuOpen = true;
        }

        public void ClosePauseMenu()
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            _menuOpen = false;
        }
    }
}
