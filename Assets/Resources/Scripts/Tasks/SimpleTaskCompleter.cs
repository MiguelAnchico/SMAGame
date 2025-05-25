using UnityEngine;

public class SimpleTaskCompleter : MonoBehaviour
{
    [Header("Task Settings")]
    public int taskIdToComplete = 1;

    // Función pública para completar la tarea
    public void CompleteTask()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            Debug.Log($"Tarea {taskIdToComplete} completada manualmente");
        }
        else
        {
            Debug.LogWarning("TaskManager no encontrado");
        }
    }

    // Función para completar una tarea específica
    public void CompleteTask(int taskId)
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskId);
            Debug.Log($"Tarea {taskId} completada manualmente");
        }
        else
        {
            Debug.LogWarning("TaskManager no encontrado");
        }
    }
}