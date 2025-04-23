using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
 public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 5f;
    public Transform player;

    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private bool isChasing = false;

    void Start()
    {
        initialPosition = transform.position;
        currentTarget = pointB.position;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            isChasing = true;
        }
        else if (isChasing && distanceToPlayer >= detectionRange + 1f) // rango de histeresis para evitar parpadeo
        {
            isChasing = false;
            currentTarget = Vector2.Distance(transform.position, pointA.position) < Vector2.Distance(transform.position, pointB.position) ? pointB.position : pointA.position;
        }

        if (isChasing)
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
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = currentTarget == pointA.position ? pointB.position : pointA.position;
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, detectionRange);
}
}
