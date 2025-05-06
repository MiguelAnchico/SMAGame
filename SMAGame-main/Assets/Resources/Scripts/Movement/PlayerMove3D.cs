using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float jumpCooldown = 0.1f;
    
    // Referencias de componentes
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 moveDirection;
    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpReleased = true;
    private IInteractable currentInteractable;
    private float lastJumpTime = -10f;
    
    // Referencia al PlayerDataManager
    private PlayerDataManager dataManager;

    // Referencia al Animator
    private Animator animator;
    
    // Lista de objetos interactuables en rango
    private List<GameObject> interactablesInRange = new List<GameObject>();


    [SerializeField] private AudioClip saltoSonido;
    
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
            boxCollider.size = new Vector2(1f, 2f);
        }
        
        // Crear trigger para detección de suelo si no existe
        CreateGroundCheck();
        
        // Crear trigger para detección de interactuables si no existe
        CreateInteractionTrigger();
        
        // Obtener la referencia al PlayerDataManager
        dataManager = GetComponent<PlayerDataManager>();
        if (dataManager == null)
        {
            dataManager = GetComponentInChildren<PlayerDataManager>();
        }

        // Obtener la referencia al Animator
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    
    private void CreateGroundCheck()
    {
        Transform groundCheck = transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -6.83f, 0);
            
            CircleCollider2D groundCollider = checkObj.AddComponent<CircleCollider2D>();
            groundCollider.radius = 0.1f;
            groundCollider.isTrigger = true;
            
            GroundCheck groundCheckScript = checkObj.AddComponent<GroundCheck>();
            groundCheckScript.playerMove = this;
        }
    }
    
    private void CreateInteractionTrigger()
    {
        Transform interactionTrigger = transform.Find("InteractionTrigger");
        if (interactionTrigger == null)
        {
            GameObject triggerObj = new GameObject("InteractionTrigger");
            triggerObj.transform.parent = transform;
            triggerObj.transform.localPosition = Vector3.zero;
            
            CircleCollider2D interactionCollider = triggerObj.AddComponent<CircleCollider2D>();
            interactionCollider.radius = 1.5f;
            interactionCollider.isTrigger = true;
            
            InteractionDetector detector = triggerObj.AddComponent<InteractionDetector>();
            detector.playerMove = this;
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
        
        // Detectar movimiento vertical para saltar
        if (moveInput.y > 0.5f && isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            TryJump();
        }

        // Actualizar animaciones
        UpdateAnimations();
    }
    
    private void FixedUpdate()
    {
        // Aplicar el movimiento
        Move();
        
        // Aplicar la gravedad personalizada
        ApplyGravity();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
            ControladorSonido.Instance.EjecutarSonido(saltoSonido);
        }
        else if (context.canceled)
        {
            jumpPressed = false;
            jumpReleased = true;
        }
    }
    
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
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
    
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
        
        // Actualizar el parámetro isFalling inmediatamente cuando toque el suelo
        if (animator != null && grounded)
        {
            animator.SetBool("isFalling", false);
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            // Actualizar el parámetro xVelocity con la velocidad horizontal actual
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
            
            // Actualizar el parámetro isFalling
            animator.SetBool("isFalling", !isGrounded);
        }
    }
    
    public void AddInteractable(GameObject interactable)
    {
        if (!interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Add(interactable);
            UpdateCurrentInteractable();
        }
    }
    
    public void RemoveInteractable(GameObject interactable)
    {
        if (interactablesInRange.Contains(interactable))
        {
            interactablesInRange.Remove(interactable);
            UpdateCurrentInteractable();
        }
    }
    
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
        
        if (currentInteractable != null)
        {
            Debug.Log($"Objeto interactuable actual: {currentInteractable.GetName()}");
        }
    }
    
    private void CalculateMoveDirection()
    {
        moveDirection = new Vector2(moveInput.x, 0);
    }
    
    private void RotatePlayer()
    {
        if (moveDirection.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveDirection.x) * 0.2305938f, 0.2305938f, 0.2305938f);
        }
    }
    
    private void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
    }
    
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        SetGroundedState(false);
        
        // Establecer el parámetro isFalling a true al saltar
        if (animator != null)
        {
            animator.SetBool("isFalling", true);
        }
    }
    
    private void ApplyGravity()
    {
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