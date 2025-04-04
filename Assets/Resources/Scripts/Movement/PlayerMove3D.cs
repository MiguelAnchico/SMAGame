using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float jumpCooldown = 0.1f; // Cooldown para evitar saltos repetidos
    
    // Referencias de componentes
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveDirection;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpReleased = true; // Para evitar que se sostenga el salto
    private IInteractable currentInteractable;
    private float lastJumpTime = -10f; // Tiempo del último salto (inicializado en el pasado)
    
    // Referencia al PlayerDataManager
    private PlayerDataManager dataManager;
    
    // Lista de objetos interactuables en rango
    private List<GameObject> interactablesInRange = new List<GameObject>();
    
    private void Awake()
    {
        // Obtener o añadir componente Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configurar el Rigidbody2D
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Asegurarse de que el jugador tenga un collider
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 2f); // Tamaño básico, ajustar según el sprite
        }
        
        // Crear trigger para detección de suelo si no existe
        Transform groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -1.1f, 0); // Justo debajo del jugador
            
            // Añadir un collider trigger para detectar el suelo
            CircleCollider2D groundCollider = checkObj.AddComponent<CircleCollider2D>();
            groundCollider.radius = 0.1f;
            groundCollider.isTrigger = true;
            
            // Añadir un script para manejar los triggers de suelo
            GroundCheck groundCheckScript = checkObj.AddComponent<GroundCheck>();
            groundCheckScript.playerMove = this;
        }
        
        // Crear trigger para detección de interactuables si no existe
        Transform interactionTrigger = transform.Find("InteractionTrigger");
        if (interactionTrigger == null)
        {
            GameObject triggerObj = new GameObject("InteractionTrigger");
            triggerObj.transform.parent = transform;
            triggerObj.transform.localPosition = Vector3.zero;
            
            // Añadir un collider trigger para detectar objetos interactuables
            CircleCollider2D interactionCollider = triggerObj.AddComponent<CircleCollider2D>();
            interactionCollider.radius = 1.5f; // Ajustar según necesidades
            interactionCollider.isTrigger = true;
            
            // Añadir un script para manejar los triggers de interacción
            InteractionDetector detector = triggerObj.AddComponent<InteractionDetector>();
            detector.playerMove = this;
        }
        
        // Obtener la referencia al PlayerDataManager
        dataManager = GetComponent<PlayerDataManager>();
    }
    
    private void Start()
    {
        // Inicializar al jugador
        InitializePlayer();
    }
    
    /// <summary>
    /// Inicializa al jugador con los datos de posición del PlayerDataManager
    /// </summary>
    public void InitializePlayer()
    {
        // Buscar el PlayerDataManager si no se ha asignado aún
        if (dataManager == null)
        {
            dataManager = GetComponent<PlayerDataManager>();
            
            // Si aún no se encuentra, buscar en los hijos
            if (dataManager == null)
            {
                dataManager = GetComponentInChildren<PlayerDataManager>();
            }
        }
        
        // Si se encontró el PlayerDataManager, usar su posición
        if (dataManager != null)
        {
            Vector3 savedPosition = dataManager.GetLastSavedPosition();
            transform.position = savedPosition;
            Debug.Log($"Jugador inicializado en posición: {savedPosition}");
        }
        else
        {
            Debug.LogWarning("No se encontró PlayerDataManager para inicializar la posición del jugador");
        }
    }
    
    private void Update()
    {
        // Calcular la dirección de movimiento
        CalculateMoveDirection();
        
        // Rotar el personaje hacia la dirección de movimiento
        RotatePlayer();
        
        // Gestionar el botón de salto
        if (jumpPressed && isGrounded && jumpReleased)
        {
            TryJump();
        }
        
        // Detectar movimiento vertical para saltar (parte nueva)
        if (moveInput.y > 0.5f && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            TryJump();
        }
    }
    
    private void FixedUpdate()
    {
        // Aplicar el movimiento
        Move();
        
        // Aplicar la gravedad personalizada
        ApplyGravity();
    }
    
    // Llamado por el Input System cuando se mueve el stick/WASD
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    // Llamado por el Input System cuando se presiona el botón de salto
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
        }
        else if (context.canceled)
        {
            jumpPressed = false;
            jumpReleased = true; // Resetear cuando se suelta el botón
        }
    }
    
    // Método para intentar saltar con verificación de cooldown
    private void TryJump()
    {
        if (Time.time > lastJumpTime + jumpCooldown)
        {
            Jump();
            lastJumpTime = Time.time;
            jumpPressed = false;
            jumpReleased = false;
        }
    }
    
    // Llamado por el Input System cuando se presiona el botón de interacción
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
    
    // Método para establecer el estado de tierra (llamado desde GroundCheck)
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }
    
    // Método para añadir un objeto interactuable (llamado desde InteractionDetector)
    public void AddInteractable(GameObject interactable)
    {
        if (!interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
            
            // Establecer el objeto más cercano como actual
            UpdateCurrentInteractable();
        }
    }
    
    // Método para quitar un objeto interactuable (llamado desde InteractionDetector)
    public void RemoveInteractable(GameObject interactable)
    {
        if (interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Remove(interactable);
            
            // Actualizar el objeto actual
            UpdateCurrentInteractable();
        }
    }
    
    // Actualizar cuál es el objeto interactuable actual (el más cercano)
    private void UpdateCurrentInteractable()
    {
        currentInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (GameObject obj in interactablesInRange)
        {
            if (obj != null)
            {
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    IInteractable interactable = obj.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        currentInteractable = interactable;
                    }
                }
            }
        }
        
        // Opcional: Mostrar un indicador para el objeto seleccionado
        if (currentInteractable != null)
        {
            Debug.Log($"Objeto interactuable actual: {currentInteractable.GetName()}");
        }
    }
    
    private void CalculateMoveDirection()
    {
        // Para movimento horizontal, usamos solo el componente X
        // Ignoramos el componente Y para el movimiento, ya que lo usaremos para saltar
        moveDirection = new Vector2(moveInput.x, 0);
    }
    
    private void RotatePlayer()
    {
        // Para 2D, simplemente voltear el sprite en el eje X
        if (moveDirection.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveDirection.x), 1, 1);
        }
    }
    
    private void Move()
    {
        // Aplicar movimiento horizontal, manteniendo velocidad vertical actual
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }
    
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        SetGroundedState(false);
    }
    
    private void ApplyGravity()
    {
        // Aplicar más gravedad cuando está cayendo para un salto más natural
        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector2.up * gravityValue * fallMultiplier * Time.fixedDeltaTime, ForceMode2D.Force);
        }
        else
        {
            rb.AddForce(Vector2.up * gravityValue * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }
    

}

// Clase para detectar cuando el jugador está en el suelo
public class GroundCheck : MonoBehaviour
{
    [HideInInspector] public PlayerMove3D playerMove;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si la colisión es con una capa de suelo
        if (IsGround(collision.gameObject.layer))
        {
            playerMove.SetGroundedState(true);
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Mantener el estado de tierra mientras esté en contacto
        if (IsGround(collision.gameObject.layer))
        {
            playerMove.SetGroundedState(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Al salir del suelo, verificar si aún está en contacto con algún otro suelo
        if (IsGround(collision.gameObject.layer))
        {
            // Comprobar si hay otros suelos en contacto
            Collider2D[] contacts = new Collider2D[5];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground")); // Ajustar según tu configuración
            
            if (GetComponent<Collider2D>().Overlap(filter, contacts) == 0)
            {
                playerMove.SetGroundedState(false);
            }
        }
    }
    
    // Función auxiliar para verificar si una capa es suelo
    private bool IsGround(int layer)
    {
        // Ajustar "Ground" al nombre de tu capa de suelo
        return layer == LayerMask.NameToLayer("Ground");
    }
}

// Clase para detectar objetos interactuables
public class InteractionDetector : MonoBehaviour
{
    [HideInInspector] public PlayerMove3D playerMove;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            playerMove.AddInteractable(collision.gameObject);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            playerMove.RemoveInteractable(collision.gameObject);
        }
    }
}

// Interfaz para objetos interactuables
public interface IInteractable
{
    void Interact();
    string GetName(); // Añadido para poder mostrar el nombre del objeto
}

// Ejemplo de objeto interactuable para 2D
public class Interactable2D : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionName = "Objeto";
    [SerializeField] private string interactionMessage = "Objeto activado";
    [SerializeField] private Color highlightColor = Color.yellow;
    
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Asegurarse de que tiene un collider para la detección
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }
    
    public void Interact()
    {
        isActive = !isActive;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isActive ? highlightColor : originalColor;
        }
        
        Debug.Log(interactionMessage);
    }
    
    public string GetName()
    {
        return interactionName;
    }
}