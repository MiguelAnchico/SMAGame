using UnityEngine;
using UnityEngine.Events;

public class ProcrastinationStateChecker : MonoBehaviour
{
    [Header("State Events")]
    [Space]
    public UnityEvent OnMaxProcrastination;     // Eventos cuando el estado = 1
    public UnityEvent OnMediumProcrastination;  // Eventos cuando el estado = 0.5
    public UnityEvent OnMinProcrastination;     // Eventos cuando el estado = 0
    
    [Header("Settings")]
    public float tolerance = 0.01f; // Tolerancia para comparar floats
    public float destroyDelay = 0.1f; // Tiempo antes de destruirse
    
    void Start()
    {
        CheckProcrastinationState();
    }

    private void CheckProcrastinationState()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager no encontrado");
            Destroy(gameObject, destroyDelay);
            return;
        }

        float currentState = GameStateManager.Instance.GameState;
        Debug.Log($"Estado de procrastinación actual: {currentState}");

        // Verificar el estado con tolerancia para comparar floats
        if (Mathf.Approximately(currentState, 1f))
        {
            Debug.Log("Ejecutando eventos de máxima procrastinación");
            OnMaxProcrastination?.Invoke();
        }
        else if (Mathf.Abs(currentState - 0.5f) <= tolerance)
        {
            Debug.Log("Ejecutando eventos de procrastinación media");
            OnMediumProcrastination?.Invoke();
        }
        else if (Mathf.Approximately(currentState, 0f))
        {
            Debug.Log("Ejecutando eventos de mínima procrastinación");
            OnMinProcrastination?.Invoke();
        }
        else
        {
            Debug.Log($"Estado de procrastinación no coincide con valores esperados: {currentState}");
        }

        // Destruirse después del delay
        Destroy(gameObject, destroyDelay);
    }

    // Método público para verificar manualmente (por si quieres llamarlo desde otro script)
    public void ManualCheck()
    {
        CheckProcrastinationState();
    }

    // Método para verificar un valor específico
    public void CheckSpecificValue(float targetValue)
    {
        if (GameStateManager.Instance == null) return;

        float currentState = GameStateManager.Instance.GameState;
        
        if (Mathf.Abs(currentState - targetValue) <= tolerance)
        {
            Debug.Log($"El estado actual ({currentState}) coincide con el valor objetivo ({targetValue})");
            
            // Ejecutar evento específico según el valor
            if (Mathf.Approximately(targetValue, 1f))
                OnMaxProcrastination?.Invoke();
            else if (Mathf.Abs(targetValue - 0.5f) <= tolerance)
                OnMediumProcrastination?.Invoke();
            else if (Mathf.Approximately(targetValue, 0f))
                OnMinProcrastination?.Invoke();
        }
        
        Destroy(gameObject, destroyDelay);
    }
}