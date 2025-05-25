using UnityEngine;

public class TaskPanelToggleButton : MonoBehaviour
{
    private TaskUIManager taskUIManager;

    void Start()
    {
        // Buscar el TaskUIManager automáticamente
        FindTaskUIManager();
    }

    private void FindTaskUIManager()
    {
        // Buscar TaskUIManager en la escena
        taskUIManager = FindObjectOfType<TaskUIManager>();
        
        if (taskUIManager == null)
        {
            Debug.LogWarning("TaskUIManager no encontrado en la escena");
        }
        else
        {
            Debug.Log("TaskUIManager encontrado automáticamente");
        }
    }

    // Método público para llamar desde el botón
    public void ToggleTaskPanel()
    {
        // Si no se encontró, intentar buscar nuevamente
        if (taskUIManager == null)
        {
            FindTaskUIManager();
        }

        // Ejecutar la función si existe
        if (taskUIManager != null)
        {
            taskUIManager.ToggleTaskPanel();
            Debug.Log("Panel de tareas alternado");
        }
        else
        {
            Debug.LogError("No se pudo encontrar TaskUIManager para alternar el panel");
        }
    }
}