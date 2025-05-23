using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
     public GameObject dialoguePanel;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerImage;

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

        interaccionJugador.interactionPanel.SetActive(false);
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
    }
}
