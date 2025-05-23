using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoaderLevel1 : MonoBehaviour
{
    public TaskManager taskManager;
    public TaskUIManager uiManager;

    void Start()
    {
        // Crear tareas con tiempo limitado (no infinitas)
        taskManager.AddTask(new Task(1, "Aprende a moverte", "Alcanza al Dubitador usando el Joystick en pantalla", true, 10f, false));
        taskManager.AddTask(new Task(2, "Aprende a saltar", "Parece que el Dubitador a dejado trampas por el camino. Saltalas usando el Joystick hacia arriba", true, 15f, false));
        taskManager.AddTask(new Task(3, "Aprende a atacar", "Usa tu reloj para acabar con el Dubitator", true, 20f, false));

        // Ya no es necesario llamar UpdateTaskList() aquí
        // El UI se actualizará automáticamente cuando el usuario abra el menú
        // uiManager.UpdateTaskList(); // ← Eliminar esta línea
        
        Debug.Log("📋 Tareas del intro cargadas. Se mostrarán cuando el usuario abra el menú.");
    }
}