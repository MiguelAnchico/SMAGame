using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueUI : MonoBehaviour
{
    public GameObject panel; // Panel UI diálogo
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image speakerImage;  // Imagen del personaje que habla

    private DialogueEntry[] currentDialogue;
    private int currentIndex = 0;
    private bool dialogueActive = false;

    public UnityEvent OnDialogueEnded;

    void Update()
    {
        if (dialogueActive && Input.GetMouseButtonDown(0)) // click o toque para avanzar
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(DialogueEntry[] lines)
    {
        currentDialogue = lines;
        currentIndex = 0;
        dialogueActive = true;
        panel.SetActive(true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (currentIndex < currentDialogue.Length)
        {
            nameText.text = currentDialogue[currentIndex].speakerName;
            dialogueText.text = currentDialogue[currentIndex].line;

            if (speakerImage != null && currentDialogue[currentIndex].speakerImage != null)
            {
                speakerImage.sprite = currentDialogue[currentIndex].speakerImage;
                speakerImage.color = Color.white;  // Asegúrate que no esté transparente
            }

            currentIndex++;
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        panel.SetActive(false);
        dialogueActive = false;
        OnDialogueEnded?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string speakerName;
    public string line;
    public Sprite speakerImage;
}
