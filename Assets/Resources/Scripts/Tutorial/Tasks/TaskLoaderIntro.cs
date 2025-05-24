using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoaderIntro : MonoBehaviour
{
    [Header("Managers")]
    public TaskManager taskManager;
    public TaskUIManager uiManager;
    
    [Header("Task Audio Clips")]
    public AudioClip moveTaskAudio;
    public AudioClip jumpTaskAudio;
    public AudioClip attackTaskAudio;

    void Start()
    {
        // Crear tareas con tiempo infinito y audio obligatorio
        taskManager.AddTask(new Task(1, "Aprende a moverte", "Alcanza al Dubitador usando el Joystick en pantalla", true, 0f, true, moveTaskAudio));
        taskManager.AddTask(new Task(2, "Aprende a saltar", "Parece que hay obstáculos por el camino, salta usando el Joystick hacia arriba", true, 0f, true, jumpTaskAudio));
        taskManager.AddTask(new Task(3, "Aprende a atacar", "Usa tu reloj para acabar con el Dubitator", true, 0f, true, attackTaskAudio));

        Debug.Log("📋 Tareas del intro cargadas. Se mostrarán cuando el usuario abra el menú.");
    }
}