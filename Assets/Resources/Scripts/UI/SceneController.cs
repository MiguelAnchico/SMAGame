using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public string sceneLoadName;
    public TextMeshProUGUI textProgress;
    public Slider sliderProgress;
    public float currentPercent;
    private AsyncOperation loadAsync;
    
    [Header("Configuración avanzada")]
    [Tooltip("Tiempo mínimo de carga en segundos")]
    public float minimumLoadTime = 2.0f;
    
    [Tooltip("Tiempo adicional para precarga de recursos")]
    public float preloadTime = 2.0f;
    
    [Header("Recursos para precargar")]
    [Tooltip("Rutas de los recursos de animación a precargar (SOLO el nombre dentro de Resources, sin 'Assets/Resources/')")]
    public string[] animationResourcePaths;
    
    [Tooltip("Rutas de los recursos de sprites a precargar (SOLO el nombre dentro de Resources, sin 'Assets/Resources/')")]
    public string[] spriteResourcePaths;
    
    [Tooltip("Número de intentos de precarga")]
    public int maxLoadAttempts = 3;
    
    [Tooltip("Tiempo entre intentos")]
    public float timeBetweenAttempts = 0.5f;
    
    [Header("Depuración")]
    [Tooltip("Mostrar mensajes detallados en la consola")]
    public bool showDebugMessages = true;

    private List<Object> preloadedResources = new List<Object>();

    public void LoadSceneButton()
    {
        StartCoroutine(LoadSceneRobust(sceneLoadName));
    }

    public IEnumerator LoadSceneRobust(string nameToLoad)
    {
        // Iniciar tiempo
        float startTime = Time.time;
        
        // Iniciar carga asíncrona
        textProgress.text = "Cargando.. 00%";
        loadAsync = SceneManager.LoadSceneAsync(nameToLoad);
        loadAsync.allowSceneActivation = false;
        
        // Esperar hasta que la carga esté casi completa (0.9)
        while (loadAsync.progress < 0.9f)
        {
            currentPercent = loadAsync.progress * 100 / 0.9f;
            textProgress.text = "Cargando.. " + currentPercent.ToString("00") + "%";
            sliderProgress.value = Mathf.MoveTowards(sliderProgress.value, currentPercent, 25 * Time.deltaTime);
            yield return null;
        }
        
        // Establecer visualmente al 85%
        currentPercent = 85f;
        textProgress.text = "Cargando recursos... 85%";
        
        // Asegurar tiempo mínimo de carga
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minimumLoadTime)
        {
            yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
        }
        
        // Limpieza de memoria antes de precargar
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        yield return new WaitForSeconds(0.5f);
        
        // Imprimir información de depuración sobre las rutas si está habilitado
        if (showDebugMessages)
        {
            Debug.Log("=== INICIANDO CARGA DE RECURSOS ===");
            Debug.Log($"Carpeta Resources en: {Application.dataPath}/Resources");
            
            if (animationResourcePaths != null && animationResourcePaths.Length > 0)
            {
                Debug.Log("Rutas de animación configuradas:");
                foreach (string path in animationResourcePaths)
                {
                    Debug.Log($"  - '{path}'");
                }
            }
            
            if (spriteResourcePaths != null && spriteResourcePaths.Length > 0)
            {
                Debug.Log("Rutas de sprites configuradas:");
                foreach (string path in spriteResourcePaths)
                {
                    Debug.Log($"  - '{path}'");
                }
            }
        }
        
        // Precargar animaciones y sprites específicamente
        yield return StartCoroutine(PreloadResources());
        
        // Establecer visualmente al 95%
        currentPercent = 95f;
        textProgress.text = "Preparando escena... 95%";
        sliderProgress.value = 95f;
        
        // Tiempo extra para asegurar que todo esté listo
        yield return new WaitForSeconds(preloadTime);
        
        // Completar visualmente
        currentPercent = 100f;
        textProgress.text = "¡Listo! 100%";
        sliderProgress.value = 100f;
        
        // Pausa final antes de activar la escena
        yield return new WaitForSeconds(0.5f);
        
        // Activar la escena
        loadAsync.allowSceneActivation = true;
    }
    
    private IEnumerator PreloadResources()
    {

        // Precargar sprites
        if (spriteResourcePaths != null && spriteResourcePaths.Length > 0)
        {
            for (int i = 0; i < spriteResourcePaths.Length; i++)
            {
                string path = RemoveInvalidPathPrefix(spriteResourcePaths[i]);
                yield return StartCoroutine(AttemptLoadResources<Sprite>(path));
                
                // Actualizar progreso visual de precarga de sprites
                float progressStep = 5f / spriteResourcePaths.Length;
                currentPercent = 90f + progressStep * (i + 1);
                textProgress.text = $"Cargando sprites... {currentPercent:00}%";
                sliderProgress.value = currentPercent;
                
                yield return null;
            }
        }

        // Precargar animaciones
        if (animationResourcePaths != null && animationResourcePaths.Length > 0)
        {
            for (int i = 0; i < animationResourcePaths.Length; i++)
            {
                string path = RemoveInvalidPathPrefix(animationResourcePaths[i]);
                yield return StartCoroutine(AttemptLoadResources<AnimationClip>(path));
                
                // Actualizar progreso visual de precarga
                float progressStep = 5f / animationResourcePaths.Length;
                currentPercent = 85f + progressStep * (i + 1);
                textProgress.text = $"Cargando animaciones... {currentPercent:00}%";
                sliderProgress.value = currentPercent;
                
                yield return null;
            }
        }
    }
    
    // Método para eliminar prefijos inválidos en las rutas de recursos
    private string RemoveInvalidPathPrefix(string path)
    {
        // Eliminar "Assets/" o "Assets/Resources/" si se incluyeron por error
        if (path.StartsWith("Assets/Resources/"))
        {
            path = path.Substring("Assets/Resources/".Length);
            if (showDebugMessages)
                Debug.LogWarning($"Se ha eliminado 'Assets/Resources/' de la ruta. La ruta correcta es: '{path}'");
        }
        else if (path.StartsWith("Assets/"))
        {
            path = path.Substring("Assets/".Length);
            if (showDebugMessages)
                Debug.LogWarning($"Se ha eliminado 'Assets/' de la ruta. La ruta correcta es: '{path}'");
        }
        
        // También eliminar "Resources/" si se incluyó por error
        if (path.StartsWith("Resources/"))
        {
            path = path.Substring("Resources/".Length);
            if (showDebugMessages)
                Debug.LogWarning($"Se ha eliminado 'Resources/' de la ruta. La ruta correcta es: '{path}'");
        }
        
        return path;
    }
    
    private IEnumerator AttemptLoadResources<T>(string resourcePath) where T : Object
    {
        bool loaded = false;
        int attempts = 0;
        
        while (!loaded && attempts < maxLoadAttempts)
        {
            attempts++;
            
            if (showDebugMessages)
                Debug.Log($"Intentando cargar recursos de tipo {typeof(T).Name} desde '{resourcePath}' (Intento {attempts}/{maxLoadAttempts})");
            
            // Cargar recursos de forma segura
            T[] resources = LoadResourcesSafely<T>(resourcePath, out bool success, out string errorMessage);
            
            if (success && resources != null && resources.Length > 0)
            {
                foreach (T resource in resources)
                {
                    if (resource != null)
                    {
                        preloadedResources.Add(resource);
                        if (showDebugMessages)
                            Debug.Log($"  - Recurso cargado: {resource.name}");
                    }
                }
                
                loaded = true;
                Debug.Log($"Recursos cargados con éxito: {resourcePath} ({resources.Length} elementos)");
            }
            else
            {
                string warningMsg = $"No se pudieron cargar recursos en la ruta: '{resourcePath}'. ";
                if (!string.IsNullOrEmpty(errorMessage))
                    warningMsg += errorMessage + " ";
                warningMsg += $"Intento {attempts}/{maxLoadAttempts}";
                
                Debug.LogWarning(warningMsg);
                yield return new WaitForSeconds(timeBetweenAttempts);
            }
        }
        
        if (!loaded)
        {
            // Intentar cargar recursos genéricos como último recurso
            if (typeof(T) != typeof(Object))
            {
                Debug.Log($"Intentando cargar como recursos genéricos desde '{resourcePath}'");
                Object[] genericResources = Resources.LoadAll(resourcePath);
                
                if (genericResources != null && genericResources.Length > 0)
                {
                    foreach (Object resource in genericResources)
                    {
                        if (resource != null)
                        {
                            preloadedResources.Add(resource);
                            Debug.Log($"  - Recurso genérico cargado: {resource.name} (Tipo: {resource.GetType().Name})");
                        }
                    }
                    Debug.Log($"Recursos genéricos cargados: {genericResources.Length}");
                }
                else
                {
                    Debug.LogError($"No se pudieron cargar los recursos después de {maxLoadAttempts} intentos ni como genéricos: '{resourcePath}'");
                }
            }
            else
            {
                Debug.LogError($"No se pudieron cargar los recursos después de {maxLoadAttempts} intentos: '{resourcePath}'");
            }
        }
    }
    
    // Método auxiliar que maneja la carga de recursos con try-catch de forma segura
    private T[] LoadResourcesSafely<T>(string resourcePath, out bool success, out string errorMessage) where T : Object
    {
        success = false;
        errorMessage = string.Empty;
        T[] resources = null;
        
        try
        {
            resources = Resources.LoadAll<T>(resourcePath);
            success = (resources != null);
            
            if (resources != null && resources.Length == 0 && showDebugMessages)
            {
                Debug.Log($"La ruta '{resourcePath}' existe pero no contiene recursos de tipo {typeof(T).Name}");
            }
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
        }
        
        return resources;
    }
    
    private void Update()
    {
        // Mantener la actualización suave del slider si no está al valor actual
        if (sliderProgress != null && sliderProgress.value < currentPercent)
        {
            sliderProgress.value = Mathf.MoveTowards(sliderProgress.value, currentPercent, 25 * Time.deltaTime);
        }
    }
    
    private void OnDestroy()
    {
        // Liberar los recursos precargados manualmente cuando se destruye este objeto
        if (preloadedResources != null)
        {
            foreach (Object resource in preloadedResources)
            {
                if (resource != null)
                {
                    Resources.UnloadAsset(resource);
                }
            }
            
            preloadedResources.Clear();
        }
    }
}