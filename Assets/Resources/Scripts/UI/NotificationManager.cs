using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject notificationPanel; // Panel hijo dentro del Canvas
    public TextMeshProUGUI taskTitleText;
    public TextMeshProUGUI taskDescriptionText;
    public Image taskIcon;
    
    [Header("Notification Settings")]
    public float extraDisplayTime = 3f; // Tiempo extra después del audio
    public float slideInDuration = 0.3f;
    public float slideOutDuration = 0.3f;
    public float delayBetweenTasks = 0.4f; // Pausa entre tarea completada y siguiente
    
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip taskCompletedSound;
    public AudioClip taskFailedSound;
    
    [Header("Visual Settings")]
    public Sprite taskNotificationIcon;
    public Sprite taskCompletedIcon;
    public Sprite taskFailedIcon;
    
    [Header("Animation")]
    public RectTransform notificationRect; // RectTransform del panel
    public Vector3 slideInOffset = new Vector3(0, 100f, 0);
    
    public static NotificationManager Instance { get; private set; }
    
    private Coroutine currentNotificationCoroutine;
    private Coroutine pendingTaskCoroutine;
    private float currentAudioDuration = 0f;
    private Task pendingTask = null;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // El Canvas completo persiste
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowTaskNotification(Task task)
    {
        // Si hay una notificación de tarea completada/fallida mostrándose, guardar esta tarea como pendiente
        if (currentNotificationCoroutine != null && IsShowingCompletionNotification())
        {
            pendingTask = task;
            if (pendingTaskCoroutine == null)
            {
                pendingTaskCoroutine = StartCoroutine(WaitForPendingTask());
            }
            return;
        }
        
        // Detener cualquier notificación anterior
        StopAllNotifications();
        
        // Verificar que tenemos las referencias necesarias
        if (notificationPanel == null || notificationRect == null)
        {
            return;
        }
        
        // Configurar contenido
        if (taskTitleText != null)
            taskTitleText.text = task.title;
            
        if (taskDescriptionText != null)
            taskDescriptionText.text = task.description;
            
        if (taskIcon != null && taskNotificationIcon != null)
            taskIcon.sprite = taskNotificationIcon;
        
        // Reproducir audio de la tarea y obtener duración
        currentAudioDuration = PlayTaskAudio(task);
        
        // Mostrar notificación
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
    }
    
    public void ShowTaskCompleted(Task task)
    {
        // Detener cualquier notificación anterior inmediatamente
        StopAllNotifications();
        
        // Configurar contenido para completada
        if (taskTitleText != null)
            taskTitleText.text = "¡Tarea Completada!";
            
        if (taskDescriptionText != null)
            taskDescriptionText.text = task.title;
            
        if (taskIcon != null && taskCompletedIcon != null)
            taskIcon.sprite = taskCompletedIcon;
        
        // Reproducir sonido de validación y obtener duración
        currentAudioDuration = PlaySound(taskCompletedSound);
        
        // Mostrar notificación
        currentNotificationCoroutine = StartCoroutine(ShowCompletionNotificationCoroutine());
    }
    
    public void ShowTaskFailed(Task task)
    {
        // Detener cualquier notificación anterior inmediatamente
        StopAllNotifications();
        
        // Configurar contenido para fallida
        if (taskTitleText != null)
            taskTitleText.text = "Tarea Fallida";
            
        if (taskDescriptionText != null)
            taskDescriptionText.text = task.title;
            
        if (taskIcon != null && taskFailedIcon != null)
            taskIcon.sprite = taskFailedIcon;
        
        // Reproducir sonido de fallo y obtener duración
        currentAudioDuration = PlaySound(taskFailedSound);
        
        // Mostrar notificación
        currentNotificationCoroutine = StartCoroutine(ShowCompletionNotificationCoroutine());
    }
    
    IEnumerator ShowCompletionNotificationCoroutine()
    {
        // Preparar para animación
        Vector3 originalPosition = notificationRect.anchoredPosition;
        Vector3 hiddenPosition = originalPosition + slideInOffset;
        
        // Posición inicial (oculta)
        notificationRect.anchoredPosition = hiddenPosition;
        notificationPanel.SetActive(true);
        
        // Animación de entrada (slide in)
        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideInDuration;
            
            // Slide in
            notificationRect.anchoredPosition = Vector3.Lerp(hiddenPosition, originalPosition, progress);
            
            yield return null;
        }
        
        // Asegurar posición final
        notificationRect.anchoredPosition = originalPosition;
        
        // Esperar duración del audio + tiempo extra
        float totalDisplayTime = currentAudioDuration + extraDisplayTime;
        float waitTime = totalDisplayTime - slideInDuration - slideOutDuration;
        
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        
        // Animación de salida (slide out)
        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideOutDuration;
            
            // Slide out
            notificationRect.anchoredPosition = Vector3.Lerp(originalPosition, hiddenPosition, progress);
            
            yield return null;
        }
        
        // Ocultar completamente
        HideNotificationImmediate();
        currentNotificationCoroutine = null;
        
        // Si hay una tarea pendiente, programar su visualización después del delay
        if (pendingTask != null && pendingTaskCoroutine == null)
        {
            pendingTaskCoroutine = StartCoroutine(WaitForPendingTask());
        }
    }
    
    float PlayTaskAudio(Task task)
    {
        // Reproducir audio específico de la tarea si existe
        if (task.audioClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(task.audioClip);
            return task.audioClip.length;
        }
        
        return 0f; // Sin audio, retornar 0 duración
    }
    
    float PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            return clip.length;
        }
        
        return 0f; // Sin audio, retornar 0 duración
    }
    
    void HideNotificationImmediate()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    // Métodos públicos para control externo
    public void SetExtraDisplayTime(float time)
    {
        extraDisplayTime = time;
    }
    
    public void HideCurrentNotification()
    {
        StopAllNotifications();
    }
    
    // Método para testing
    public void TestNotification()
    {
        Task testTask = new Task(999, "Tarea de Prueba", "Esta es una notificación de prueba", true, 10f, false);
        ShowTaskNotification(testTask);
    }

    IEnumerator ShowNotificationCoroutine()
    {
        // Preparar para animación
        Vector3 originalPosition = notificationRect.anchoredPosition;
        Vector3 hiddenPosition = originalPosition + slideInOffset;
        
        // Posición inicial (oculta)
        notificationRect.anchoredPosition = hiddenPosition;
        notificationPanel.SetActive(true);
        
        // Animación de entrada (slide in)
        float elapsed = 0f;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideInDuration;
            
            // Slide in
            notificationRect.anchoredPosition = Vector3.Lerp(hiddenPosition, originalPosition, progress);
            
            yield return null;
        }
        
        // Asegurar posición final
        notificationRect.anchoredPosition = originalPosition;
        
        // Esperar duración del audio + tiempo extra
        float totalDisplayTime = currentAudioDuration + extraDisplayTime;
        float waitTime = totalDisplayTime - slideInDuration - slideOutDuration;
        
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        
        // Animación de salida (slide out)
        elapsed = 0f;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideOutDuration;
            
            // Slide out
            notificationRect.anchoredPosition = Vector3.Lerp(originalPosition, hiddenPosition, progress);
            
            yield return null;
        }
        
        // Ocultar completamente
        HideNotificationImmediate();
        currentNotificationCoroutine = null;
    }
    
    IEnumerator WaitForPendingTask()
    {
        // Esperar el delay configurado entre tareas
        yield return new WaitForSeconds(delayBetweenTasks);
        
        // Mostrar la tarea pendiente
        if (pendingTask != null)
        {
            Task taskToShow = pendingTask;
            pendingTask = null;
            pendingTaskCoroutine = null;
            
            ShowTaskNotification(taskToShow);
        }
        else
        {
            pendingTaskCoroutine = null;
        }
    }
    
    void StopAllNotifications()
    {
        // Detener audio actual
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Detener corrutinas
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
            currentNotificationCoroutine = null;
        }
        
        if (pendingTaskCoroutine != null)
        {
            StopCoroutine(pendingTaskCoroutine);
            pendingTaskCoroutine = null;
        }
        
        // Ocultar panel
        HideNotificationImmediate();
    }
    
    bool IsShowingCompletionNotification()
    {
        // Verificar si el texto actual indica una notificación de completado/fallido
        if (taskTitleText != null)
        {
            string currentText = taskTitleText.text;
            return currentText == "¡Tarea Completada!" || currentText == "Tarea Fallida";
        }
        return false;
    }
}