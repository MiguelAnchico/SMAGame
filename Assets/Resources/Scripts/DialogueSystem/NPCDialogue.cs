using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public string npcName;  // Nombre del NPC
    public List<string> dialogueLines;  // Lista de líneas de diálogo

    public NPCDialogue(string name, List<string> lines)
    {
        npcName = name;
        dialogueLines = lines;
    }
}
