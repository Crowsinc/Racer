using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private bool _menuOpen = false;
    private bool _buttonDown = false;
    public GameObject pauseMenu;
    private SimulationController _simulationController;

    private void Awake()
    {
        _simulationController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>();
    }

    void Update()
    {
        Debug.Log(Input.GetAxisRaw("Cancel"));
        if (Input.GetAxisRaw("Cancel") > 0 && !_buttonDown && !_simulationController.inBuildMode)
        {
            _buttonDown = true;
            if (_menuOpen)
            {
                Time.timeScale = 1;
                pauseMenu.SetActive(false);
                _menuOpen = false;
            }
            else
            {
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
                _menuOpen = true;
            }
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
}
