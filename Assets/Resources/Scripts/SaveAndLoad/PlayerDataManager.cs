using UnityEngine;
using TigerForge;


/// <summary>
/// Ejemplo simplificado de componente que implementa ISaveable para guardar datos básicos del jugador
/// </summary>
public class PlayerDataManager : MonoBehaviour, ISaveable
{
    // Datos del jugador que queremos persistir (simplificados)
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private Vector3 lastPosition = Vector3.zero;
    
    // Implementación de la propiedad SaveID de la interfaz ISaveable
    public string SaveID => "PlayerData";
    
    private void Start()
    {
        // Cargar datos automáticamente al iniciar
        if (GameManagerLoader.Instance != null && GameManagerLoader.Instance.SaveFileExists())
        {
            GameManagerLoader.Instance.LoadSpecificComponent(SaveID);
        }
    }
    
    // Actualizar la posición del jugador antes de guardar
    private void UpdatePlayerPosition()
    {
        if (transform != null)
        {
            lastPosition = transform.position;
        }
    }
    
    // Implementación del método SaveData de la interfaz ISaveable
    public void SaveData(EasyFileSave saveFile)
    {
        UpdatePlayerPosition();
        
        // Guardar los datos básicos del jugador
        string prefix = $"{SaveID}_";
        
        saveFile.Add($"{prefix}Level", playerLevel);
        saveFile.Add($"{prefix}Health", playerHealth);
        saveFile.Add($"{prefix}Position", lastPosition);
        
        Debug.Log($"PlayerDataManager: Datos básicos guardados. Nivel: {playerLevel}, Salud: {playerHealth}");
    }
    
    // Implementación del método LoadData de la interfaz ISaveable
    public void LoadData(EasyFileSave saveFile)
    {
        string prefix = $"{SaveID}_";
        
        // Cargar datos básicos con valores por defecto en caso de no encontrarse
        playerLevel = saveFile.GetInt($"{prefix}Level", playerLevel);
        playerHealth = saveFile.GetFloat($"{prefix}Health", playerHealth);
        lastPosition = saveFile.GetUnityVector3($"{prefix}Position", lastPosition);
        
        // Actualizar la posición del jugador si es necesario
        if (transform != null)
        {
            transform.position = lastPosition;
        }
        
        Debug.Log($"PlayerDataManager: Datos básicos cargados. Nivel: {playerLevel}, Salud: {playerHealth}");
    }
    
    // Métodos públicos para ser utilizados por otros componentes
    
    public void LevelUp()
    {
        playerLevel++;
        Debug.Log($"PlayerDataManager: Nivel aumentado a {playerLevel}");
        
        // Guardar automáticamente al subir de nivel
        if (GameManagerLoader.Instance != null)
        {
            GameManagerLoader.Instance.SaveSpecificComponent(SaveID);
        }
    }
    
    public void SetHealth(float newHealth)
    {
        playerHealth = newHealth;
        Debug.Log($"PlayerDataManager: Salud actualizada a {playerHealth}");
        
        // Opcional: guardar automáticamente al cambiar la salud
        // if (GameManagerLoader.Instance != null)
        // {
        //     GameManagerLoader.Instance.SaveSpecificComponent(SaveID);
        // }
    }
    
    public float GetHealth()
    {
        return playerHealth;
    }
    
    public int GetLevel()
    {
        return playerLevel;
    }
    
    public Vector3 GetLastSavedPosition()
    {
        return lastPosition;
    }
}