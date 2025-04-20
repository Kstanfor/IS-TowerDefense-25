using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class Enemyv2 : MonoBehaviour
{
    // ───── EXISTING FIELDS ──────────────────────────────────────────────────
    public float startSpeed = 10f;
    [HideInInspector] public float speed;
    public float startHealth = 100;
    private float health;
    public int valueOfEnemy = 25;
    public GameObject deathEffect;
    public Image healthBar;

    void Start()
    {
        speed = startSpeed;
        health = startHealth;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthBar.fillAmount = health / startHealth;
        if (health <= 0)
            Die();
    }

    public void Slow(float pct)
    {
        speed = startSpeed * (1f - pct);
    }

    // ───── NEW COROUTINES FOR STATUS EFFECTS ────────────────────────────────

    /// <summary>Burn: deals damagePerTick every interval seconds, for duration seconds.</summary>
    public IEnumerator Burn(int damagePerTick, float duration, float interval)
    {
        Debug.Log($"[Enemy] Started Burn: {damagePerTick} dmg every {interval}s for {duration}s");
        float elapsed = 0f;
        while (elapsed < duration)
        {
            TakeDamage(damagePerTick);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    /// <summary>Water slow: reduces speed by pct for duration, then restores.</summary>
    public IEnumerator SlowRoutine(float pct, float duration)
    {
        Debug.Log($"[Enemy] SlowRoutine started: pct={pct}, duration={duration}");
        //float original = speed;
        //speed = startSpeed * (1f - pct);
        //yield return new WaitForSeconds(duration);
        //speed = original;

        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            // continuously enforce the reduced speed
            speed = startSpeed * (1f - pct);
            yield return null;  // wait until next frame
        }
        speed = startSpeed;
        Debug.Log("[Enemy] SlowRoutine ended, speed restored");
    }

    /// <summary>Ice freeze: zeroes speed for duration, then restores.</summary>
    public IEnumerator FreezeRoutine(float duration)
    {
        Debug.Log($"[Enemy] FreezeRoutine started: duration={duration}");
        float original = speed;
        speed = 0f;
        yield return new WaitForSeconds(duration);
        speed = original;
    }

    void Die()
    {
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        PlayerStats.Money += valueOfEnemy;
        WaveSpawner.EnemiesAlive--;
        Destroy(effect, 5f);
        Destroy(gameObject);
    }
}
