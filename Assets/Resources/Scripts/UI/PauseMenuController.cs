using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;   

    // Method to pause the game and show the pause menu
    public void PauseGame()
    {
        Time.timeScale = 0f;              
        pauseMenuPanel.SetActive(true);   
    }

    // Method to continue the game and hide the pause menu
    public void ContinueGame()
    {
        Time.timeScale = 1f;              
        pauseMenuPanel.SetActive(false);  
    }

    // Method to exit the game (this can load the main menu or quit)
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;              
        SceneManager.LoadScene("MainMenu"); 
    }
}
