using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task
{
    public int id;
    public string title;
    public string description;
    public bool isCompleted;
    public float timeRemaining;

    public Task(int id, string title, string description = "", float time = -1f)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.isCompleted = false;
        this.timeRemaining = time;
    }

    public bool HasReminder() => timeRemaining > 0;

    public void UpdateTimer(float deltaTime)
    {
        if (HasReminder() && !isCompleted)
            timeRemaining -= deltaTime;
    }
}