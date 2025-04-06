using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TaskUIManager : MonoBehaviour
{
    public GameObject taskPanel; // Panel que se muestra/oculta con botón

    public TextMeshProUGUI task1Text;
    public TextMeshProUGUI task2Text;
    public TextMeshProUGUI task3Text;

    public Image check1;
    public Image check2;
    public Image check3;

    public TaskManager taskManager; // Asignar en el inspector

    void Start()
    {
        taskPanel.SetActive(false);
        check1.enabled = false;
        check2.enabled = false;
        check3.enabled = false;
    }

    public void ToggleTaskPanel()
    {
        taskPanel.SetActive(!taskPanel.activeSelf);
    }

    public void UpdateTaskList()
    {
        List<Task> tasks = taskManager.GetTasks();

        if (tasks.Count > 0)
        {
            task1Text.text = tasks[0].description;
            check1.enabled = tasks[0].isCompleted;
        }
        if (tasks.Count > 1)
        {
            task2Text.text = tasks[1].description;
            check2.enabled = tasks[1].isCompleted;
        }
        if (tasks.Count > 2)
        {
            task3Text.text = tasks[2].description;
            check3.enabled = tasks[2].isCompleted;
        }
    }
}