using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // Singleton instance
    public static GameStateManager Instance { get; private set; }

    // Estado del juego (0 a 1)
    private float gameState = 0.5f;

    // Dificultad del nivel (0, 1, 2)
    private int levelDifficulty = 0;

    // Propiedad para acceder y modificar el estado
    public float GameState
    {
        get { return gameState; }
        set { gameState = Mathf.Clamp01(value); }
    }

    // Propiedad para acceder y modificar la dificultad del nivel
    public int LevelDifficulty
    {
        get { return levelDifficulty; }
        set { levelDifficulty = Mathf.Clamp(value, 0, 2); }
    }

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Métodos para modificar el estado
    public void SetState(float newState)
    {
        GameState = newState;
    }

    public void AddToState(float amount)
    {
        GameState = Mathf.Clamp01(gameState + amount);
        Debug.Log($"Estado del juego actualizado: {gameState}");
    }

    public void SubtractFromState(float amount)
    {
        GameState = Mathf.Clamp01(gameState - amount);
        Debug.Log($"Estado del juego actualizado: {gameState}");
    }

    // Métodos para modificar la dificultad del nivel
    public void SetLevelDifficulty(int difficulty)
    {
        LevelDifficulty = difficulty;
    }
}