using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TimelineSkipController : MonoBehaviour
{
    [Header("Timeline Configuration")]
    public PlayableDirector playableDirector;
    
    [Header("UI Elements")]
    public Button skipButton;
    public GameObject skipButtonObject; // GameObject del botón para mostrar/ocultar
    
    [Header("Skip Settings")]
    public bool canSkip = true;
    public float skipToTime = -1f; // -1 significa saltar al final
    public bool hideButtonWhenPlaying = false;
    
    void Start()
    {
        // Configurar el botón
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTimeline);
        }
        
        // Suscribirse a eventos del Timeline
        if (playableDirector != null)
        {
            playableDirector.played += OnTimelineStarted;
            playableDirector.stopped += OnTimelineStopped;
        }
        
        // Configuración inicial del botón
        UpdateButtonVisibility();
    }
    
    void Update()
    {
        // Opcional: Skip con tecla (ej: Escape o Space)
        if (canSkip && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipTimeline();
        }
    }
    
    public void SkipTimeline()
    {
        if (!canSkip || playableDirector == null)
            return;
            
        if (playableDirector.state == PlayState.Playing)
        {
            if (skipToTime >= 0)
            {
                // Saltar a un tiempo específico
                playableDirector.time = skipToTime;
            }
            else
            {
                // Saltar al final
                playableDirector.time = playableDirector.duration;
            }
            
            // Opcional: Pausar después de saltar
            // playableDirector.Pause();
            
            // Opcional: Detener completamente
            // playableDirector.Stop();
        }
    }
    
    public void SetCanSkip(bool canSkipValue)
    {
        canSkip = canSkipValue;
        UpdateButtonVisibility();
    }
    
    void OnTimelineStarted(PlayableDirector director)
    {
        if (hideButtonWhenPlaying)
        {
            UpdateButtonVisibility();
        }
    }
    
    void OnTimelineStopped(PlayableDirector director)
    {
        UpdateButtonVisibility();
    }
    
    void UpdateButtonVisibility()
    {
        if (skipButtonObject != null)
        {
            bool shouldShow = canSkip;
            
            if (hideButtonWhenPlaying && playableDirector != null)
            {
                shouldShow = shouldShow && (playableDirector.state != PlayState.Playing);
            }
            
            skipButtonObject.SetActive(shouldShow);
        }
        
        if (skipButton != null)
        {
            skipButton.interactable = canSkip;
        }
    }
    
    void OnDestroy()
    {
        // Limpiar eventos
        if (playableDirector != null)
        {
            playableDirector.played -= OnTimelineStarted;
            playableDirector.stopped -= OnTimelineStopped;
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipTimeline);
        }
    }
}