using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskTrigger : MonoBehaviour
{
    public int taskIdToComplete = 1;
    public TaskUIManager uiManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            uiManager.UpdateTaskList(); // Refresca los chulitos
            gameObject.SetActive(false); // Opcional
        }
    }
}