using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskLoaderLevel1 : MonoBehaviour
{
    public TaskManager taskManager;
    public TaskUIManager uiManager;

    [Header("Task Audio Clips")]
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;

    void Start()
    {
        ReconfigureReferences();

        // Obtener multiplicador basado en dificultad
        float difficultyMultiplier = GetDifficultyMultiplier();

        // Crear tareas con tiempo ajustado por dificultad
        taskManager.AddTask(new Task(1, "Aprende a moverte", "Habla con Ethan", true, 0f, true, audio1));
        taskManager.AddTask(new Task(2, "Aprende a saltar", "Derrota a Somnior", true, 50f * difficultyMultiplier, false, audio2));
        taskManager.AddTask(new Task(3, "Aprende a atacar", "Habla denuevo con Ethan", true, 0f, true, audio3));

        Debug.Log($"📋 Tareas cargadas con multiplicador de dificultad: {difficultyMultiplier}");
    }

    private float GetDifficultyMultiplier()
    {
        if (GameStateManager.Instance != null)
        {
            int difficulty = GameStateManager.Instance.LevelDifficulty;

            switch (difficulty)
            {
                case 0: // Fácil
                    return 1.0f;
                case 1: // Normal
                    return 0.75f;
                case 2: // Difícil
                    return 0.5f;
                default:
                    return 1.0f;
            }
        }
        else
        {
            Debug.LogWarning("GameStateManager no encontrado, usando dificultad por defecto");
            return 1.0f;
        }
    }

    private void ReconfigureReferences()
    {
        // Buscar TaskManager si no está asignado
        if (taskManager == null)
        {
            taskManager = TaskManager.Instance;
        }

        // Buscar TaskUIManager si no está asignado
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<TaskUIManager>();
        }
    }
}