using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogueTrigger : MonoBehaviour
{
   public GameObject promptUI; // Texto de "Presiona A"
    public DialogueManager dialogueManager;
    public string[] dialogueLines;

    private bool isPlayerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            promptUI.SetActive(false);
            dialogueManager.EndDialogue();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isPlayerInRange)
        {
            if (!dialogueManager.IsDialogueActive())
            {
                dialogueManager.StartDialogue(dialogueLines);
            }
            else
            {
                dialogueManager.ShowNextLine();
            }
        }
    }
}
