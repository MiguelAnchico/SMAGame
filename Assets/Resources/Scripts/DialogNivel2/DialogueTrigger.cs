using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueUI dialogueUI; // referencia al UI de diálogo
    public DialogueEntry[] dialogueLines; // las líneas de diálogo a mostrar

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

