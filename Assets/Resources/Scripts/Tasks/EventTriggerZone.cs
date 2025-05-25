using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerZone : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string targetTag = "Player"; // Tag del objeto que activará los eventos
    public bool oneTimeUse = true; // Si solo se puede activar una vez
    public bool destroyAfterUse = false; // Si se destruye después de activarse
    
    [Header("Events")]
    [Space]
    public UnityEvent OnTriggerEnterEvents; // Eventos cuando entra
    public UnityEvent OnTriggerExitEvents;  // Eventos cuando sale
    public UnityEvent OnTriggerStayEvents;  // Eventos mientras permanece dentro
    
    [Header("Stay Events Settings")]
    public bool enableStayEvents = false; // Activar eventos de permanencia
    public float stayEventInterval = 1f; // Intervalo para eventos de permanencia
    
    private bool hasBeenTriggered = false;
    private bool isObjectInside = false;
    private Coroutine stayEventsCoroutine;

    void Start()
    {
        // Asegurar que el collider esté configurado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el objeto correcto
        if (other.CompareTag(targetTag))
        {
            // Si es de un solo uso y ya se usó, no hacer nada
            if (oneTimeUse && hasBeenTriggered)
                return;

            Debug.Log($"{other.name} entró en la zona de eventos");
            
            isObjectInside = true;
            hasBeenTriggered = true;
            
            // Ejecutar eventos de entrada
            OnTriggerEnterEvents?.Invoke();
            
            // Iniciar eventos de permanencia si están habilitados
            if (enableStayEvents && stayEventsCoroutine == null)
            {
                stayEventsCoroutine = StartCoroutine(StayEventsCoroutine());
            }
            
            // Destruir el objeto si está configurado para ello
            if (destroyAfterUse)
            {
                Destroy(gameObject, 0.1f); // Small delay to ensure events execute
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(targetTag) && isObjectInside)
        {
            Debug.Log($"{other.name} salió de la zona de eventos");
            
            isObjectInside = false;
            
            // Ejecutar eventos de salida
            OnTriggerExitEvents?.Invoke();
            
            // Detener eventos de permanencia
            if (stayEventsCoroutine != null)
            {
                StopCoroutine(stayEventsCoroutine);
                stayEventsCoroutine = null;
            }
        }
    }

    private IEnumerator StayEventsCoroutine()
    {
        while (isObjectInside)
        {
            yield return new WaitForSeconds(stayEventInterval);
            
            if (isObjectInside) // Verificar nuevamente por si salió durante la espera
            {
                OnTriggerStayEvents?.Invoke();
            }
        }
        
        stayEventsCoroutine = null;
    }

    // Métodos públicos para control externo
    public void ResetTrigger()
    {
        hasBeenTriggered = false;
        isObjectInside = false;
        
        if (stayEventsCoroutine != null)
        {
            StopCoroutine(stayEventsCoroutine);
            stayEventsCoroutine = null;
        }
    }

    public void ForceActivate()
    {
        Debug.Log("Zona de eventos activada manualmente");
        OnTriggerEnterEvents?.Invoke();
        hasBeenTriggered = true;
    }

    public bool HasBeenTriggered()
    {
        return hasBeenTriggered;
    }

    public bool IsObjectInside()
    {
        return isObjectInside;
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = hasBeenTriggered ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}