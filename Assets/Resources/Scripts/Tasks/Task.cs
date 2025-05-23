using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public int id;
    public string title;
    public string description;
    public bool isCompleted;
    public bool isFailed; // Nueva propiedad para tareas fallidas
    public float timeRemaining;
    public bool hasReminder;
    public bool infiniteTime; // Nueva variable para tiempo infinito
    public AudioClip audioClip; // Audio para reproducir cuando se muestra la notificación

    public Task(int taskId, string taskTitle, string taskDescription, bool reminder = false, float time = 0f, bool isInfinite = false, AudioClip audio = null)
    {
        id = taskId;
        title = taskTitle;
        description = taskDescription;
        isCompleted = false;
        isFailed = false;
        hasReminder = reminder;
        timeRemaining = time;
        infiniteTime = isInfinite;
        audioClip = audio;
    }

    public bool HasReminder()
    {
        return hasReminder;
    }

    public bool IsInfiniteTime()
    {
        return infiniteTime;
    }

    public bool IsActive()
    {
        return !isCompleted && !isFailed;
    }

    public void UpdateTimer(float deltaTime)
    {
        // Solo actualizar el timer si no es tiempo infinito y no está fallida
        if (!infiniteTime && hasReminder && !isFailed)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining < 0)
                timeRemaining = 0;
        }
    }

    public void MarkAsFailed()
    {
        isFailed = true;
        timeRemaining = 0f;
    }

    public void SetInfiniteTime(bool infinite)
    {
        infiniteTime = infinite;
        if (infinite)
        {
            timeRemaining = float.MaxValue; // Establecer tiempo muy alto
        }
    }

    public void SetAudioClip(AudioClip audio)
    {
        audioClip = audio;
    }

    public string GetTimeRemainingText()
    {
        if (isFailed)
        {
            return "Fallida";
        }
        else if (isCompleted)
        {
            return "Completada";
        }
        else if (infiniteTime)
        {
            return "∞"; // Símbolo de infinito
        }
        else if (hasReminder)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        else
        {
            return "Sin límite";
        }
    }
}