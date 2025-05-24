using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskTrigger : MonoBehaviour
{
    [Header("Task Settings")]
    public int taskIdToComplete = 1;
    
    [Header("Trigger Settings")]
    public bool destroyAfterTrigger = true;
    public bool oneTimeUse = true;
    
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificar si ya fue usado (para one-time use)
            if (oneTimeUse && hasBeenTriggered)
                return;
                
            // Completar la tarea usando el TaskManager
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(taskIdToComplete);
                hasBeenTriggered = true;
                
                // El TaskManager automáticamente:
                // 1. Marca la tarea como completada
                // 2. Llama al NotificationManager para mostrar "¡Tarea Completada!"
                // 3. Actualiza el UI cuando se abra el menú
                // 4. Cambia a la siguiente tarea disponible
                
                // Destruir el trigger si está configurado para hacerlo
                if (destroyAfterTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // Método público para resetear el trigger
    public void ResetTrigger()
    {
        hasBeenTriggered = false;
    }
    
    // Método público para cambiar la tarea a completar
    public void SetTaskId(int newTaskId)
    {
        taskIdToComplete = newTaskId;
    }
}