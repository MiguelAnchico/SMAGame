using UnityEngine;

public class ObjectDisappearTaskCompleter : MonoBehaviour
{
    [Header("Task Settings")]
    public int taskIdToComplete = 1;
    
    [Header("Object Detection")]
    public GameObject objectToWatch; // Objeto a monitorear
    public bool watchSelfDestruction = false; // Si monitorear la destrucción de este mismo objeto
    
    [Header("Detection Method")]
    public DetectionMode detectionMode = DetectionMode.ObjectDestroyed;
    
    [Header("Optional Settings")]
    public float checkInterval = 0.1f; // Intervalo de verificación en segundos
    
    public enum DetectionMode
    {
        ObjectDestroyed,    // El objeto fue destruido (null)
        ObjectDeactivated,  // El objeto fue desactivado
        ObjectInvisible,    // El objeto se volvió invisible (renderer disabled)
        ObjectMoved         // El objeto se movió fuera de la escena
    }
    
    private bool hasCompleted = false;
    private bool wasObjectActiveLastFrame;
    private Renderer objectRenderer;
    private Vector3 lastKnownPosition;
    private float checkTimer = 0f;
    
    void Start()
    {
        // Si no se asigna objeto específico, monitorear este mismo objeto
        if (objectToWatch == null && watchSelfDestruction)
        {
            objectToWatch = gameObject;
            Debug.Log($"🎯 Monitoreando autodestrucción del objeto {gameObject.name}");
        }
        
        if (objectToWatch != null)
        {
            wasObjectActiveLastFrame = objectToWatch.activeInHierarchy;
            objectRenderer = objectToWatch.GetComponent<Renderer>();
            lastKnownPosition = objectToWatch.transform.position;
            Debug.Log($"🎯 Iniciando monitoreo del objeto {objectToWatch.name} para tarea {taskIdToComplete}");
        }
        else
        {
            Debug.LogWarning("⚠️ ObjectDisappearTaskCompleter: No hay objeto asignado para monitorear");
        }
    }
    
    void Update()
    {
        if (hasCompleted) return;
        
        // Verificar con intervalo para optimizar performance
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckObjectStatus();
        }
    }
    
    void CheckObjectStatus()
    {
        bool objectDisappeared = false;
        
        switch (detectionMode)
        {
            case DetectionMode.ObjectDestroyed:
                // Si el objeto es null, definitivamente fue destruido
                objectDisappeared = (objectToWatch == null);
                break;
                
            case DetectionMode.ObjectDeactivated:
                // Si es null O está desactivado
                objectDisappeared = (objectToWatch == null || !objectToWatch.activeInHierarchy);
                break;
                
            case DetectionMode.ObjectInvisible:
                if (objectToWatch == null)
                {
                    // Si el objeto ya no existe, considerarlo como desaparecido
                    objectDisappeared = true;
                }
                else if (objectRenderer != null)
                {
                    objectDisappeared = !objectRenderer.enabled;
                }
                else
                {
                    // Si no tiene renderer, verificar si fue destruido
                    objectDisappeared = (objectToWatch == null);
                }
                break;
                
            case DetectionMode.ObjectMoved:
                if (objectToWatch == null)
                {
                    // Si el objeto ya no existe, considerarlo como desaparecido
                    objectDisappeared = true;
                }
                else
                {
                    // Verificar si se movió muy lejos (fuera de los límites de la escena)
                    float distanceMoved = Vector3.Distance(objectToWatch.transform.position, lastKnownPosition);
                    objectDisappeared = (distanceMoved > 1000f); // Ajustar según necesidades
                }
                break;
        }
        
        if (objectDisappeared)
        {
            OnObjectDisappeared();
        }
    }
    
    void OnObjectDisappeared()
    {
        if (hasCompleted) return;
        
        hasCompleted = true;
        
        Debug.Log($"🎯 Objeto desaparecido detectado - Completando tarea {taskIdToComplete}");
        
        // Completar la tarea
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(taskIdToComplete);
            Debug.Log($"✅ Tarea {taskIdToComplete} completada por desaparición de objeto");
        }
        else
        {
            Debug.LogError("❌ TaskManager.Instance es null - No se puede completar la tarea");
        }
    }
    
    // Si este script está en el mismo objeto que se va a destruir
    void OnDestroy()
    {
        if (watchSelfDestruction && !hasCompleted)
        {
            Debug.Log($"🎯 OnDestroy activado - Completando tarea {taskIdToComplete}");
            
            // Completar la tarea antes de que se destruya este script
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(taskIdToComplete);
                Debug.Log($"✅ Tarea {taskIdToComplete} completada en OnDestroy");
            }
            else
            {
                Debug.LogError("❌ TaskManager.Instance es null en OnDestroy");
            }
        }
    }
    
    // Métodos públicos para control externo
    public void SetObjectToWatch(GameObject newObject)
    {
        objectToWatch = newObject;
        if (objectToWatch != null)
        {
            wasObjectActiveLastFrame = objectToWatch.activeInHierarchy;
            objectRenderer = objectToWatch.GetComponent<Renderer>();
            lastKnownPosition = objectToWatch.transform.position;
        }
        hasCompleted = false;
    }
    
    public void SetTaskId(int newTaskId)
    {
        taskIdToComplete = newTaskId;
    }
    
    public void ResetWatcher()
    {
        hasCompleted = false;
        if (objectToWatch != null)
        {
            wasObjectActiveLastFrame = objectToWatch.activeInHierarchy;
            lastKnownPosition = objectToWatch.transform.position;
        }
    }
    
    public void ForceComplete()
    {
        OnObjectDisappeared();
    }
    
    // Información de debug
    void OnDrawGizmosSelected()
    {
        if (objectToWatch != null)
        {
            // Dibujar línea hacia el objeto monitoreado
            Gizmos.color = hasCompleted ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, objectToWatch.transform.position);
            
            // Dibujar esfera en el objeto
            Gizmos.color = hasCompleted ? Color.green : Color.red;
            Gizmos.DrawWireSphere(objectToWatch.transform.position, 0.5f);
        }
    }
}