using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;
    
    [Header("Panel de Interacción Asociado")]
    public GameObject associatedInteractionPanel;
    
    public UnityEvent OnDialogueEnd;

    private Queue<DialogueLine> dialogueQueue;
    private PlayerInteract interaccionJugador;

    private void Start()
    {
        dialogueQueue = new Queue<DialogueLine>();
        interaccionJugador = FindObjectOfType<PlayerInteract>();
    }

    public void StartDialogue(NPCDialogue dialogue)
    {
        dialogueQueue.Clear();

        foreach (var line in dialogue.dialogueLines)
        {
            dialogueQueue.Enqueue(line);
        }

        // Ocultar el panel de interacción asociado a este DialogueManager
        if (associatedInteractionPanel != null)
        {
            associatedInteractionPanel.SetActive(false);
        }
        
        dialoguePanel.SetActive(true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (dialogueQueue.Count > 0)
        {
            DialogueLine currentLine = dialogueQueue.Dequeue();
            npcNameText.text = currentLine.speakerName;
            dialogueText.text = currentLine.line;
            speakerImage.sprite = currentLine.speakerImage;
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        FindObjectOfType<PlayerInteract>().GetComponent<Rigidbody2D>().gravityScale = 9.71f;
        OnDialogueEnd?.Invoke();
    }

    // Método público para verificar si el diálogo está activo
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeSelf;
    }

    // Método público para verificar si hay más líneas
    public bool HasMoreLines()
    {
        return dialogueQueue.Count > 0;
    }
}