using UnityEngine;
using TigerForge;

public class PlayerDataManager : MonoBehaviour, ISaveable
{
    [SerializeField] private int playerLevel = 1;
    [SerializeField] private float playerHealth = 100f;
    [SerializeField] private Vector3 lastPosition = Vector3.zero;
    
    public string SaveID => "PlayerData";
    
    private void UpdatePlayerPosition()
    {
        if (transform != null)
        {
            lastPosition = transform.position;
        }
    }
    
    public void SaveData(EasyFileSave saveFile)
    {
        UpdatePlayerPosition();
        
        string prefix = $"{SaveID}_";
        
        saveFile.Add($"{prefix}Level", playerLevel);
        saveFile.Add($"{prefix}Health", playerHealth);
        saveFile.Add($"{prefix}Position", lastPosition);
        
        Debug.Log($"PlayerDataManager: Datos guardados. Nivel: {playerLevel}, Salud: {playerHealth}");
    }
    
    public void LoadData(EasyFileSave saveFile)
    {
        string prefix = $"{SaveID}_";
        
        playerLevel = saveFile.GetInt($"{prefix}Level", playerLevel);
        playerHealth = saveFile.GetFloat($"{prefix}Health", playerHealth);
        lastPosition = saveFile.GetUnityVector3($"{prefix}Position", lastPosition);
        
        if (transform != null)
        {
            transform.position = lastPosition;
        }
        
        Debug.Log($"PlayerDataManager: Datos cargados. Nivel: {playerLevel}, Salud: {playerHealth}");
    }
    
    public void LevelUp()
    {
        playerLevel++;
        Debug.Log($"PlayerDataManager: Nivel aumentado a {playerLevel}");
        
        if (GameManagerLoader.Instance != null)
        {
            GameManagerLoader.Instance.SaveGame();
        }
    }
    
    public void SetHealth(float newHealth)
    {
        playerHealth = newHealth;
        Debug.Log($"PlayerDataManager: Salud actualizada a {playerHealth}");
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
    
    public void SetLastPosition(Vector3 position)
    {
        lastPosition = position;
    }
}