using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TaskManager : MonoBehaviour
{
    [Header("Task Management")]
    private List<Task> tasks = new List<Task>();
    private int currentTaskIndex = -1; // Índice de la tarea actual (-1 = ninguna)
    
    [Header("References")]
    public TaskUIManager taskUIManager;
    public NotificationManager notificationManager;
    
    [Header("Completion Events")]
    public UnityEvent OnAllTasksCompleted; // Evento cuando todas las tareas se completan
    public UnityEvent OnAllTasksFinished;  // Evento cuando todas las tareas terminan (completadas o fallidas)
    
    [Header("Individual Task Events")]
    public UnityEvent<int> OnTaskCompleted; // Evento cuando se completa una tarea individual (pasa el ID)
    public UnityEvent<int> OnTaskFailed;    // Evento cuando falla una tarea individual (pasa el ID)

    [Header("Scene-Specific Events")]
    public UnityEvent OnSceneTasksCompleted; // Eventos específicos de la escena actual
    public UnityEvent OnSceneTasksFailed;    // Eventos específicos cuando fallan tareas en la escena

    public static TaskManager Instance { get; private set; }
    
    private bool allTasksCompletedEventFired = false;
    private bool allTasksFinishedEventFired = false;

    private void Start()
    {
        // Reconfigurar referencias para la escena actual
        ReconfigureReferences();
    }

    private void OnLevelWasLoaded(int level)
    {
        // Unity legacy - se llama cuando cambia la escena
        ReconfigureReferences();
    }

    private void ReconfigureReferences()
    {
        // Buscar el TaskUIManager en la nueva escena
        TaskUIManager newTaskUIManager = FindObjectOfType<TaskUIManager>();
        
        if (newTaskUIManager != null && newTaskUIManager != taskUIManager)
        {
            // Transferir eventos del nuevo TaskUIManager al singleton
            TransferEventsFromNewUIManager(newTaskUIManager);
            
            // Asignar el nuevo como referencia
            taskUIManager = newTaskUIManager;
            
            // Destruir el nuevo TaskUIManager ya que transferimos sus eventos
            Destroy(newTaskUIManager.gameObject);
        }
        else if (taskUIManager == null)
        {
            taskUIManager = newTaskUIManager;
        }
        
        // Buscar el NotificationManager en la nueva escena
        if (notificationManager == null)
        {
            notificationManager = FindObjectOfType<NotificationManager>();
        }

        // Actualizar el UI si existe
        if (taskUIManager != null)
        {
            taskUIManager.RefreshUI();
        }

        Debug.Log($"🔗 Referencias reconfiguradas para nueva escena");
    }

    private void TransferEventsFromNewUIManager(TaskUIManager newUIManager)
    {
        // Verificar si el nuevo UIManager tiene un TaskManager configurado con eventos
        TaskManager newTaskManager = newUIManager.GetComponent<TaskManager>();
        
        if (newTaskManager != null)
        {
            // Transferir eventos específicos de la escena
            if (newTaskManager.OnSceneTasksCompleted != null && newTaskManager.OnSceneTasksCompleted.GetPersistentEventCount() > 0)
            {
                OnSceneTasksCompleted = newTaskManager.OnSceneTasksCompleted;
            }
            
            if (newTaskManager.OnSceneTasksFailed != null && newTaskManager.OnSceneTasksFailed.GetPersistentEventCount() > 0)
            {
                OnSceneTasksFailed = newTaskManager.OnSceneTasksFailed;
            }
            
            // Transferir otros eventos globales si es necesario
            if (newTaskManager.OnAllTasksCompleted != null && newTaskManager.OnAllTasksCompleted.GetPersistentEventCount() > 0)
            {
                OnAllTasksCompleted = newTaskManager.OnAllTasksCompleted;
            }
            
            if (newTaskManager.OnAllTasksFinished != null && newTaskManager.OnAllTasksFinished.GetPersistentEventCount() > 0)
            {
                OnAllTasksFinished = newTaskManager.OnAllTasksFinished;
            }
        }
        
        Debug.Log($"🔄 Eventos transferidos del TaskUIManager de la nueva escena");
    }

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
            
            // Solo modificar estado de procrastinación si la tarea tiene tiempo
            if (task.timeRemaining > 0f && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SubtractFromState(0.5f);
            }

            // Disparar evento de tarea individual completada
            OnTaskCompleted?.Invoke(id);
            
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
            
            // Verificar si todas las tareas están completadas
            CheckAllTasksCompletion();

            task.GetTimeRemainingText();

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
            tasks[taskIndex].OnBecomeCurrentTask();

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
        
        // Verificar eventos de finalización
        CheckAllTasksCompletion();
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

                    if (GameStateManager.Instance != null)
                    {
                        GameStateManager.Instance.AddToState(0.5f);
                    }
                    // Disparar evento de tarea individual fallida
                    OnTaskFailed?.Invoke(currentTask.id);
                    
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
                    
                    // Verificar si todas las tareas están terminadas
                    CheckAllTasksCompletion();
                    
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

    void CheckAllTasksCompletion()
    {
        if (tasks.Count == 0) return;
        
        int completedTasks = 0;
        int finishedTasks = 0; // Completadas + fallidas
        
        foreach (Task task in tasks)
        {
            if (task.isCompleted)
            {
                completedTasks++;
                finishedTasks++;
            }
            else if (task.isFailed)
            {
                finishedTasks++;
            }
        }
        
        // Verificar si TODAS las tareas están completadas (exitosamente)
        if (completedTasks == tasks.Count && !allTasksCompletedEventFired)
        {
            allTasksCompletedEventFired = true;
            Debug.Log("🎊 ¡TODAS LAS TAREAS COMPLETADAS EXITOSAMENTE!");
            OnAllTasksCompleted?.Invoke();
            OnSceneTasksCompleted?.Invoke(); // Ejecutar eventos específicos de escena
        }
        
        // Verificar si TODAS las tareas han terminado (completadas o fallidas)
        if (finishedTasks == tasks.Count && !allTasksFinishedEventFired)
        {
            allTasksFinishedEventFired = true;
            Debug.Log("🏁 ¡TODAS LAS TAREAS HAN TERMINADO!");
            OnAllTasksFinished?.Invoke();
            
            // Si hay tareas fallidas, ejecutar eventos de fallo específicos de escena
            if (GetFailedTasksCount() > 0)
            {
                OnSceneTasksFailed?.Invoke();
            }
        }
    }
    
    // Métodos públicos para obtener estado de finalización
    public bool AreAllTasksCompleted()
    {
        if (tasks.Count == 0) return false;
        return tasks.TrueForAll(task => task.isCompleted);
    }
    
    public bool AreAllTasksFinished()
    {
        if (tasks.Count == 0) return false;
        return tasks.TrueForAll(task => task.isCompleted || task.isFailed);
    }
    
    public int GetCompletedTasksCount()
    {
        return tasks.FindAll(task => task.isCompleted).Count;
    }
    
    public int GetFailedTasksCount()
    {
        return tasks.FindAll(task => task.isFailed).Count;
    }
    
    public int GetActiveTasksCount()
    {
        return tasks.FindAll(task => task.IsActive()).Count;
    }
    
    // Método para resetear los eventos (útil para reiniciar nivel)
    public void ResetCompletionEvents()
    {
        allTasksCompletedEventFired = false;
        allTasksFinishedEventFired = false;
    }
}