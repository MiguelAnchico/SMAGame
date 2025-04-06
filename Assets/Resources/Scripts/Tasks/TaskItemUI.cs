using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TaskItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image checkmarkIcon;

    private Task task;

    public void Setup(Task task)
    {
        this.task = task;
        UpdateUI();
    }

    public void UpdateUI()
    {
        titleText.text = task.title;
        descriptionText.text = task.description;
        checkmarkIcon.gameObject.SetActive(task.isCompleted);
    }
}