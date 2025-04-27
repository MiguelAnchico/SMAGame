using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerInteract : MonoBehaviour
{     
       private bool canInteract = true;
    private bool isNearNPC = false;
    private NPC currentNPC;  // El NPC con el que el jugador está interactuando

    public GameObject interactionPanel;  // Panel que muestra el mensaje de "Interactuar" con el NPC
    public GameObject dialoguePanel;  // Panel donde se muestra el diálogo con el NPC
    public DialogueManager dialogueManager;  // El DialogueManager que controla el flujo del diálogo
    public NPCDialogue npcDialogue;  // El diálogo del NPC actual

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && canInteract && isNearNPC)
        {
            var control = context.control;

            if (control.path.EndsWith("/buttonEast"))
            {
                Debug.Log("Botón Este presionado: Interactuar");
                TryInteract();
            }
            else
            {
                Debug.Log("Otro botón presionado en Interact: " + control.path);
            }
        }
    }

    private void TryInteract()
    {
        // Si el diálogo no ha terminado, mostramos la siguiente línea
        if (dialogueManager != null && dialoguePanel.activeSelf)
        {
            dialogueManager.ShowNextLine();
        }
        else
        {
            // Si el diálogo ha terminado, iniciamos un nuevo diálogo con el NPC actual
            if (currentNPC != null)
            {
                dialogueManager.StartDialogue(currentNPC.npcDialogue);  // Usamos el NPCDialogue del NPC actual
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))  // Asegúrate de que tus NPCs tengan el tag 'NPC'
        {
            currentNPC = other.GetComponent<NPC>();  // Obtenemos el NPC con el que el jugador está cerca
            if (currentNPC != null)
            {
                isNearNPC = true;
                interactionPanel.SetActive(true);  // Mostrar el panel de "Interactuar"
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            currentNPC = null;  // Limpiar la referencia al NPC
            interactionPanel.SetActive(false);  // Ocultar el panel de "Interactuar"
        }
    }
}
