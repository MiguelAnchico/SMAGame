using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 100;
    
    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;

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
                // Verificar si el jugador tiene invulnerabilidad activa
                if (playerHealth.IsInvulnerable())
                    return;

                // Aplicar daño (el PlayerHealth se encarga de sonidos y efectos)
                playerHealth.TakeDamage(damageAmount);
                
                // Aplicar empuje
                ApplyKnockback(other);
                
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
    
    private void ApplyKnockback(Collider2D player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        
        if (playerRb != null)
        {
            // Calcular dirección simple: si jugador está a la derecha, empujar a la derecha
            Vector3 direction = (player.transform.position - transform.position).normalized;
            Vector3 force = direction * knockbackForce;
            playerRb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    
    // Método público para resetear el trigger (útil si quieres reutilizarlo)
    public void ResetTrigger()
    {
        hasBeenUsed = false;
    }
}