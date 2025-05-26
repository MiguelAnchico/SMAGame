using System.Collections;
using UnityEngine;

public class EnemyHealthLevel2 : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth;

    public float invulnerabilityTime = 0.5f;
    private bool isInvulnerable = false;

    public GameObject deathEffect;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    void OnEnable()
    {
        Debug.Log($"{gameObject.name} suscrito a OnAttackHit");
        PlayerCombat.OnAttackHit += HandleAttackHit;
    }

    void OnDisable()
    {
        PlayerCombat.OnAttackHit -= HandleAttackHit;
    }

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    private void HandleAttackHit(GameObject target, float damage)
    {
        if (target == gameObject)
        {
            Debug.Log($"Enemigo {gameObject.name} recibió daño: {damage}");
            TakeDamage((int)damage);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[EnemyHealthLevel2] {gameObject.name} está recibiendo daño: {damage}");
        if (isInvulnerable)
            return;

        currentHealth -= damage;

        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityPeriod());
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
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (TaskManagerLevel2.Instance != null)
        {
            TaskManagerLevel2.Instance.EnemyDefeated();
        }

        Destroy(gameObject, 0.5f);
    }
}

