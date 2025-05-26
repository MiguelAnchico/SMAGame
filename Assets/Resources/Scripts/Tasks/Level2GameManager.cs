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
            Debug.Log("[Level2GameManager] Iniciando di�logo inicial...");
            taskManagerLevel2.dialogueUI.StartDialogue(taskManagerLevel2.dialogoInicial);

            // Esperar a que el di�logo est� activo
            while (!taskManagerLevel2.dialogueUI.IsDialogueActive())
            {
                yield return null;
            }

            Debug.Log("[Level2GameManager] Di�logo activo, esperando que finalice...");

            // Esperar hasta que termine el di�logo
            while (taskManagerLevel2.dialogueUI.IsDialogueActive())
            {
                yield return null;
            }

            Debug.Log("[Level2GameManager] Di�logo inicial terminado. Iniciando misi�n 1...");
            taskManagerLevel2.StartMission(1);
        }
        else
        {
            Debug.LogWarning("[Level2GameManager] taskManagerLevel2 no est� referenciado");
        }
    }
}


