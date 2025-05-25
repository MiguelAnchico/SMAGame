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
    
    [Header("Continuous Attack Settings")]
    public float attackInterval = 1f;
    
    private bool hasBeenUsed = false;
    private bool playerInside = false;
    private PlayerHealth currentPlayerHealth;
    private Coroutine continuousAttackCoroutine;

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

            playerInside = true;
            currentPlayerHealth = other.GetComponent<PlayerHealth>();
            
            if (currentPlayerHealth != null)
            {
                // Iniciar ataque continuo
                continuousAttackCoroutine = StartCoroutine(ContinuousAttack(other));
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            currentPlayerHealth = null;
            
            // Detener ataque continuo
            if (continuousAttackCoroutine != null)
            {
                StopCoroutine(continuousAttackCoroutine);
                continuousAttackCoroutine = null;
            }
        }
    }

    private IEnumerator ContinuousAttack(Collider2D player)
    {
        while (playerInside && currentPlayerHealth != null)
        {
            // Verificar si el jugador tiene invulnerabilidad activa
            if (!currentPlayerHealth.IsInvulnerable())
            {
                // Aplicar daño
                currentPlayerHealth.TakeDamage(damageAmount);
                
                // Aplicar empuje
                ApplyKnockback(player);
                
                // Marcar como usado si es de un solo uso
                if (oneTimeUse)
                {
                    hasBeenUsed = true;
                    
                    // Destruir el objeto si está configurado para ello
                    if (destroyAfterUse)
                    {
                        Destroy(gameObject);
                        yield break;
                    }
                }
            }
            
            // Esperar el intervalo antes del siguiente ataque
            yield return new WaitForSeconds(attackInterval);
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