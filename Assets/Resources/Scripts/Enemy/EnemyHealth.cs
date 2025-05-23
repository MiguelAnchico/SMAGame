using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth;
    
    public float invulnerabilityTime = 0.5f;
    private bool isInvulnerable = false;
    
    // Referencias opcionales
    public GameObject deathEffect;
    public AudioClip hitSound;
    public AudioClip deathSound;
    
    // Evento que se dispara cuando el enemigo muere
    public delegate void EnemyDeathHandler(GameObject enemy);
    public static event EnemyDeathHandler OnEnemyDeath;

    private Animator animator;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
            return;
            
        currentHealth -= damage;
        
        // Reproducir sonido de golpe si existe
        if (hitSound != null && ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.EjecutarSonido(hitSound);
        }
        
        // Efecto visual de daño
        StartCoroutine(FlashDamage());
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Periodo breve de invulnerabilidad
            StartCoroutine(InvulnerabilityPeriod());
        }
    }
    
    private IEnumerator FlashDamage()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    private IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }
    
    private void Die()
    {
        // Reproducir sonido de muerte si existe
        if (deathSound != null && ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.EjecutarSonido(deathSound);
        }
        
        // Instanciar efecto de muerte si existe
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Notificar a otros objetos de la muerte del enemigo
        if (OnEnemyDeath != null)
        {
            OnEnemyDeath(gameObject);
        }
        
        // Activar animación de muerte
        if (animator != null)
        {
            animator.SetBool("die", true);
        }
        
        // Desactivar físicas y colisiones
        DisablePhysicsAndCollisions();
        
        // Destruir el objeto después de 0.5 segundos
        Destroy(gameObject, 0.5f);
    }

    private void DisablePhysicsAndCollisions()
    {
        // Desactivar Rigidbody2D (gravedad y físicas)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Desactivar todos los Collider2D
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
        
        // También desactivar colliders en hijos si los hay
        Collider2D[] childColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in childColliders)
        {
            col.enabled = false;
        }
        
        // Desactivar el script de movimiento para que no interfiera
        EnemyMovement movementScript = GetComponent<EnemyMovement>();
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
    }
}