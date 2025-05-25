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
    public Sprite incompleteSprite;   // Imagen para tarea incompleta (tiempo agotado)

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
        // Reconfigurar referencias si están vacías
        ReconfigureReferences();
        
        if (taskPanel != null)
        {
            taskPanel.SetActive(false);
        }
        
        // Inicializar todas las imágenes como pendientes
        InitializeTaskImages();
    }

    private void ReconfigureReferences()
    {
        bool foundReferences = false;

        // Buscar TaskManager si no está asignado
        if (taskManager == null)
        {
            taskManager = TaskManager.Instance; // Usar la instancia singleton
            if (taskManager != null)
            {
                Debug.Log("🔗 TaskManager encontrado automáticamente");
                foundReferences = true;
            }
        }

        // Buscar referencias UI si están vacías
        if (taskPanel == null)
        {
            taskPanel = GameObject.Find("PanelTareas");
            if (taskPanel != null) Debug.Log("🔗 PanelTareas encontrado automáticamente");
        }

        // Buscar textos de tareas
        if (task1Text == null)
        {
            GameObject textObj = GameObject.Find("text1");
            if (textObj != null) task1Text = textObj.GetComponent<TextMeshProUGUI>();
        }
        if (task2Text == null)
        {
            GameObject textObj = GameObject.Find("text2");
            if (textObj != null) task2Text = textObj.GetComponent<TextMeshProUGUI>();
        }
        if (task3Text == null)
        {
            GameObject textObj = GameObject.Find("text3");
            if (textObj != null) task3Text = textObj.GetComponent<TextMeshProUGUI>();
        }

        // Buscar imágenes de estado
        if (task1StatusImage == null)
        {
            GameObject imageObj = GameObject.Find("Image");
            if (imageObj != null) task1StatusImage = imageObj.GetComponent<Image>();
        }
        if (task2StatusImage == null)
        {
            GameObject imageObj = GameObject.Find("Image (1)");
            if (imageObj != null) task2StatusImage = imageObj.GetComponent<Image>();
        }
        if (task3StatusImage == null)
        {
            GameObject imageObj = GameObject.Find("Image (2)");
            if (imageObj != null) task3StatusImage = imageObj.GetComponent<Image>();
        }

        // Método alternativo más robusto usando búsqueda por jerarquía
        if (taskPanel != null && (task1Text == null || task2Text == null || task3Text == null || 
                                  task1StatusImage == null || task2StatusImage == null || task3StatusImage == null))
        {
            SearchUIElementsInHierarchy();
        }

        if (foundReferences)
        {
            Debug.Log("🔄 Referencias UI reconfiguradas para nueva escena");
        }
    }

    private void SearchUIElementsInHierarchy()
    {
        // Buscar dentro del panel de tareas o en toda la escena
        Transform searchRoot = taskPanel != null ? taskPanel.transform : null;
        
        if (searchRoot == null)
        {
            // Buscar en Canvas si no encontramos el panel
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) searchRoot = canvas.transform;
        }

        if (searchRoot != null)
        {
            // Buscar todos los TextMeshProUGUI
            TextMeshProUGUI[] allTexts = searchRoot.GetComponentsInChildren<TextMeshProUGUI>();
            Image[] allImages = searchRoot.GetComponentsInChildren<Image>();

            // Asignar textos basándose en nombres o posición
            foreach (var text in allTexts)
            {
                if (text.name.Contains("text1") && task1Text == null)
                    task1Text = text;
                else if (text.name.Contains("text2") && task2Text == null)
                    task2Text = text;
                else if (text.name.Contains("text3") && task3Text == null)
                    task3Text = text;
            }

            // Asignar imágenes (excluyendo las que claramente no son de estado)
            int imageIndex = 0;
            foreach (var image in allImages)
            {
                // Saltar imágenes que claramente no son de estado de tareas
                if (image.name.ToLower().Contains("background") || 
                    image.name.ToLower().Contains("panel") ||
                    image.transform == taskPanel?.transform) continue;

                if (imageIndex == 0 && task1StatusImage == null)
                {
                    task1StatusImage = image;
                    imageIndex++;
                }
                else if (imageIndex == 1 && task2StatusImage == null)
                {
                    task2StatusImage = image;
                    imageIndex++;
                }
                else if (imageIndex == 2 && task3StatusImage == null)
                {
                    task3StatusImage = image;
                    break;
                }
            }

            Debug.Log("🔍 Búsqueda en jerarquía completada");
        }
    }

    // Método público para reconfigurar manualmente (útil para el TaskManager)
    public void ForceReconfigureReferences()
    {
        ReconfigureReferences();
    }

    void InitializeTaskImages()
    {
        HideImage(task1StatusImage);
        HideImage(task2StatusImage);
        HideImage(task3StatusImage);
    }

    public void ToggleTaskPanel()
    {
        if (taskPanel != null)
        {
            taskPanel.SetActive(!taskPanel.activeSelf);
            
            // IMPORTANTE: Siempre actualizar cuando se abre el panel
            if (taskPanel.activeSelf)
            {
                // Forzar actualización completa al abrir el menú
                ForceUpdateAllTaskStates();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ TaskPanel no encontrado - intentando reconfigurar referencias");
            ReconfigureReferences();
        }
    }

    // Método para forzar la actualización de todas las tareas cuando se abre el menú
    private void ForceUpdateAllTaskStates()
    {
        if (taskManager == null)
        {
            Debug.LogWarning("⚠️ TaskManager no encontrado - intentando reconfigurar");
            ReconfigureReferences();
            return;
        }

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
        if (taskManager == null)
        {
            Debug.LogWarning("⚠️ TaskManager no disponible para UpdateTaskList");
            return;
        }

        List<Task> tasks = taskManager.GetTasks();
        int currentTaskIndex = taskManager.GetCurrentTaskIndex();

        // Actualizar tarea 1
        if (tasks.Count > 0)
        {
            if (task1Text != null) task1Text.text = tasks[0].description;
            UpdateTaskImageBasedOnStatus(tasks[0], 0, currentTaskIndex, task1StatusImage);
        }
        else
        {
            if (task1Text != null) task1Text.text = "";
            HideImage(task1StatusImage);
        }

        // Actualizar tarea 2
        if (tasks.Count > 1)
        {
            if (task2Text != null) task2Text.text = tasks[1].description;
            UpdateTaskImageBasedOnStatus(tasks[1], 1, currentTaskIndex, task2StatusImage);
        }
        else
        {
            if (task2Text != null) task2Text.text = "";
            HideImage(task2StatusImage);
        }

        // Actualizar tarea 3
        if (tasks.Count > 2)
        {
            if (task3Text != null) task3Text.text = tasks[2].description;
            UpdateTaskImageBasedOnStatus(tasks[2], 2, currentTaskIndex, task3StatusImage);
        }
        else
        {
            if (task3Text != null) task3Text.text = "";
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
        if (taskManager == null) return;

        // Encontrar qué imagen corresponde a este ID de tarea
        List<Task> tasks = taskManager.GetTasks();
        int taskIndex = tasks.FindIndex(t => t.id == taskId);
        
        if (taskIndex == -1) return; // Tarea no encontrada

        Image targetImage = GetImageByIndex(taskIndex);
        if (targetImage == null) return;

        // Solo cambiar la imagen si el panel está activo
        if (taskPanel != null && taskPanel.activeSelf)
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

        Debug.Log($"🖼️ Imagen de tarea {taskId} cambiada a: {state} (Panel activo: {taskPanel?.activeSelf})");
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
        if (taskManager == null) return;
        
        List<Task> tasks = taskManager.GetTasks();
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            SetTaskImageState(tasks[taskIndex].id, state);
        }
    }
}