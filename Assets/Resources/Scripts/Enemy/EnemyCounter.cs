using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyCounter : MonoBehaviour
{
    [Header("Container Settings")]
    public Transform enemyContainer; // Container donde están los enemigos
    public EnemySpawner enemySpawner; // Referencia al spawner de enemigos
    
    [Header("Counter Settings")]
    public float checkInterval = 0.5f; // Cada cuánto revisar el conteo
    public bool startCountingImmediately = true;
    
    [Header("Events")]
    public UnityEvent OnAllEnemiesDefeated; // Evento cuando no quedan enemigos Y no hay más por spawnear
    public UnityEvent<int> OnEnemyCountChanged; // Evento cuando cambia el número (pasa cantidad actual)
    
    private int currentEnemyCount = 0;
    private int lastEnemyCount = -1;
    private bool isCountingActive = false;
    private bool allEnemiesDefeatedEventFired = false;

    void Start()
    {
        // Buscar spawner automáticamente si no está asignado
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }
        
        if (startCountingImmediately)
        {
            StartCounting();
        }
    }

    public void StartCounting()
    {
        if (enemyContainer == null)
        {
            Debug.LogWarning("Enemy Container no asignado en EnemyCounter");
            return;
        }
        
        isCountingActive = true;
        allEnemiesDefeatedEventFired = false;
        StartCoroutine(CountEnemies());
    }

    public void StopCounting()
    {
        isCountingActive = false;
        StopAllCoroutines();
    }

    private IEnumerator CountEnemies()
    {
        while (isCountingActive)
        {
            if (enemyContainer != null)
            {
                // Contar enemigos activos (no destroyed)
                currentEnemyCount = 0;
                
                foreach (Transform child in enemyContainer)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        currentEnemyCount++;
                    }
                }

                // Si el conteo cambió, disparar evento
                if (currentEnemyCount != lastEnemyCount)
                {
                    OnEnemyCountChanged?.Invoke(currentEnemyCount);
                    Debug.Log($"Enemigos restantes: {currentEnemyCount}");
                    lastEnemyCount = currentEnemyCount;
                }

                // Verificar si todos los enemigos están derrotados Y no hay más por spawnear
                bool noMoreEnemiesLeft = (currentEnemyCount == 0);
                bool spawnerFinished = (enemySpawner == null || enemySpawner.IsSpawningComplete());
                
                if (noMoreEnemiesLeft && spawnerFinished && !allEnemiesDefeatedEventFired)
                {
                    allEnemiesDefeatedEventFired = true;
                    Debug.Log("¡Todos los enemigos han sido derrotados y no quedan más por spawnear!");
                    OnAllEnemiesDefeated?.Invoke();
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // Métodos públicos para control externo
    public int GetCurrentEnemyCount()
    {
        return currentEnemyCount;
    }

    public bool AreAllEnemiesDefeated()
    {
        bool noEnemiesLeft = (currentEnemyCount == 0);
        bool spawnerFinished = (enemySpawner == null || enemySpawner.IsSpawningComplete());
        
        return noEnemiesLeft && spawnerFinished && allEnemiesDefeatedEventFired;
    }

    public void ResetCounter()
    {
        allEnemiesDefeatedEventFired = false;
        lastEnemyCount = -1;
        currentEnemyCount = 0;
    }

    public void SetEnemyContainer(Transform container)
    {
        enemyContainer = container;
    }

    public void SetEnemySpawner(EnemySpawner spawner)
    {
        enemySpawner = spawner;
    }

    // Método para vincular automáticamente con EnemySpawner
    public void LinkWithSpawner(EnemySpawner spawner)
    {
        if (spawner != null)
        {
            enemyContainer = spawner.GetEnemyContainer();
            enemySpawner = spawner;
            Debug.Log("EnemyCounter vinculado con EnemySpawner");
        }
    }

    // Método para obtener información completa del estado
    public string GetStatusInfo()
    {
        int remainingToSpawn = enemySpawner != null ? enemySpawner.GetRemainingEnemies() : 0;
        return $"Enemigos activos: {currentEnemyCount}, Por spawnear: {remainingToSpawn}";
    }
}