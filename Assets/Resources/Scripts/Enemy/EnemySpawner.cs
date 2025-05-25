using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int totalEnemies = 5;
    public float spawnInterval = 2f;
    public Transform[] spawnPoints; // lugares donde pueden aparecer
    
    [Header("Container")]
    public Transform enemyContainer; // Objeto padre para los enemigos

    private int enemiesSpawned = 0;
    private float timer = 0f;

    void Start()
    {
        // Si no se asignó un container, crear uno automáticamente
        if (enemyContainer == null)
        {
            GameObject container = new GameObject("Enemy Container");
            container.transform.parent = transform;
            enemyContainer = container.transform;
        }
    }

    void Update()
    {
        if (enemiesSpawned >= totalEnemies) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        
        // Hacer que el enemigo sea hijo del container
        newEnemy.transform.parent = enemyContainer;
        
        enemiesSpawned++;
    }

    // Métodos públicos para obtener información
    public Transform GetEnemyContainer()
    {
        return enemyContainer;
    }

    public bool IsSpawningComplete()
    {
        return enemiesSpawned >= totalEnemies;
    }

    public int GetRemainingEnemies()
    {
        return Mathf.Max(0, totalEnemies - enemiesSpawned);
    }

    public int GetSpawnedEnemies()
    {
        return enemiesSpawned;
    }

    public int GetTotalEnemies()
    {
        return totalEnemies;
    }
}