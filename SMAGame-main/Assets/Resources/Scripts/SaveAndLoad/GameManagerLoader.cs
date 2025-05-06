using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TigerForge;
using UnityEngine.SceneManagement;

public interface ISaveable
{
    void SaveData(EasyFileSave saveFile);
    void LoadData(EasyFileSave saveFile);
    string SaveID { get; }
}

public class GameManagerLoader : MonoBehaviour
{
    [SerializeField] private string saveFileName = "game_data";
    [SerializeField] private string encryptionPassword = "";
    [SerializeField] private bool searchInEntireScene = true;
    [SerializeField] private float delayBeforeUnfreeze = 0.2f;
    
    private List<ISaveable> saveableComponents = new List<ISaveable>();
    
    private static GameManagerLoader _instance;
    public static GameManagerLoader Instance { get { return _instance; } }
    
    // Variable para controlar si la escena está congelada mientras carga
    private bool isLoadingData = false;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cargar una nueva escena, congelar temporalmente el tiempo
        Time.timeScale = 0f;
        isLoadingData = true;
        
        // Refrescar lista de componentes guardables
        RefreshSaveableComponents();
        
        // Si llegamos aquí desde una carga de juego, los datos se cargarán en ReloadSceneAndLoadData
        // Si no, verificar si hay datos guardados y cargarlos inmediatamente
        if (!isLoadingData && SaveFileExists())
        {
            LoadDataFromFile();
        }
        
        // Iniciar la corrutina para descongelar después de un pequeño retraso
        StartCoroutine(UnfreezeAfterDelay());
    }
    
    private IEnumerator UnfreezeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeUnfreeze);
        
        // Descongelar la escena
        Time.timeScale = 1f;
        isLoadingData = false;
        
        Debug.Log("GameManagerLoader: Escena descongelada, juego iniciado");
    }
    
    public void RefreshSaveableComponents()
    {
        saveableComponents.Clear();
        
        if (searchInEntireScene)
        {
            var components = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            saveableComponents.AddRange(components);
        }
        else
        {
            var components = GetComponentsInChildren<MonoBehaviour>().OfType<ISaveable>();
            saveableComponents.AddRange(components);
        }
        
        Debug.Log($"GameManagerLoader: Se encontraron {saveableComponents.Count} componentes para guardar/cargar");
    }
    
    public bool SaveGame()
    {
        Debug.Log("GameManagerLoader: Iniciando guardado de juego...");
        
        if (saveableComponents.Count == 0)
        {
            Debug.LogWarning("GameManagerLoader: No hay componentes para guardar.");
            RefreshSaveableComponents();
        }
        
        var saveFile = new EasyFileSave(saveFileName);
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        saveFile.Add("CurrentScene", currentSceneName);
        
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
        
        bool success;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            success = saveFile.Save();
        }
        else
        {
            success = saveFile.Save(encryptionPassword);
        }
        
        saveFile.Dispose();
        
        Debug.Log($"GameManagerLoader: Guardado de juego {(success ? "completado con éxito" : "fallido")}");
        return success;
    }
    
    public bool LoadGame()
    {
        Debug.Log("GameManagerLoader: Iniciando carga de juego...");
        
        if (!SaveFileExists())
        {
            Debug.LogWarning("GameManagerLoader: No existe archivo de guardado para cargar.");
            return false;
        }
        
        var saveFile = new EasyFileSave(saveFileName);
        
        bool loadSuccess;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            loadSuccess = saveFile.Load();
        }
        else
        {
            loadSuccess = saveFile.Load(encryptionPassword);
        }
        
        if (loadSuccess)
        {
            string sceneName = saveFile.GetString("CurrentScene", SceneManager.GetActiveScene().name);
            saveFile.Dispose();
            
            isLoadingData = true;
            StartCoroutine(ReloadSceneAndLoadData(sceneName));
            return true;
        }
        
        Debug.LogWarning("GameManagerLoader: No se pudo cargar el archivo de guardado.");
        saveFile.Dispose();
        return false;
    }
    
    private IEnumerator ReloadSceneAndLoadData(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        RefreshSaveableComponents();
        LoadDataFromFile();
    }
    
    private bool LoadDataFromFile()
    {
        var saveFile = new EasyFileSave(saveFileName);
        
        bool loadSuccess;
        if (string.IsNullOrEmpty(encryptionPassword))
        {
            loadSuccess = saveFile.Load();
        }
        else
        {
            loadSuccess = saveFile.Load(encryptionPassword);
        }
        
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
        
        saveFile.Dispose();
        
        Debug.Log($"GameManagerLoader: Carga de juego {(loadSuccess ? "completada con éxito" : "fallida")}");
        return loadSuccess;
    }
    
    public bool SaveFileExists()
    {
        var saveFile = new EasyFileSave(saveFileName);
        bool exists = saveFile.Load();
        saveFile.Dispose();
        return exists;
    }
}