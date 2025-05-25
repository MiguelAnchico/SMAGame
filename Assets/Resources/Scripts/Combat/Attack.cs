using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private float pushForce = 10f; // Fuerza con la que empujaremos al enemigo
    [SerializeField] private float pushDuration = 0.3f; // Duración del empuje
    [SerializeField] private float lifeTime = 0.05f; // Duración del ataque en segundos (50 milisegundos)
    [SerializeField] private int damageAmount = 20; // Cantidad de daño que inflige este ataque
    
    private Transform playerTransform; // Referencia a la transformación del jugador

    private void Start()
    {
        // Obtenemos la referencia al transform del objeto padre (jugador)
        playerTransform = transform.parent;
        
        // Iniciamos la autodestrucción del objeto de ataque
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si el objeto con el que colisionamos tiene el tag "Enemy"
        if (collision.CompareTag("EnemyTest"))
        {
            // Imprimimos en consola que atacamos al enemigo
            Debug.Log("¡Atacando a un enemigo!");
            
            // Calculamos la dirección desde el jugador hacia el enemigo
            Vector2 direction = (collision.transform.position - playerTransform.position).normalized;
            
            // Buscar y stunear el EnemyMovement si existe
            EnemyMovement enemyMovement = collision.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.StunEnemy(pushDuration); // Stunear durante el empuje
            }
            
            // Obtenemos el Rigidbody2D del enemigo para aplicar la fuerza
            Rigidbody2D enemyRigidbody = collision.GetComponent<Rigidbody2D>();
            
            // Si el enemigo tiene un Rigidbody2D, le aplicamos la fuerza física
            if (enemyRigidbody != null)
            {
                enemyRigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
            }
            else
            {
                // Si no tiene Rigidbody2D, usar empuje por Transform
                StartCoroutine(PushByTransform(collision.transform, direction));
            }
            
            // Buscamos el componente IDamageable en el enemigo para aplicar daño
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                // Aplicamos el daño al enemigo
                damageable.TakeDamage(damageAmount);
                Debug.Log($"Aplicando {damageAmount} de daño a {collision.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"El enemigo {collision.gameObject.name} no tiene un componente IDamageable");
            }
        }
    }

    private IEnumerator PushByTransform(Transform enemyTransform, Vector2 direction)
    {
        Vector3 startPosition = enemyTransform.position;
        Vector3 targetPosition = startPosition + (Vector3)(direction * pushForce * 0.1f); // Reducir distancia para Transform
        
        float elapsed = 0f;
        
        while (elapsed < pushDuration)
        {
            float progress = elapsed / pushDuration;
            float easedProgress = 1f - (progress - 1f) * (progress - 1f); // Easing out para suavizar
            
            enemyTransform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar posición final
        enemyTransform.position = targetPosition;
    }
}