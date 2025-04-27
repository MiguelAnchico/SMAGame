using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Niveles de Ataque")]
    [SerializeField] private int attackLevel = 1; // Nivel inicial de ataque
    [SerializeField] private int maxAttackLevel = 3; // Nivel máximo de ataque

    [Header("Configuración de Ataques")]
    [SerializeField] private float basicAttackDamage = 10f;
    [SerializeField] private float areaAttackDamage = 15f;
    [SerializeField] private float specialAttackDamage = 25f;

    [Header("Rangos de Ataque")]
    [SerializeField] private float basicAttackRange = 1.5f;
    [SerializeField] private float areaAttackRadius = 3f;
    [SerializeField] private float specialAttackRange = 5f;

    [Header("Tiempos de Enfriamiento")]
    [SerializeField] private float basicAttackCooldown = 0.5f;
    [SerializeField] private float areaAttackCooldown = 2f;
    [SerializeField] private float specialAttackCooldown = 5f;

    [Header("Efectos Visuales")]
    [SerializeField] private GameObject basicAttackEffect;
    [SerializeField] private GameObject areaAttackEffect;
    [SerializeField] private GameObject specialAttackEffect;

    [Header("Efecto de Cámara")]
    [SerializeField] private float cameraShakeIntensity = 0.5f;
    [SerializeField] private float cameraShakeDuration = 0.3f;

    [Header("Capas para detectar enemigos")]
    [SerializeField] private LayerMask enemyLayers;

    // Variables de control
    private float lastBasicAttackTime = -10f;
    private float lastAreaAttackTime = -10f;
    private float lastSpecialAttackTime = -10f;
    private bool isAttacking = false;

    // Referencias
    private Animator animator;
    private Transform attackPoint;
    private CameraShake cameraShake;

    // Eventos de ataque para coordinación con otros sistemas
    // Eventos para antes, durante y después del ataque
    public delegate void AttackEventHandler(int attackType);
    public event AttackEventHandler OnAttackStarted; // Al iniciar el ataque
    public event AttackEventHandler OnAttackPerformed; // Cuando el ataque conecta
    public event AttackEventHandler OnAttackFinished; // Al finalizar el ataque

    // Definir tipos de ataque como constantes
    public const int ATTACK_BASIC = 1;
    public const int ATTACK_AREA = 2;
    public const int ATTACK_SPECIAL = 3;

    // Evento para cuando un ataque impacta a un enemigo
    public delegate void AttackHitHandler(GameObject target, float damage);
    public static event AttackHitHandler OnAttackHit;


    [SerializeField] private AudioClip ataqueBasicoSonido;

    [SerializeField] private AudioClip ataqueAreaSonido;

    [SerializeField] private AudioClip ataqueEspecialSonido;

    private void Awake()
    {
        // Inicializar referencias
        animator = GetComponent<Animator>();
        
        // Buscar la referencia al sistema de temblor de cámara
        cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake == null && Camera.main != null)
        {
            cameraShake = Camera.main.gameObject.AddComponent<CameraShake>();
        }
        
        // Crear punto de ataque si no existe
        Transform existingAttackPoint = transform.Find("AttackPoint");
        if (existingAttackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.parent = transform;
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f); // Adelante del jugador
            attackPoint = attackPointObj.transform;
        }
        else
        {
            attackPoint = existingAttackPoint;
        }
    }

    // Este método será llamado por el Input System
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            // Obtén el control que activó esta acción
            var control = context.control;
            
            // Determina qué botón se presionó basándote en el path del control
            if (control.path.EndsWith("/buttonNorth"))
            {
                // Botón Norte - Ataque Básico
                Debug.Log("Botón Norte presionado: Ataque Básico");
                TryBasicAttack();
            }
            else if (control.path.EndsWith("/buttonSouth"))
            {
                // Botón Sur - Ataque de Área
                Debug.Log("Botón Sur presionado: Ataque de Área");
                TryAreaAttack();
            }
            else if (control.path.EndsWith("/buttonWest"))
            {
               // Botón Oeste - Ataque Especial
                Debug.Log("Botón Oeste presionado: Ataque Especial");
                TrySpecialAttack();
            }
            else
            {
                // Fallback - Ataque Básico
                Debug.Log("Otro botón presionado: " + control.path);
                TryBasicAttack();
            }
        }
    }

    // Intenta realizar un ataque básico si no está en enfriamiento
    private void TryBasicAttack()
    {
        if (Time.time >= lastBasicAttackTime + basicAttackCooldown)
        {
            StartCoroutine(PerformBasicAttack());
            lastBasicAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Ataque básico en enfriamiento. Tiempo restante: " + 
                      (basicAttackCooldown - (Time.time - lastBasicAttackTime)).ToString("F1") + "s");
        }
    }

    // Intenta realizar un ataque de área si no está en enfriamiento
    private void TryAreaAttack()
    {
        if (Time.time >= lastAreaAttackTime + areaAttackCooldown)
        {
            StartCoroutine(PerformAreaAttack());
            lastAreaAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Ataque de área en enfriamiento. Tiempo restante: " + 
                      (areaAttackCooldown - (Time.time - lastAreaAttackTime)).ToString("F1") + "s");
        }
    }

    // Intenta realizar un ataque especial si no está en enfriamiento
    private void TrySpecialAttack()
    {
        if (Time.time >= lastSpecialAttackTime + specialAttackCooldown)
        {
            StartCoroutine(PerformSpecialAttack());
            lastSpecialAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Ataque especial en enfriamiento. Tiempo restante: " + 
                      (specialAttackCooldown - (Time.time - lastSpecialAttackTime)).ToString("F1") + "s");
        }
    }

    // Realiza un ataque básico
    private IEnumerator PerformBasicAttack()
    {
        isAttacking = true;
        
        // Disparar evento de inicio de ataque
        OnAttackStarted?.Invoke(ATTACK_BASIC);
        
        // Reproducir animación si existe
        if (animator != null)
            animator.SetTrigger("BasicAttack");
        
            ControladorSonido.Instance.EjecutarSonido(ataqueBasicoSonido);

        // Esperar a que la animación llegue al frame de daño (ajustar tiempo)
        yield return new WaitForSeconds(0.2f);
        
        // Detectar enemigos en rango
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, 
            basicAttackRange, 
            enemyLayers
        );
        
        // Disparar evento de ataque realizado
        OnAttackPerformed?.Invoke(ATTACK_BASIC);

        
        // Notificar que se ha golpeado a los enemigos
        foreach (Collider2D enemy in hitEnemies)
        {
            // Enviar evento con el objetivo y el daño
            OnAttackHit?.Invoke(enemy.gameObject, basicAttackDamage);
            Debug.Log("Ataque básico conectado: " + enemy.name);
        }
        
        // Instanciar efecto visual
        if (basicAttackEffect != null)
        {
            Instantiate(basicAttackEffect, attackPoint.position, Quaternion.identity);
        }
        
        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.3f);
        
        // Disparar evento de fin de ataque
        OnAttackFinished?.Invoke(ATTACK_BASIC);
        
        isAttacking = false;
    }

    // Realiza un ataque de área
    private IEnumerator PerformAreaAttack()
    {
        isAttacking = true;
        
        // Disparar evento de inicio de ataque
        OnAttackStarted?.Invoke(ATTACK_AREA);
        
        // Reproducir animación si existe
        if (animator != null)
            animator.SetTrigger("AreaAttack");

            ControladorSonido.Instance.EjecutarSonido(ataqueAreaSonido);
        
        // Esperar a que la animación llegue al frame de daño
        yield return new WaitForSeconds(0.3f);
        
        // Detectar enemigos en un área más grande
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position, // Desde el centro del jugador
            areaAttackRadius, 
            enemyLayers
        );
        
        // Disparar evento de ataque realizado
        OnAttackPerformed?.Invoke(ATTACK_AREA);
        
        // Notificar que se ha golpeado a los enemigos
        foreach (Collider2D enemy in hitEnemies)
        {
            // Enviar evento con el objetivo y el daño
            OnAttackHit?.Invoke(enemy.gameObject, areaAttackDamage);
            Debug.Log("Ataque de área conectado: " + enemy.name);
        }
        
        // Instanciar efecto visual
        if (areaAttackEffect != null)
        {
            Instantiate(areaAttackEffect, transform.position, Quaternion.identity);
        }
        
        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.5f);
        
        // Disparar evento de fin de ataque
        OnAttackFinished?.Invoke(ATTACK_AREA);

        
        isAttacking = false;
    }

    // Realiza un ataque especial
    private IEnumerator PerformSpecialAttack()
    {
        isAttacking = true;
        
        // Disparar evento de inicio de ataque
        OnAttackStarted?.Invoke(ATTACK_SPECIAL);
        
        // Reproducir animación si existe
        if (animator != null)
            animator.SetTrigger("SpecialAttack");

            ControladorSonido.Instance.EjecutarSonido(ataqueEspecialSonido);
        
        // Esperar carga del ataque
        yield return new WaitForSeconds(0.5f);
        
        // Detectar enemigos en una línea recta o un área mayor
        Vector2 direction = new Vector2(transform.localScale.x, 0).normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position, 
            direction, 
            specialAttackRange, 
            enemyLayers
        );
        
        // Disparar evento de ataque realizado
        OnAttackPerformed?.Invoke(ATTACK_SPECIAL);
        
        // Activar el temblor de cámara
        if (cameraShake != null)
        {
            cameraShake.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);
        }
        
        // Notificar que se ha golpeado a los enemigos
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                // Enviar evento con el objetivo y el daño
                OnAttackHit?.Invoke(hit.collider.gameObject, specialAttackDamage);
                Debug.Log("Ataque especial conectado: " + hit.collider.name);
            }
        }
        
        // Instanciar efecto visual
        if (specialAttackEffect != null)
        {
            GameObject effect = Instantiate(
                specialAttackEffect, 
                transform.position, 
                Quaternion.identity
            );
            
            // Orientar el efecto en la dirección correcta
            effect.transform.right = direction;
        }
        
        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.7f);
        
        // Disparar evento de fin de ataque
        OnAttackFinished?.Invoke(ATTACK_SPECIAL);
        
        isAttacking = false;
    }

    // Método para aumentar el nivel de ataque
    public void IncreaseAttackLevel()
    {
        if (attackLevel < maxAttackLevel)
        {
            attackLevel++;
            Debug.Log($"Nivel de ataque aumentado a {attackLevel}");
        }
    }

    // Método para reducir el nivel de ataque
    public void DecreaseAttackLevel()
    {
        if (attackLevel > 1)
        {
            attackLevel--;
            Debug.Log($"Nivel de ataque reducido a {attackLevel}");
        }
    }

    // Método para establecer directamente el nivel de ataque
    public void SetAttackLevel(int level)
    {
        attackLevel = Mathf.Clamp(level, 1, maxAttackLevel);
        Debug.Log($"Nivel de ataque establecido a {attackLevel}");
    }

    // Obtener el tiempo restante de enfriamiento para un tipo de ataque
    public float GetCooldownRemaining(int attackType)
    {
        switch (attackType)
        {
            case ATTACK_BASIC:
                return Mathf.Max(0, basicAttackCooldown - (Time.time - lastBasicAttackTime));
            case ATTACK_AREA:
                return Mathf.Max(0, areaAttackCooldown - (Time.time - lastAreaAttackTime));
            case ATTACK_SPECIAL:
                return Mathf.Max(0, specialAttackCooldown - (Time.time - lastSpecialAttackTime));
            default:
                return 0;
        }
    }

    // Verificar si un ataque está disponible
    public bool IsAttackAvailable(int attackType)
    {
        return GetCooldownRemaining(attackType) <= 0;
    }

    // Visualizar los rangos de ataque en el editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Rango de ataque básico
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, basicAttackRange);
            
            // Rango de ataque de área
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
            
            // Rango de ataque especial
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, new Vector3(transform.localScale.x, 0, 0) * specialAttackRange);
        }
    }
}

// Sistema de temblor de cámara
public class CameraShake : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        cameraTransform = transform;
        originalPosition = cameraTransform.localPosition;
    }

    // Iniciar el efecto de temblor
    public void ShakeCamera(float intensity, float duration)
    {
        // Detener cualquier temblor existente
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        // Guardar la posición original
        originalPosition = cameraTransform.localPosition;
        
        // Iniciar nuevo temblor
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    // Coroutine para el efecto de temblor
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0.0f;
        
        while (elapsed < duration)
        {
            // Calcular intensidad decreciente
            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp01(4.0f * percentComplete - 3.0f);
            
            // Generar valores aleatorios
            float x = Random.Range(-1f, 1f) * intensity * damper;
            float y = Random.Range(-1f, 1f) * intensity * damper;
            
            // Aplicar temblor
            cameraTransform.localPosition = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );
            
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        
        // Restaurar la posición original
        cameraTransform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}

// Interfaz para suscribirse a eventos de ataque
// Puede ser implementada por cualquier componente que necesite responder a ataques
public interface IAttackListener
{
    void OnAttackStart(int attackType);
    void OnAttackPerform(int attackType);
    void OnAttackEnd(int attackType);
}

// Ejemplo de un componente que escucha eventos de ataque
// Puedes añadir este componente a cualquier GameObject que necesite responder a ataques
public class AttackEventListener : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    
    // Lista de listeners
    private List<IAttackListener> listeners = new List<IAttackListener>();

    private void OnEnable()
    {
        if (playerCombat == null)
        {
            playerCombat = FindObjectOfType<PlayerCombat>();
        }
        
        if (playerCombat != null)
        {
            // Suscribirse a eventos
            playerCombat.OnAttackStarted += HandleAttackStart;
            playerCombat.OnAttackPerformed += HandleAttackPerform;
            playerCombat.OnAttackFinished += HandleAttackEnd;
        }
    }

    private void OnDisable()
    {
        if (playerCombat != null)
        {
            // Desuscribirse de eventos
            playerCombat.OnAttackStarted -= HandleAttackStart;
            playerCombat.OnAttackPerformed -= HandleAttackPerform;
            playerCombat.OnAttackFinished -= HandleAttackEnd;
        }
    }

    // Registrar un nuevo listener
    public void RegisterListener(IAttackListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    // Desregistrar un listener
    public void UnregisterListener(IAttackListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }

    // Manejadores de eventos
    private void HandleAttackStart(int attackType)
    {
        foreach (var listener in listeners)
        {
            listener.OnAttackStart(attackType);
        }
    }

    private void HandleAttackPerform(int attackType)
    {
        foreach (var listener in listeners)
        {
            listener.OnAttackPerform(attackType);
        }
    }

    private void HandleAttackEnd(int attackType)
    {
        foreach (var listener in listeners)
        {
            listener.OnAttackEnd(attackType);
        }
    }
}