using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkipButtonEnabler : MonoBehaviour
{
    [Header("References")]
    public Button triggerButton; // El botón que se toca para mostrar el skip
    public GameObject skipButtonObject; // El GameObject del botón de skip
    public TimelineSkipController skipController; // Referencia al controlador de skip
    
    [Header("Settings")]
    public float displayDuration = 5f; // Tiempo que se muestra el botón de skip
    public bool fadeTransitions = false; // Si usar transiciones de fade
    public CanvasGroup skipButtonCanvasGroup; // Para fade (opcional)
    
    private Coroutine hideSkipButtonCoroutine;
    private bool isSkipButtonVisible = false;
    
    void Start()
    {
        // Configurar el botón trigger
        if (triggerButton != null)
        {
            triggerButton.onClick.AddListener(ShowSkipButton);
        }
        
        // Asegurar que el botón de skip esté oculto al inicio
        HideSkipButtonImmediate();
    }
    
    public void ShowSkipButton()
    {
        // Detener cualquier corrutina anterior de ocultar
        if (hideSkipButtonCoroutine != null)
        {
            StopCoroutine(hideSkipButtonCoroutine);
        }
        
        // Mostrar el botón de skip
        if (skipButtonObject != null)
        {
            skipButtonObject.SetActive(true);
        }
        
        // Habilitar el skip en el controlador
        if (skipController != null)
        {
            skipController.SetCanSkip(true);
        }
        
        // Aplicar fade in si está habilitado
        if (fadeTransitions && skipButtonCanvasGroup != null)
        {
            StartCoroutine(FadeInSkipButton());
        }
        
        isSkipButtonVisible = true;
        
        // Iniciar el timer para ocultar después de 5 segundos
        hideSkipButtonCoroutine = StartCoroutine(HideSkipButtonAfterDelay());
    }
    
    IEnumerator HideSkipButtonAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HideSkipButton();
    }
    
    void HideSkipButton()
    {
        if (!isSkipButtonVisible) return;
        
        if (fadeTransitions && skipButtonCanvasGroup != null)
        {
            StartCoroutine(FadeOutSkipButton());
        }
        else
        {
            HideSkipButtonImmediate();
        }
        
        isSkipButtonVisible = false;
    }
    
    void HideSkipButtonImmediate()
    {
        // Ocultar el botón de skip
        if (skipButtonObject != null)
        {
            skipButtonObject.SetActive(false);
        }
        
        // Deshabilitar el skip en el controlador
        if (skipController != null)
        {
            skipController.SetCanSkip(false);
        }
        
        // Asegurar alpha completo si hay CanvasGroup
        if (skipButtonCanvasGroup != null)
        {
            skipButtonCanvasGroup.alpha = 1f;
        }
    }
    
    IEnumerator FadeInSkipButton()
    {
        float elapsed = 0f;
        float fadeTime = 0.3f;
        
        skipButtonCanvasGroup.alpha = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            skipButtonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
            yield return null;
        }
        
        skipButtonCanvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOutSkipButton()
    {
        float elapsed = 0f;
        float fadeTime = 0.3f;
        float startAlpha = skipButtonCanvasGroup.alpha;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            skipButtonCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeTime);
            yield return null;
        }
        
        skipButtonCanvasGroup.alpha = 0f;
        HideSkipButtonImmediate();
    }
    
    // Métodos públicos para control externo
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }
    
    public void ForceHideSkipButton()
    {
        if (hideSkipButtonCoroutine != null)
        {
            StopCoroutine(hideSkipButtonCoroutine);
        }
        HideSkipButton();
    }
    
    public void ExtendDisplayTime(float additionalTime)
    {
        if (isSkipButtonVisible)
        {
            // Reiniciar el timer con tiempo adicional
            if (hideSkipButtonCoroutine != null)
            {
                StopCoroutine(hideSkipButtonCoroutine);
            }
            hideSkipButtonCoroutine = StartCoroutine(HideSkipButtonAfterDelay());
        }
    }
    
    void OnDestroy()
    {
        // Limpiar eventos
        if (triggerButton != null)
        {
            triggerButton.onClick.RemoveListener(ShowSkipButton);
        }
        
        // Detener corrutinas
        if (hideSkipButtonCoroutine != null)
        {
            StopCoroutine(hideSkipButtonCoroutine);
        }
    }
}