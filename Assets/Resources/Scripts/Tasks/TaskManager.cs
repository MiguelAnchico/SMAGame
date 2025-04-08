using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private List<Task> tasks = new List<Task>();

    public void AddTask(Task task) => tasks.Add(task);

    public void CompleteTask(int id)
    {
        Task task = tasks.Find(t => t.id == id);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            Debug.Log($"✅ Tarea completada: {task.title}");
        }
    }

    public List<Task> GetTasks() => tasks;

    public static TaskManager Instance { get; private set; }

    private void Awake()
{
    if (Instance == null)
        Instance = this;
    else
        Destroy(gameObject);
}

    void Update()
    {
        foreach (var task in tasks)
        {
            if (task.HasReminder())
            {
                task.UpdateTimer(Time.deltaTime);
                if (task.timeRemaining <= 0 && !task.isCompleted)
                {
                    Debug.Log("⏰ Recordatorio: " + task.title);
                }
            }
        }
    }
}