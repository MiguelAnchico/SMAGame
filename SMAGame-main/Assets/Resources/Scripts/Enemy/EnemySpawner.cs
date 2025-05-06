using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int totalEnemies = 5;
    public float spawnInterval = 2f;
    public Transform[] spawnPoints; // lugares donde pueden aparecer

    private int enemiesSpawned = 0;
    private float timer = 0f;

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
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemiesSpawned++;
    }
}
