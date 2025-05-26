using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueUI dialogueUI; // referencia al UI de di�logo
    public DialogueEntry[] dialogueLines; // las l�neas de di�logo a mostrar

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return; // evitar disparar varias veces
        if (other.CompareTag("Player"))
        {
            triggered = true;
            dialogueUI.StartDialogue(dialogueLines);
        }
    }
}

