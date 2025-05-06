using UnityEngine;

public class SaveLoadFunctions : MonoBehaviour
{
    public void SaveGame()
    {
        if (GameManagerLoader.Instance != null)
        {
            GameManagerLoader.Instance.SaveGame();
            Debug.Log("Juego guardado desde botón");
        }
    }
    
    public void LoadGame()
    {
        if (GameManagerLoader.Instance != null)
        {
            GameManagerLoader.Instance.LoadGame();
            Debug.Log("Juego cargado desde botón");
        }
    }
}