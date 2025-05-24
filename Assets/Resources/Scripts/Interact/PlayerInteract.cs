using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerInteract : MonoBehaviour
{     
    private bool canInteract = true;
    private bool isNearNPC = false;
    private NPC currentNPC;

    public GameObject interactionPanel;
    public GameObject dialoguePanel;
    public DialogueManager dialogueManager;

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact action triggered");
        if (context.performed && canInteract && isNearNPC)
        {
            var control = context.control;

            if (control.path.EndsWith("/buttonEast"))
            {
                TryInteract();
            }
        }
    }

    private void TryInteract()
    {
        if (dialogueManager != null && dialoguePanel.activeSelf)
        {
            dialogueManager.ShowNextLine();
        }
        else if (currentNPC != null)
        {
            dialogueManager.StartDialogue(currentNPC.npcDialogue);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            currentNPC = other.GetComponent<NPC>();
            if (currentNPC != null)
            {
                isNearNPC = true;
                interactionPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            currentNPC = null;
            interactionPanel.SetActive(false);
        }
    }
}
