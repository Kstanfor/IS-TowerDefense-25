using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;



public class WaveSpawner : MonoBehaviour
{
    public static int EnemiesAlive = 0;

    public Wave[] waves;

    public Transform spawnPoint;

    public float betweenWaves = 5f;
    private float countDown = 2f;

    public Text waveCountdownText;

    private int waveIndex = 0;

    void Update ()
    {
        if (EnemiesAlive > 0)
        {
            return;
        }

        if (waveIndex >= waves.Length) //new
        {
            return;
        }

        if (countDown <= 0f) 
        {
            StartCoroutine(SpawnWave());
            countDown = betweenWaves;
            return;
        }

        countDown -= Time.deltaTime;

        countDown = Mathf.Clamp(countDown, 0f, Mathf.Infinity);

        waveCountdownText.text = string.Format("{0:00.00}", countDown);
    }

    public event Action<int> OnWaveStart;

    IEnumerator SpawnWave ()
    {
        OnWaveStart?.Invoke(waveIndex);
        PlayerStats.Rounds++;

        Wave wave = waves[waveIndex];

        //  for (int i = 0; i < wave.enemyCount; i++)
        //  {
        //     SpawnEnemy(wave.enemyPrefab);
        //     yield return new WaitForSeconds(1f / wave.spawnRate);
        //  }

        //  waveIndex++;
        //  Debug.Log("Wave Incoming!!!");

        // if (waveIndex == waves.Length)
        // {
        //    Debug.Log("LEVEL WON!");
        //   this.enabled = false;
        //}

        for (int z = 0; z < wave.enemies.Length; z++)
        {
            for (int i = 0; i < wave.enemies[z].count; i++)
            {
                SpawnEnemy(wave.enemies[z].enemy);
                yield return new WaitForSeconds(1f / wave.spawnRate);
            }
            if (waveIndex == waves.Length)
            {
                Debug.Log("TODO - End Level");
                this.enabled = false;
            }
        }
        waveIndex++;
    }

    void SpawnEnemy (GameObject enemyPrefab)
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemiesAlive++;
    }
}
