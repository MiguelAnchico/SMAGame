using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float projectileSpeed = 10f;
    private float shootTimer;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        shootTimer = 0f;
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f && player != null)
        {
            ShootAtPlayer();
            shootTimer = shootCooldown;
        }
    }

    void ShootAtPlayer()
    {
        Vector3 shootDirection = (player.position - transform.position).normalized;

        Vector3 spawnPos = transform.position + (shootDirection * 0.5f); // offset spawn forward
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectileScript = projectile.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.SetDirection(shootDirection, projectileSpeed);
        }
        else
        {
            Debug.LogWarning("[EnemyAttack] No Projectile script found on prefab!");
        }
    }
}
