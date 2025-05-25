using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Timer Display")]
    public string timerTextName = "TimerText"; // Nombre del GameObject con TextMeshPro
    private TextMeshProUGUI timerText;
    private bool timerTextFound = false;

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
        timerTextName = "TimerText";
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
            
            // Actualizar el texto del timer si es la tarea actual
        }
            UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        // Solo buscar el texto si no se ha encontrado antes
        if (!timerTextFound)
        {
            FindTimerText();
        }

        // Actualizar el texto si se encontró y esta es la tarea actual
        if (timerText != null && IsCurrentTask())
        {
            timerText.text = GetTimeRemainingText();
        }
    }

    private void FindTimerText()
    {
        // Buscar el GameObject por nombre
        GameObject timerObject = GameObject.Find(timerTextName);
        
        if (timerObject != null)
        {
            timerText = timerObject.GetComponent<TextMeshProUGUI>();
            
            if (timerText != null)
            {
                timerTextFound = true;
                Debug.Log($"Timer text encontrado: {timerTextName}");
            }
        }
    }

    private bool IsCurrentTask()
    {
        // Verificar si esta es la tarea actual del TaskManager
        if (TaskManager.Instance != null)
        {
            Task currentTask = TaskManager.Instance.GetCurrentTask();
            return currentTask != null && currentTask.id == this.id;
        }
        return false;
    }

    public void MarkAsFailed()
    {
        isFailed = true;
        timeRemaining = 0f;
        
        // Actualizar display al fallar
        UpdateTimerDisplay();
    }

    public void SetInfiniteTime(bool infinite)
    {
        infiniteTime = infinite;
        if (infinite)
        {
            timeRemaining = float.MaxValue; // Establecer tiempo muy alto
        }
        
        // Actualizar display
        UpdateTimerDisplay();
    }

    public void SetAudioClip(AudioClip audio)
    {
        audioClip = audio;
    }

    public void SetTimerTextName(string textName)
    {
        timerTextName = textName;
        timerTextFound = false; // Resetear para buscar el nuevo texto
        timerText = null;
    }

    public string GetTimeRemainingText()
    {
        if (isFailed)
        {
            return "x";
        }
        else if (isCompleted)
        {
            return "∞";
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
            return "∞";
        }
    }

    // Método público para forzar actualización del display
    public void ForceUpdateDisplay()
    {
        UpdateTimerDisplay();
    }

    public void OnBecomeCurrentTask()
    {
        timerTextFound = false;
        timerText = null;
        UpdateTimerDisplay();
    }
}