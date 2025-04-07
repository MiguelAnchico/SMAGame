using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour
{
    public GameObject levelSelectionPanel;
    public GameObject mainMenuPanel;

    // Method to load the selected level
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName); // Load the level based on button clicked
    }

    // Method to go back to the Main Menu
    public void ReturnToMainMenu()
    {
        levelSelectionPanel.SetActive(false);  // Hide the level selection panel
        mainMenuPanel.SetActive(true);         // Show the main menu
    }
}
