using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private float playerSpeed = 5.0f;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Desactivar gravedad
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // Llamado por la acción Move del Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        // Leer valores del Left Stick
        moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // Rotar el sprite según la dirección (opcional)
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
        }
    }

    void FixedUpdate()
    {
        // Aplicar el movimiento en ambas direcciones (x e y)
        rb.velocity = moveInput * playerSpeed;
    }
}