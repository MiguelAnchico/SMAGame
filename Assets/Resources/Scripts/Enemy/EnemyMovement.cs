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

    [Header("Comportamiento")]
    public bool oneWayMovement = false; // Nuevo booleano para ir de A a B y quedarse quieto

    [Header("Referencias")]
    private Animator animator; // Referencia al componente Animator
    private SpriteRenderer spriteRenderer; // Referencia al componente SpriteRenderer
    
    [Header("Stun System")]
    private bool isStunned = false;
    private float stunEndTime = 0f;
    
    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private bool isChasing = false;
    private bool hasReachedFinalDestination = false; // Para saber si ya llegó al punto B en modo one-way
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

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log("Jugador encontrado automáticamente");
            }
            else
            {
                Debug.LogWarning("No se encontró ningún objeto con tag 'Player'");
            }
        }
    }

    void Update()
    {
        // Verificar si el stun ha terminado
        if (isStunned && Time.time >= stunEndTime)
        {
            isStunned = false;
        }
        
        // Si está stunned, no hacer movimiento
        if (isStunned)
        {
            // Actualizar animaciones para mostrar que está quieto
            UpdateAnimationAndDirection();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Solo detectar al jugador si no está en modo one-way movement
        if (!oneWayMovement && distanceToPlayer < detectionRange)
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
            if (oneWayMovement)
            {
                OneWayMovement();
            }
            else
            {
                Patrol();
            }
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

    void OneWayMovement()
    {
        // Si ya llegó al destino final, no hacer nada
        if (hasReachedFinalDestination)
        {
            return;
        }

        Vector3 oldPosition = transform.position;

        Vector3 newPosition = Vector2.MoveTowards(transform.position, pointB.position, patrolSpeed * Time.deltaTime);
        newPosition.z = transform.position.z;
        transform.position = newPosition;

        // Calcular dirección de movimiento
        if (transform.position != oldPosition)
        {
            lastMoveDirection = (transform.position - oldPosition).normalized;
        }

        // Verificar si llegó al punto B
        if (Vector2.Distance(transform.position, pointB.position) < 0.1f)
        {
            hasReachedFinalDestination = true;
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
        bool isMoving = false;
        
        // Si está stunned, no está en movimiento
        if (isStunned)
        {
            isMoving = false;
        }
        else if (oneWayMovement)
        {
            // En modo one-way, está en movimiento solo si no ha llegado al destino final
            isMoving = !hasReachedFinalDestination;
        }
        else
        {
            // En modo normal, está en movimiento si no está cerca del objetivo
            isMoving = (Vector2.Distance(transform.position, 
                       isChasing ? player.position : currentTarget) > 0.1f);
        }
        
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

    // Métodos públicos para el sistema de stun
    public void StunEnemy(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    // Método público para resetear el estado one-way (útil para reutilizar el enemigo)
    public void ResetOneWayMovement()
    {
        hasReachedFinalDestination = false;
        currentTarget = pointB.position;
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