using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // Singleton instance
    public static GameStateManager Instance { get; private set; }

    // Estado del juego (0 a 1)
    private float gameState = 0f;

    // Propiedad para acceder y modificar el estado
    public float GameState
    {
        get { return gameState; }
        set { gameState = Mathf.Clamp01(value); }
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
    }

    public void SubtractFromState(float amount)
    {
        GameState = Mathf.Clamp01(gameState - amount);
    }
}