using UnityEngine;

public class MovementTaskCompleter : MonoBehaviour
{
    [Header("Player Detection")]
    public Transform player;
    public float movementThreshold = 0.1f; // Distancia mínima para considerar movimiento
    
    [Header("Task Settings")]
    public int taskIdToComplete = 1; // Tarea número 1
    
    private Vector3 initialPosition;
    private bool hasDetectedMovement = false;
    private bool isActive = true;
    
    void Start()
    {
        if (player != null)
        {
            initialPosition = player.position;
        }
        else
        {
            Debug.LogWarning("⚠️ MovementTaskCompleter: No se asignó el player");
        }
    }
    
    void Update()
    {
        if (!isActive || player == null || hasDetectedMovement) return;
        
        // Verificar si el player se ha movido
        float distanceMoved = Vector3.Distance(player.position, initialPosition);
        
        if (distanceMoved > movementThreshold)
        {
            OnMovementDetected();
            hasDetectedMovement = true;
            isActive = false; // Desactivar después de detectar movimiento
        }
    }
    
    void OnMovementDetected()
    {
        // Completar la tarea número 1
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
        }
    }
    
    // Métodos públicos para control externo
    public void ResetDetection()
    {
        if (player != null)
        {
            initialPosition = player.position;
        }
        hasDetectedMovement = false;
        isActive = true;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
    }
    
    public void SetMovementThreshold(float threshold)
    {
        movementThreshold = threshold;
    }
    
    // Información de debug visual
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Dibujar la posición inicial
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(initialPosition, movementThreshold);
            
            // Dibujar línea desde posición inicial a actual
            Gizmos.color = hasDetectedMovement ? Color.green : Color.red;
            Gizmos.DrawLine(initialPosition, player.position);
            
            // Dibujar posición actual
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, 0.1f);
        }
    }
}