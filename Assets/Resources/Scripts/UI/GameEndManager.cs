using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndManager : MonoBehaviour
{
    public GameObject gameOverCanvas;

    public void ShowGameEndScreen()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            Time.timeScale = 0f; // Pausa el juego
        }
        else
        {
            Debug.LogWarning("GameOverCanvas no asignado en el inspector.");
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Reanuda el juego antes de cambiar de escena
        SceneManager.LoadScene("MainMenu"); // Asegúrate de que la escena "MainMenu" esté en las configuraciones de construcción
    }
}

