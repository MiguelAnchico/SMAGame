using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Patrullaje")]
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float patrolPointReachThreshold = 0.5f;

    [Header("Persecución")]
    public float chaseSpeed = 3.5f;
    public float knockbackDistance = 1f;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private Transform player;
    private bool isChasing = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        currentTarget = pointB; // empieza patrullando hacia B
    }

    void Update()
    {
        if (isChasing && player != null)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * patrolSpeed * Time.deltaTime);

        // Girar sprite según dirección horizontal
        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < patrolPointReachThreshold)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * chaseSpeed * Time.deltaTime);

        if (direction.x != 0)
            spriteRenderer.flipX = direction.x < 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isChasing = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            player = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Vector2 pushBack = (transform.position - collision.transform.position).normalized * knockbackDistance;
            rb.MovePosition(rb.position + pushBack);
        }
    }
}
