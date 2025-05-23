using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Puntos de Patrulla")]
    public Transform pointA;
    public Transform pointB;

    [Header("Velocidades")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Detección")]
    public float detectionRange = 5f;
    public Transform player;

    [Header("Referencias")]
    private Animator animator; // Referencia al componente Animator
    private SpriteRenderer spriteRenderer; // Referencia al componente SpriteRenderer
    
    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private bool isChasing = false;
    private Vector2 lastMoveDirection; // Dirección del último movimiento

    // Nombres de parámetros del Animator
    private const string IS_MOVING = "isMoving";
    private const string SPEED = "speed"; // Opcional, para controlar la velocidad de animación

    void Start()
    {
        initialPosition = transform.position;
        currentTarget = pointB.position;
        
        // Obtener referencias a componentes
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Inicializar la dirección de movimiento
        lastMoveDirection = Vector2.right;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Determinar si debe perseguir al jugador
        if (distanceToPlayer < detectionRange)
        {
            isChasing = true;
        }
        else if (isChasing && distanceToPlayer >= detectionRange + 1f) // rango de histéresis para evitar parpadeo
        {
            isChasing = false;
            currentTarget = Vector2.Distance(transform.position, pointA.position) < 
                           Vector2.Distance(transform.position, pointB.position) ? 
                           pointB.position : pointA.position;
        }

        // Realizar movimiento según estado
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
        
        // Actualizar animaciones
        UpdateAnimationAndDirection();
    }

    void Patrol()
    {
        Vector3 oldPosition = transform.position;

        Vector3 newPosition = Vector2.MoveTowards(transform.position, currentTarget, patrolSpeed * Time.deltaTime);
        newPosition.z = transform.position.z;
        transform.position = newPosition;

        // Calcular dirección de movimiento
        if (transform.position != oldPosition)
        {
            lastMoveDirection = (transform.position - oldPosition).normalized;
        }

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = currentTarget == pointA.position ? pointB.position : pointA.position;
        }
    }

    void ChasePlayer()
    {
        Vector3 oldPosition = transform.position;

        Vector3 newPosition = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
        newPosition.z = transform.position.z; // Mantener Z explícitamente
        transform.position = newPosition;

        // Calcular dirección de movimiento
        if (transform.position != oldPosition)
        {
            lastMoveDirection = (transform.position - oldPosition).normalized;
        }
    }
    
    void UpdateAnimationAndDirection()
    {
        // Determinar si está en movimiento
        bool isMoving = (Vector2.Distance(transform.position, 
                         isChasing ? player.position : currentTarget) > 0.1f);
        
        // Actualizar el parámetro isMoving del Animator
        if (animator != null)
        {
            animator.SetBool(IS_MOVING, isMoving);
            
            // Opcional: actualizar la velocidad de animación basada en si está persiguiendo o patrullando
            float currentSpeed = isChasing ? chaseSpeed / patrolSpeed : 1.0f;
            animator.SetFloat(SPEED, currentSpeed);
        }
        
        // Voltear el sprite basado en la dirección del movimiento
        if (spriteRenderer != null && lastMoveDirection != Vector2.zero)
        {
            // Si el movimiento es hacia la izquierda, voltear el sprite
            // Si el movimiento es hacia la derecha, no voltear el sprite
            spriteRenderer.flipX = (lastMoveDirection.x > 0);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Visualizar los puntos de patrulla
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
        }
    }
}