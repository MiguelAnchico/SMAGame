using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damagePerSecond = 10;
    public float damageInterval = 0.5f;  // Daño cada medio segundo

    private List<IDamageable> enemiesInZone = new List<IDamageable>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !enemiesInZone.Contains(damageable))
        {
            enemiesInZone.Add(damageable);
            StartCoroutine(DamageOverTime(damageable));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && enemiesInZone.Contains(damageable))
        {
            enemiesInZone.Remove(damageable);
        }
    }

    private IEnumerator DamageOverTime(IDamageable enemy)
    {
        while (enemiesInZone.Contains(enemy))
        {
            enemy.TakeDamage(damagePerSecond);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}

