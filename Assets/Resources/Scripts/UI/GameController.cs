using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PauseMenuController pauseMenuController;
    void Update()
    {
        // When Escape key is pressed, pause the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenuController.PauseGame();
        }
    }
}
