using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoaderLevel1 : MonoBehaviour
{
    public TaskManager taskManager;
    public TaskUIManager uiManager;

    void Start()
    {
        taskManager.AddTask(new Task(1, "Aprende a moverte", "Desplazate hacia la derecha hasta que desaparezca el mensaje", 10f));
        taskManager.AddTask(new Task(2, "Aprende a saltar", "Salta sobre el primer mensaje de como saltar", 15f));
        taskManager.AddTask(new Task(3, "Cruzar el puente", "Activa la palanca para bajarlo", 20f));

        uiManager.UpdateTaskList();
    }
}