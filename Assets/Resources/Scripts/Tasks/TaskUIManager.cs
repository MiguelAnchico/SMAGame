using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TaskUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject taskPanel;

    [Header("Task Texts")]
    public TextMeshProUGUI task1Text;
    public TextMeshProUGUI task2Text;
    public TextMeshProUGUI task3Text;

    [Header("Task Status Images")]
    public Image task1StatusImage;
    public Image task2StatusImage;
    public Image task3StatusImage;

    [Header("Status Sprites")]
    public Sprite inProgressSprite;   // Imagen para tarea en progreso
    public Sprite completedSprite;    // Imagen para tarea completada
    public Sprite incompleteSprite;   // Imagen para tarea incompleta (tiempo agotado)      // Imagen para tarea pendiente (opcional)

    [Header("References")]
    public TaskManager taskManager;

    public enum TaskState
    {
        Pending,     // Tarea pendiente (no iniciada)
        InProgress,  // Tarea actual en progreso
        Completed,   // Tarea completada exitosamente
        Incomplete   // Tarea no completada (tiempo agotado)
    }

    void Start()
    {
        taskPanel.SetActive(false);
        
        // Inicializar todas las imágenes como pendientes
        InitializeTaskImages();
    }

    void InitializeTaskImages()
    {
        HideImage(task1StatusImage);
        HideImage(task2StatusImage);
        HideImage(task3StatusImage);
    }

    public void ToggleTaskPanel()
    {
        taskPanel.SetActive(!taskPanel.activeSelf);
        
        // IMPORTANTE: Siempre actualizar cuando se abre el panel
        if (taskPanel.activeSelf)
        {
            // Forzar actualización completa al abrir el menú
            ForceUpdateAllTaskStates();
        }
    }

    // Método para forzar la actualización de todas las tareas cuando se abre el menú
    private void ForceUpdateAllTaskStates()
    {
        List<Task> tasks = taskManager.GetTasks();
        int currentTaskIndex = taskManager.GetCurrentTaskIndex();

        for (int i = 0; i < tasks.Count && i < 3; i++) // Máximo 3 tareas
        {
            Task task = tasks[i];
            Image statusImage = GetImageByIndex(i);
            
            if (statusImage != null)
            {
                if (task.isCompleted)
                {
                    SetImageSprite(statusImage, completedSprite);
                }
                else if (task.isFailed)
                {
                    SetImageSprite(statusImage, incompleteSprite);
                }
                else if (i == currentTaskIndex)
                {
                    SetImageSprite(statusImage, inProgressSprite);
                }
                else
                {
                    HideImage(statusImage);
                }
            }
        }

        // También actualizar los textos
        UpdateTaskList();
    }

    public void UpdateTaskList()
    {
        List<Task> tasks = taskManager.GetTasks();
        int currentTaskIndex = taskManager.GetCurrentTaskIndex();

        // Actualizar tarea 1
        if (tasks.Count > 0)
        {
            task1Text.text = tasks[0].description;
            UpdateTaskImageBasedOnStatus(tasks[0], 0, currentTaskIndex, task1StatusImage);
        }
        else
        {
            task1Text.text = "";
            HideImage(task1StatusImage);
        }

        // Actualizar tarea 2
        if (tasks.Count > 1)
        {
            task2Text.text = tasks[1].description;
            UpdateTaskImageBasedOnStatus(tasks[1], 1, currentTaskIndex, task2StatusImage);
        }
        else
        {
            task2Text.text = "";
            HideImage(task2StatusImage);
        }

        // Actualizar tarea 3
        if (tasks.Count > 2)
        {
            task3Text.text = tasks[2].description;
            UpdateTaskImageBasedOnStatus(tasks[2], 2, currentTaskIndex, task3StatusImage);
        }
        else
        {
            task3Text.text = "";
            HideImage(task3StatusImage);
        }
    }

    private void UpdateTaskImageBasedOnStatus(Task task, int taskIndex, int currentTaskIndex, Image statusImage)
    {
        if (task.isCompleted)
        {
            SetImageSprite(statusImage, completedSprite);
        }
        else if (task.isFailed)
        {
            SetImageSprite(statusImage, incompleteSprite);
        }
        else if (taskIndex == currentTaskIndex)
        {
            SetImageSprite(statusImage, inProgressSprite);
        }
        else
        {
            HideImage(statusImage); // No mostrar imagen para tareas pendientes
        }
    }

    public void SetTaskImageState(int taskId, TaskState state)
    {
        // Encontrar qué imagen corresponde a este ID de tarea
        List<Task> tasks = taskManager.GetTasks();
        int taskIndex = tasks.FindIndex(t => t.id == taskId);
        
        if (taskIndex == -1) return; // Tarea no encontrada

        Image targetImage = GetImageByIndex(taskIndex);
        if (targetImage == null) return;

        // Solo cambiar la imagen si el panel está activo
        if (taskPanel.activeSelf)
        {
            // Establecer el sprite apropiado
            switch (state)
            {
                case TaskState.Pending:
                    HideImage(targetImage);
                    break;
                case TaskState.InProgress:
                    SetImageSprite(targetImage, inProgressSprite);
                    break;
                case TaskState.Completed:
                    SetImageSprite(targetImage, completedSprite);
                    break;
                case TaskState.Incomplete:
                    SetImageSprite(targetImage, incompleteSprite);
                    break;
            }
        }

        Debug.Log($"🖼️ Imagen de tarea {taskId} cambiada a: {state} (Panel activo: {taskPanel.activeSelf})");
    }

    private Image GetImageByIndex(int index)
    {
        switch (index)
        {
            case 0: return task1StatusImage;
            case 1: return task2StatusImage;
            case 2: return task3StatusImage;
            default: return null;
        }
    }

    private void SetImageSprite(Image image, Sprite sprite)
    {
        if (image != null && sprite != null)
        {
            image.sprite = sprite;
            image.enabled = true;
        }
        else if (image != null)
        {
            image.enabled = false;
        }
    }

    private void HideImage(Image image)
    {
        if (image != null)
        {
            image.enabled = false;
        }
    }

    // Métodos públicos para control externo
    public void RefreshUI()
    {
        UpdateTaskList();
    }

    public void SetTaskAsCurrentFromUI(int taskIndex)
    {
        if (taskManager != null)
        {
            taskManager.SetCurrentTask(taskIndex);
            UpdateTaskList();
        }
    }

    // Método para testing - cambiar estado manualmente
    [System.Obsolete("Solo para testing")]
    public void TestChangeTaskState(int taskIndex, TaskState state)
    {
        List<Task> tasks = taskManager.GetTasks();
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            SetTaskImageState(tasks[taskIndex].id, state);
        }
    }
}