using System.Collections;
using UnityEngine;

public class Level2GameManager : MonoBehaviour
{
    public TaskManagerLevel2 taskManagerLevel2;

    void Start()
    {
        StartCoroutine(LevelStartSequence());
    }

    private IEnumerator LevelStartSequence()
    {
        if (taskManagerLevel2 != null)
        {
            Debug.Log("[Level2GameManager] Iniciando diálogo inicial...");
            taskManagerLevel2.dialogueUI.StartDialogue(taskManagerLevel2.dialogoInicial);

            // Esperar a que el diálogo esté activo
            while (!taskManagerLevel2.dialogueUI.IsDialogueActive())
            {
                yield return null;
            }

            Debug.Log("[Level2GameManager] Diálogo activo, esperando que finalice...");

            // Esperar hasta que termine el diálogo
            while (taskManagerLevel2.dialogueUI.IsDialogueActive())
            {
                yield return null;
            }

            Debug.Log("[Level2GameManager] Diálogo inicial terminado. Iniciando misión 1...");
            taskManagerLevel2.StartMission(1);
        }
        else
        {
            Debug.LogWarning("[Level2GameManager] taskManagerLevel2 no está referenciado");
        }
    }
}


