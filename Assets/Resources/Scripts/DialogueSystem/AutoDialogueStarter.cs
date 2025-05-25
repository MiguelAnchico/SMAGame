using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDialogueStarter : MonoBehaviour
{
    [Header("Configuración")]
    public PlayerInteract playerInteract;
    public float delayBeforeStart = 0.5f;

    void Start()
    {
        StartCoroutine(StartDialogueAfterDelay());
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeStart);
        
        if (playerInteract != null)
        {
            // Simular la primera interacción automáticamente
            playerInteract.TryInteract();
            Debug.Log("TryInteract ejecutado automáticamente");
        }
        else
        {
            Debug.LogWarning("PlayerInteract no asignado en AutoDialogueStarter");
        }
    }
}