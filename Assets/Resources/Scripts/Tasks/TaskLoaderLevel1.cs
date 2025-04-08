using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoaderLevel1 : MonoBehaviour
{
    public TaskManager taskManager;
    public TaskUIManager uiManager;

    void Start()
    {
        taskManager.AddTask(new Task(1, "Recoger la linterna", "Está sobre la mesa", 10f));
        taskManager.AddTask(new Task(2, "Hablar con el anciano", "Encuéntralo junto a la fogata", 15f));
        taskManager.AddTask(new Task(3, "Cruzar el puente", "Activa la palanca para bajarlo", 20f));

        uiManager.UpdateTaskList(); // Se actualiza la UI al inicio
    }
}