using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private Vector2 direction;
    private float speed;

    public int damage = 10;
    public float lifetime = 5f;

    private bool canCollide = false;

    private Collider2D interactionTriggerCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();

        // Ignorar colisión con InteractionTrigger del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform interactionTrigger = player.transform.Find("InteractionTrigger");
            if (interactionTrigger != null)
            {
                interactionTriggerCollider = interactionTrigger.GetComponent<Collider2D>();
                if (interactionTriggerCollider != null && projectileCollider != null)
                {
                    Physics2D.IgnoreCollision(projectileCollider, interactionTriggerCollider);
                }
            }
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
        StartCoroutine(EnableCollisionAfterDelay(0.1f));
    }

    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;

        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        else
        {
            Debug.LogWarning("[Projectile] Rigidbody2D is missing!");
        }
    }

    private IEnumerator EnableCollisionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canCollide = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canCollide) return;

        if (collision.CompareTag("Player"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("DamageZone"))
        {
            // No destruir ni hacer nada
        }
        else if (!collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
