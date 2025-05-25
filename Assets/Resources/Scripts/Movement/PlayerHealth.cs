using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("Invulnerability")]
    public float invulnerabilityTime = 2.0f;
    private bool isInvulnerable = false;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    
    [Header("Effects")]
    public GameObject deathEffect;

    
    // Eventos
    public delegate void PlayerDeathHandler();
    public static event PlayerDeathHandler OnPlayerDeath;
    
    public delegate void PlayerHealthChangedHandler(int currentHealth, int maxHealth);
    public static event PlayerHealthChangedHandler OnPlayerHealthChanged;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerMove3D playerMovement;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMove3D>();
        
        // Notificar la salud inicial
        if (OnPlayerHealthChanged != null)
        {
            OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
            return;
            
        currentHealth -= damage;
        
        // Asegurar que la salud no sea negativa
        currentHealth = Mathf.Max(currentHealth, 0);
        
        // Reproducir sonido de golpe si existe
        if (hitSound != null && ControladorSonido.Instance != null)
        {
            ControladorSonido.Instance.EjecutarSonido(hitSound);
        }
        
        // Efecto visual de daño
        StartCoroutine(FlashDamage());

        // Notificar cambio de salud
        if (OnPlayerHealthChanged != null)
        {
            OnPlayerHealthChanged(currentHealth, maxHealth);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(TemporaryDisableControls(0.1f));
            // Periodo de invulnerabilidad después de recibir daño
            StartCoroutine(InvulnerabilityPeriod());
        }
    }
    
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // Notificar cambio de salud
        if (OnPlayerHealthChanged != null)
        {
            OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }
    
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        
        // Notificar cambio de salud
        if (OnPlayerHealthChanged != null)
        {
            OnPlayerHealthChanged(currentHealth, maxHealth);
        }
    }
    
    private IEnumerator FlashDamage()
    {
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
        yield return new WaitForSeconds(0.2f);
        isInvulnerable = true;
        
        // Efecto visual de parpadeo durante invulnerabilidad
        StartCoroutine(BlinkEffect());
        
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }
    
    private IEnumerator BlinkEffect()
    {
        if (spriteRenderer != null)
        {
            float blinkDuration = invulnerabilityTime;
            float blinkInterval = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < blinkDuration)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                yield return new WaitForSeconds(blinkInterval);
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval * 2;
            }
            
            // Asegurar que el sprite vuelva a ser visible
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
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
        
        // Notificar la muerte del jugador
        if (OnPlayerDeath != null)
        {
            OnPlayerDeath();
        }
        
        // Activar animación de muerte
        if (animator != null)
        {
            animator.SetBool("die", true);
        }
        
        // Desactivar controles del jugador
        DisablePlayerControls();
    }

    private IEnumerator TemporaryDisableControls(float duration)
    {
        // Desactivar movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        yield return new WaitForSeconds(duration);
        
        // Reactivar movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
    
    private void DisablePlayerControls()
    {
        // Desactivar movimiento del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Detener físicas
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
    

    
    // Métodos públicos para acceder a la información de salud
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
    
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}