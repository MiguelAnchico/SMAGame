using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 100;
    

    
    [Header("Trigger Settings")]
    public bool destroyAfterUse = false;
    public bool oneTimeUse = true;
    
    private bool hasBeenUsed = false;

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
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            // Si es de un solo uso y ya se usó, no hacer nada
            if (oneTimeUse && hasBeenUsed)
                return;

            // Buscar el componente de salud del jugador
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // Aplicar daño (el PlayerHealth se encarga de sonidos y efectos)
                playerHealth.TakeDamage(damageAmount);
                
                // Marcar como usado
                hasBeenUsed = true;
                
                // Destruir el objeto si está configurado para ello
                if (destroyAfterUse)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // Método público para resetear el trigger (útil si quieres reutilizarlo)
    public void ResetTrigger()
    {
        hasBeenUsed = false;
    }
}