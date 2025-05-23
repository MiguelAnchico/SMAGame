using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public string sceneLoadName;

    public void LoadSceneButton()
    {
        // Carga simple y directa
        SceneManager.LoadScene(sceneLoadName);
    }
}