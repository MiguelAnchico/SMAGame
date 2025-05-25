using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject newGamePanel;
    public GameObject levelSelectionPanel;

    void Start()
    {
        // Ensure the Main Menu panel is active and Level Selection panel is inactive
        mainMenuPanel.SetActive(true);
        levelSelectionPanel.SetActive(false);
    }

    // Method to show the Level Selection panel and hide the Main Menu panel
    public void NewGame()
    {
        mainMenuPanel.SetActive(false);
        newGamePanel.SetActive(true);
    }

    // Method to start the game (load the first level or continue)
    public void ContinueGame()
    {
        mainMenuPanel.SetActive(false);
        levelSelectionPanel.SetActive(true);
    }

    // Method to exit the game
    public void ExitGame()
    {
        Application.Quit();  // This will quit the application
    }
}
