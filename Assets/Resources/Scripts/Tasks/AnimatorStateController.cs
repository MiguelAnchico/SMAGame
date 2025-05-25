using UnityEngine;

public class AnimatorStateController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Actualizar el estado al cargar
        UpdateAnimatorState();
    }

    // Función para actualizar el valor "state" del animator
    public void UpdateAnimatorState()
    {
        if (animator != null && GameStateManager.Instance != null)
        {
            float currentState = GameStateManager.Instance.GameState;
            animator.SetFloat("state", currentState);

            Debug.Log($"Animator state actualizado a: {currentState}");
        }
        else
        {
            Debug.LogWarning("Animator o GameStateManager no encontrado");
        }
    }

    // Función para cambiar el valor "isSleeping"
    public void SetSleeping(bool isSleeping)
    {
        if (animator != null)
        {
            animator.SetBool("isSleeping", isSleeping);
            
            Debug.Log($"isSleeping cambiado a: {isSleeping}");
        }
        else
        {
            Debug.LogWarning("Animator no encontrado");
        }
    }
}