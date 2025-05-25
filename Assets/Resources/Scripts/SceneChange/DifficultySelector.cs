using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string targetSceneName = "GameLevel";

    // Método para dificultad fácil (0)
    public void SelectEasyDifficulty()
    {
        SetDifficultyAndLoadScene(0);
    }

    // Método para dificultad normal (1)
    public void SelectNormalDifficulty()
    {
        SetDifficultyAndLoadScene(1);
    }

    // Método para dificultad difícil (2)
    public void SelectHardDifficulty()
    {
        SetDifficultyAndLoadScene(2);
    }

    // Método general para establecer dificultad y cargar escena
    private void SetDifficultyAndLoadScene(int difficulty)
    {
        // Establecer la dificultad en el GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetLevelDifficulty(difficulty);
            Debug.Log($"Dificultad establecida a: {difficulty}");
        }
        else
        {
            Debug.LogWarning("GameStateManager no encontrado");
        }

        // Cargar la escena
        SceneManager.LoadScene(targetSceneName);
    }

    // Método público alternativo para usar con cualquier dificultad
    public void SetDifficultyAndLoad(int difficulty)
    {
        SetDifficultyAndLoadScene(difficulty);
    }
}