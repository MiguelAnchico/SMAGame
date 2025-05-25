using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class DialogueSystem
{
    public GameObject interactionPanel;
    public GameObject dialoguePanel;
    public DialogueManager dialogueManager;
}

[System.Serializable]
public class NPCDialogueMapping
{
    public NPC npc;
    public DialogueSystem dialogueSystem;
}

public class PlayerInteract : MonoBehaviour
{
    private bool canInteract = true;
    private bool isNearNPC = false;
    private NPC currentNPC;
    private DialogueSystem currentDialogueSystem;

    [Header("Mapeo NPC - Sistema de Diálogo")]
    public List<NPCDialogueMapping> npcMappings = new List<NPCDialogueMapping>();

    private Rigidbody2D playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void BlockPlayerMovement()
    {
        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = 10000f;
            playerRigidbody.velocity = Vector2.zero;
        }
    }

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

    public void TryInteract()
    {
        if (currentDialogueSystem != null)
        {
            if (currentDialogueSystem.dialogueManager != null && currentDialogueSystem.dialoguePanel.activeSelf)
            {
                currentDialogueSystem.dialogueManager.ShowNextLine();
            }
            else if (currentNPC != null)
            {
                Debug.Log("Interacting with NPC: " + currentNPC.name);
                BlockPlayerMovement();
                currentDialogueSystem.dialogueManager.StartDialogue(currentNPC.npcDialogue);
            }
        }
    }

    private DialogueSystem GetDialogueSystemForNPC(NPC npc)
    {
        foreach (var mapping in npcMappings)
        {
            if (mapping.npc == npc)
            {
                return mapping.dialogueSystem;
            }
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            currentNPC = other.GetComponent<NPC>();
            if (currentNPC != null)
            {
                isNearNPC = true;
                currentDialogueSystem = GetDialogueSystemForNPC(currentNPC);
                
                if (currentDialogueSystem != null && currentDialogueSystem.interactionPanel != null)
                {
                    currentDialogueSystem.interactionPanel.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            if (currentDialogueSystem != null && currentDialogueSystem.interactionPanel != null)
            {
                currentDialogueSystem.interactionPanel.SetActive(false);
            }
            
            isNearNPC = false;
            currentNPC = null;
            currentDialogueSystem = null;
        }
    }
}