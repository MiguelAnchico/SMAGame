using UnityEngine;
using UnityEngine.Events;

public class AutoEventTrigger : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnStart; // Eventos que se ejecutan en Start()
    
    [Header("Settings")]
    public float delay = 0f; // Delay antes de ejecutar los eventos
    public bool destroyAfterExecution = false; // Si se destruye después de ejecutar
    
    [Header("TaskManager Integration")]
    public bool reconfigureTaskManager = false; // Si debe reconfigurar el TaskManager
    
    void Start()
    {
        if (delay > 0f)
        {
            Invoke(nameof(ExecuteEvents), delay);
        }
        else
        {
            ExecuteEvents();
        }
    }
    
    private void ExecuteEvents()
    {
        OnStart?.Invoke();
        Debug.Log("Eventos automáticos ejecutados");
        
        // Ejecutar reconfiguración del TaskManager si está activado
        if (reconfigureTaskManager)
        {
            ReconfigureTaskManager();
        }
        
        if (destroyAfterExecution)
        {
            Destroy(gameObject);
        }
    }
    
    private void ReconfigureTaskManager()
    {
        if (TaskManager.Instance != null)
        {
            // Usar reflexión para llamar al método privado ReconfigureReferences del TaskManager
            var method = typeof(TaskManager).GetMethod("ReconfigureReferences", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(TaskManager.Instance, null);
                Debug.Log("TaskManager.ReconfigureReferences() ejecutado");
            }
        }
        else
        {
            Debug.LogWarning("TaskManager.Instance no encontrado");
        }
    }
}