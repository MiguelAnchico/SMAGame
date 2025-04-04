using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TigerForge;
using UnityEngine.SceneManagement;

/// <summary>
/// Interfaz que deben implementar todos los componentes que quieran persistir datos
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Método que se llama cuando se requiere guardar datos
    /// </summary>
    /// <param name="saveFile">Instancia de EasyFileSave que se utilizará para guardar</param>
    void SaveData(EasyFileSave saveFile);
    
    /// <summary>
    /// Método que se llama cuando se requiere cargar datos
    /// </summary>
    /// <param name="saveFile">Instancia de EasyFileSave que se utilizará para cargar</param>
    void LoadData(EasyFileSave saveFile);
    
    /// <summary>
    /// Identificador único del componente para el guardado
    /// </summary>
    string SaveID { get; }
}

/// <summary>
/// GameManagerLoader que coordina el guardado y carga de todos los componentes hijos
/// que implementan ISaveable
/// </summary>
public class GameManagerLoader : MonoBehaviour
{
    [Tooltip("Nombre del archivo de guardado")]
    [SerializeField] private string saveFileName = "game_data";
    
    [Tooltip("Contraseña para encriptar el archivo (opcional)")]
    [SerializeField] private string encryptionPassword = "";
    
    [Tooltip("Si es verdadero, busca componentes ISaveable en la escena completa, no solo en hijos")]
    [SerializeField] private bool searchInEntireScene = false;
    
    [Tooltip("Si es verdadero, recarga la escena completa al cargar un juego guardado")]
    [SerializeField] private bool reloadSceneOnLoad = true;
    
    [Tooltip("Tiempo de espera después de cargar la escena (en segundos)")]
    [SerializeField] private float waitTimeAfterSceneLoad = 0.2f;
    
    // La lista de componentes que implementan ISaveable
    private List<ISaveable> saveableComponents = new List<ISaveable>();
    
    // Singleton para acceder desde cualquier parte
    private static GameManagerLoader _instance;
    public static GameManagerLoader Instance
    {
        get { return _instance; }
    }
    
    // Variable para rastrear si estamos en proceso de cargar juego después de recargar escena
    private bool isLoadingAfterSceneReload = false;
    
    private void Awake()
    {
        // Configuración del singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Inicializar la lista de componentes
        RefreshSaveableComponents();
        
        // Suscribirse al evento de escena cargada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento al destruir el objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Manejador para el evento de carga de escena
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Refrescar componentes cada vez que se carga una escena
        RefreshSaveableComponents();
    }
    
    /// <summary>
    /// Refresca la lista de componentes que implementan ISaveable
    /// </summary>
    public void RefreshSaveableComponents()
    {
        saveableComponents.Clear();
        
        if (searchInEntireScene)
        {
            // Buscar en toda la escena
            var components = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            saveableComponents.AddRange(components);
        }
        else
        {
            // Buscar solo en los hijos de este objeto
            var components = GetComponentsInChildren<MonoBehaviour>().OfType<ISaveable>();
            saveableComponents.AddRange(components);
        }
        
        Debug.Log($"GameManagerLoader: Se encontraron {saveableComponents.Count} componentes para guardar/cargar");
    }
    
    /// <summary>
    /// Guarda los datos de todos los componentes ISaveable
    /// </summary>
    /// <returns>True si el guardado fue exitoso</returns>
    public bool SaveGame()
    {
        Debug.Log("GameManagerLoader: Iniciando guardado de juego...");
        
        // Verificar si hay componentes para guardar
        if (saveableComponents.Count == 0)
        {
            Debug.LogWarning("GameManagerLoader: No hay componentes para guardar.");
            RefreshSaveableComponents();
        }
        
        // Crear instancia de EasyFileSave
        var saveFile = new EasyFileSave(saveFileName);
        
        // Guardar la escena actual para saber dónde recargar
        string currentSceneName = SceneManager.GetActiveScene().name;
        saveFile.Add("CurrentScene", currentSceneName);
        
        // Llamar al método SaveData de cada componente
        foreach (var component in saveableComponents)
        {
            try
            {
                component.SaveData(saveFile);
                Debug.Log($"GameManagerLoader: Guardado componente {component.SaveID}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameManagerLoader: Error al guardar componente {component.SaveID}: {e.Message}");
            }
        }
        
        // Guardar el archivo
        bool success;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            success = saveFile.Save();
        }
        else
        {
            success = saveFile.Save(encryptionPassword);
        }
        
        // Liberar recursos
        saveFile.Dispose();
        
        Debug.Log($"GameManagerLoader: Guardado de juego {(success ? "completado con éxito" : "fallido")}");
        return success;
    }
    
    /// <summary>
    /// Carga los datos de todos los componentes ISaveable
    /// </summary>
    /// <returns>True si la carga fue exitosa o se inició correctamente</returns>
    public bool LoadGame()
    {
        // Verificar si el archivo existe antes de intentar cargarlo
        if (!SaveFileExists())
        {
            Debug.LogWarning("GameManagerLoader: No existe archivo de guardado para cargar.");
            return false;
        }
        
        // Si está configurado para recargar la escena y no estamos ya en proceso de carga
        if (reloadSceneOnLoad && !isLoadingAfterSceneReload)
        {
            Debug.Log("GameManagerLoader: Iniciando carga de juego con recarga de escena...");
            
            // Cargar archivo para verificar la escena guardada
            var saveFile = new EasyFileSave(saveFileName);
            
            bool loadCheck;
            if (string.IsNullOrEmpty(encryptionPassword))
            {
                loadCheck = saveFile.Load();
            }
            else
            {
                loadCheck = saveFile.Load(encryptionPassword);
            }
            
            string sceneToLoad = SceneManager.GetActiveScene().name; // Por defecto, la escena actual
            
            if (loadCheck)
            {
                // Obtener la escena guardada, si existe
                string savedScene = saveFile.GetString("CurrentScene", sceneToLoad);
                sceneToLoad = savedScene;
            }
            
            saveFile.Dispose();
            
            // Configurar para ejecutar la carga después de que la escena se recargue
            StartCoroutine(LoadGameAfterSceneReload(sceneToLoad));
            
            return true;
        }
        else
        {
            // Carga directa sin recargar escena (o estamos en el proceso de carga post-recarga)
            return LoadGameDirect();
        }
    }
    
    /// <summary>
    /// Corrutina que espera a que se recargue la escena y luego carga los datos guardados
    /// </summary>
    private IEnumerator LoadGameAfterSceneReload(string sceneName)
    {
        isLoadingAfterSceneReload = true;
        
        // Recargar la escena 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Esperar a que la escena termine de cargar
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Dar un pequeño tiempo para que los objetos se inicialicen
        yield return new WaitForSeconds(waitTimeAfterSceneLoad);
        
        // Cargar los datos
        LoadGameDirect();
        
        isLoadingAfterSceneReload = false;
    }
    
    /// <summary>
    /// Realiza la carga directa de los datos sin recargar la escena
    /// </summary>
    /// <returns>True si la carga fue exitosa</returns>
    private bool LoadGameDirect()
    {
        Debug.Log("GameManagerLoader: Cargando datos de juego guardado...");
        
        // Verificar si hay componentes para cargar
        if (saveableComponents.Count == 0)
        {
            Debug.LogWarning("GameManagerLoader: No hay componentes para cargar.");
            RefreshSaveableComponents();
        }
        
        // Crear instancia de EasyFileSave
        var saveFile = new EasyFileSave(saveFileName);
        
        // Intentar cargar el archivo
        bool loadSuccess;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            loadSuccess = saveFile.Load();
        }
        else
        {
            loadSuccess = saveFile.Load(encryptionPassword);
        }
        
        // Si la carga fue exitosa, cargar los datos de cada componente
        if (loadSuccess)
        {
            foreach (var component in saveableComponents)
            {
                try
                {
                    component.LoadData(saveFile);
                    Debug.Log($"GameManagerLoader: Cargado componente {component.SaveID}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"GameManagerLoader: Error al cargar componente {component.SaveID}: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogWarning("GameManagerLoader: No se pudo cargar el archivo de guardado.");
        }
        
        // Liberar recursos
        saveFile.Dispose();
        
        Debug.Log($"GameManagerLoader: Carga de juego {(loadSuccess ? "completada con éxito" : "fallida")}");
        return loadSuccess;
    }
    
    /// <summary>
    /// Verifica si existe un archivo de guardado
    /// </summary>
    /// <returns>True si existe un archivo de guardado</returns>
    public bool SaveFileExists()
    {
        var saveFile = new EasyFileSave(saveFileName);
        bool exists = saveFile.Load();
        saveFile.Dispose();
        return exists;
    }
    
    /// <summary>
    /// Guarda un componente específico por su SaveID
    /// </summary>
    /// <param name="saveID">El ID del componente a guardar</param>
    /// <returns>True si el guardado fue exitoso</returns>
    public bool SaveSpecificComponent(string saveID)
    {
        var component = saveableComponents.FirstOrDefault(c => c.SaveID == saveID);
        if (component == null)
        {
            Debug.LogError($"GameManagerLoader: No se encontró componente con SaveID: {saveID}");
            return false;
        }
        
        var saveFile = new EasyFileSave(saveFileName);
        
        // Cargar primero para mantener otros datos intactos
        bool exists = false;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            exists = saveFile.Load();
        }
        else
        {
            exists = saveFile.Load(encryptionPassword);
        }
        
        // Si no existe un archivo previo, guardar la escena actual
        if (!exists)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            saveFile.Add("CurrentScene", currentSceneName);
        }
        
        // Guardar el componente específico
        try
        {
            component.SaveData(saveFile);
            Debug.Log($"GameManagerLoader: Guardado componente específico {component.SaveID}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameManagerLoader: Error al guardar componente {component.SaveID}: {e.Message}");
            saveFile.Dispose();
            return false;
        }
        
        // Guardar el archivo
        bool success;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            success = saveFile.Save();
        }
        else
        {
            success = saveFile.Save(encryptionPassword);
        }
        
        // Liberar recursos
        saveFile.Dispose();
        
        return success;
    }
    
    /// <summary>
    /// Carga un componente específico por su SaveID
    /// </summary>
    /// <param name="saveID">El ID del componente a cargar</param>
    /// <returns>True si la carga fue exitosa</returns>
    public bool LoadSpecificComponent(string saveID)
    {
        var component = saveableComponents.FirstOrDefault(c => c.SaveID == saveID);
        if (component == null)
        {
            Debug.LogError($"GameManagerLoader: No se encontró componente con SaveID: {saveID}");
            return false;
        }
        
        var saveFile = new EasyFileSave(saveFileName);
        
        // Intentar cargar el archivo
        bool loadSuccess;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            loadSuccess = saveFile.Load();
        }
        else
        {
            loadSuccess = saveFile.Load(encryptionPassword);
        }
        
        // Si la carga fue exitosa, cargar el componente específico
        if (loadSuccess)
        {
            try
            {
                component.LoadData(saveFile);
                Debug.Log($"GameManagerLoader: Cargado componente específico {component.SaveID}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameManagerLoader: Error al cargar componente {component.SaveID}: {e.Message}");
                saveFile.Dispose();
                return false;
            }
        }
        else
        {
            Debug.LogWarning("GameManagerLoader: No se pudo cargar el archivo de guardado.");
        }

        // Liberar recursos
        saveFile.Dispose();
        
        return loadSuccess;
    }
}