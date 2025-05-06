using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{    public GameObject dialoguePanel;  // Panel donde se muestra el diálogo
    public TextMeshProUGUI npcNameText;  // Texto donde se muestra el nombre del NPC
    public TextMeshProUGUI dialogueText;  // Texto donde se muestra la línea de diálogo

    private Queue<string> dialogueQueue;  // Cola de diálogos a mostrar
    private NPCDialogue currentDialogue;  // El diálogo actual del NPC
    private int currentLineIndex = 0;  // Índice de la línea actual

    private PlayerInteract interaccionJugador;

    private void Start()
    {
        dialogueQueue = new Queue<string>();

         interaccionJugador = FindObjectOfType<PlayerInteract>();
    }

    // Inicia el diálogo con un NPC
    public void StartDialogue(NPCDialogue dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueQueue.Clear();

        foreach (var line in dialogue.dialogueLines)
        {
            dialogueQueue.Enqueue(line);  // Añade las líneas del NPC a la cola
        }

         interaccionJugador.interactionPanel.SetActive(false); 
        npcNameText.text = dialogue.npcName;  // Muestra el nombre del NPC
        dialoguePanel.SetActive(true);  // Activa el panel de diálogo
        ShowNextLine();  // Muestra la primera línea de diálogo
    }

    // Muestra la siguiente línea del diálogo
    public void ShowNextLine()
    {
        if (dialogueQueue.Count > 0)
        {
            dialogueText.text = dialogueQueue.Dequeue();  // Extrae y muestra la siguiente línea
        }
        else
        {
            EndDialogue();  // Si ya no hay más líneas, termina el diálogo
        }
    }

    // Termina el diálogo
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);  // Desactiva el panel de diálogo
    }
}
