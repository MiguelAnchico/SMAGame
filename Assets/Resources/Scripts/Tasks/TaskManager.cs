using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Task Management")]
    private List<Task> tasks = new List<Task>();
    private int currentTaskIndex = -1; // Índice de la tarea actual (-1 = ninguna)
    
    [Header("References")]
    public TaskUIManager taskUIManager;
    public NotificationManager notificationManager;

    public static TaskManager Instance { get; private set; }

    private void Awake()
    {
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

    public void AddTask(Task task) 
    {
        tasks.Add(task);
        
        // Si es la primera tarea, hacerla la tarea actual automáticamente
        if (tasks.Count == 1)
        {
            SetCurrentTask(0);
        }
    }

    public void CompleteTask(int id)
    {
        Task task = tasks.Find(t => t.id == id);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            Debug.Log($"✅ Tarea completada: {task.title}");
            
            // Mostrar notificación de tarea completada
            if (notificationManager != null)
            {
                notificationManager.ShowTaskCompleted(task);
            }
            
            // Notificar al UI que la tarea se completó (solo si el UI está disponible)
            if (taskUIManager != null)
            {
                taskUIManager.SetTaskImageState(id, TaskUIManager.TaskState.Completed);
            }
            
            // Si la tarea completada era la actual, pasar a la siguiente
            int taskIndex = tasks.FindIndex(t => t.id == id);
            if (taskIndex == currentTaskIndex)
            {
                MoveToNextIncompleteTask();
            }
        }
    }

    public void SetCurrentTask(int taskIndex)
    {
        if (taskIndex < 0 || taskIndex >= tasks.Count)
            return;

        // Si ya hay una tarea actual diferente, no hacer nada por ahora
        if (currentTaskIndex != taskIndex)
        {
            currentTaskIndex = taskIndex;
            
            // Mostrar notificación de nueva tarea
            if (notificationManager != null)
            {
                notificationManager.ShowTaskNotification(tasks[taskIndex]);
            }
            
            // Notificar al UI que esta tarea está en progreso (solo si el UI está disponible)
            if (taskUIManager != null && !tasks[taskIndex].isCompleted)
            {
                taskUIManager.SetTaskImageState(tasks[taskIndex].id, TaskUIManager.TaskState.InProgress);
            }
            
            Debug.Log($"🎯 Tarea actual: {tasks[taskIndex].title}");
        }
    }

    private void MoveToNextIncompleteTask()
    {
        // Buscar la siguiente tarea activa (no completada ni fallida)
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].IsActive())
            {
                SetCurrentTask(i);
                Debug.Log($"🔄 Cambiando a la siguiente tarea: {tasks[i].title}");
                return;
            }
        }
        
        // Si no hay más tareas activas
        currentTaskIndex = -1;
        Debug.Log("🎉 ¡Todas las tareas completadas o fallidas!");
    }

    public List<Task> GetTasks() => tasks;
    
    public Task GetCurrentTask()
    {
        if (currentTaskIndex >= 0 && currentTaskIndex < tasks.Count)
        {
            return tasks[currentTaskIndex];
        }
        return null;
    }

    public int GetCurrentTaskIndex() => currentTaskIndex;

    void Update()
    {
        // Solo actualizar el timer de la tarea actual
        if (currentTaskIndex >= 0 && currentTaskIndex < tasks.Count)
        {
            Task currentTask = tasks[currentTaskIndex];
            
            // Solo actualizar si tiene reminder Y no es tiempo infinito Y está activa (no completada ni fallida)
            if (currentTask.HasReminder() && !currentTask.IsInfiniteTime() && currentTask.IsActive() && currentTask.timeRemaining > 0)
            {
                currentTask.UpdateTimer(Time.deltaTime);
                
                if (currentTask.timeRemaining <= 0)
                {
                    // Tiempo agotado para la tarea actual
                    Debug.Log($"⏰ Tiempo agotado para: {currentTask.title}");
                    
                    // Marcar la tarea como fallida
                    currentTask.MarkAsFailed();
                    
                    // Mostrar notificación de tarea fallida
                    if (notificationManager != null)
                    {
                        notificationManager.ShowTaskFailed(currentTask);
                    }
                    
                    // Marcar como incompleta en el UI (solo si el UI está disponible)
                    if (taskUIManager != null)
                    {
                        taskUIManager.SetTaskImageState(currentTask.id, TaskUIManager.TaskState.Incomplete);
                    }
                    
                    // Pasar a la siguiente tarea
                    MoveToNextIncompleteTask();
                }
            }
        }
    }

    // Métodos públicos para control manual de tareas
    public void SetTaskAsIncomplete(int id)
    {
        Task task = tasks.Find(t => t.id == id);
        if (task != null && !task.isCompleted)
        {
            if (taskUIManager != null)
            {
                taskUIManager.SetTaskImageState(id, TaskUIManager.TaskState.Incomplete);
            }
        }
    }

    // Método público para que el UI se actualice cuando se abra el menú
    public void RefreshUI()
    {
        if (taskUIManager != null)
        {
            taskUIManager.RefreshUI();
        }
    }

    // Método para obtener información de debug
    public void LogTaskStatus()
    {
        Debug.Log($"=== ESTADO DE TAREAS ===");
        Debug.Log($"Tarea actual: {(currentTaskIndex >= 0 ? tasks[currentTaskIndex].title : "Ninguna")}");
        
        for (int i = 0; i < tasks.Count; i++)
        {
            Task task = tasks[i];
            string status = task.isCompleted ? "✅ Completada" : 
                           (i == currentTaskIndex ? "🎯 En progreso" : "⏸️ Pendiente");
            Debug.Log($"Tarea {i + 1}: {task.title} - {status}");
        }
    }
}