using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
  public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private string[] lines;
    private int currentLine;

    private bool dialogueActive = false;

    public void StartDialogue(string[] dialogueLines)
    {
        dialoguePanel.SetActive(true);
        lines = dialogueLines;
        currentLine = 0;
        dialogueActive = true;
        ShowLine();
    }

    public void ShowNextLine()
    {
        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    public void ShowLine()
    {
        dialogueText.text = lines[currentLine];
        currentLine++;
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueActive = false;
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

}
