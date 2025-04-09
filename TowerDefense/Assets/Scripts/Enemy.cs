
using System.Collections.Specialized;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float startSpeed = 10f;
    [HideInInspector]
    public float speed;

    public float startHealth = 100;
    private float health;

    public int valueOfEnemy = 25;

    public GameObject deathEffect;

    [Header("Unity Stuff")]
    public Image healthBar;

    void Start ()
    {
        speed = startSpeed;
        health = startHealth;
    }

    public void TakeDamage (float amount)
    {
        health -= amount;

        healthBar.fillAmount = health / startHealth;

        if (health <= 0)
        {
            Die();
        }
    }

    public void Slow (float pct)
    {
        speed = startSpeed * (1f - pct);
    }

    void Die()
    {
        GameObject effect = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);

        PlayerStats.Money += valueOfEnemy;

        WaveSpawner.EnemiesAlive--;

        Destroy(effect, 5f);
        Destroy(gameObject);
    }

}
