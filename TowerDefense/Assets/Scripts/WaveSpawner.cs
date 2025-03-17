using UnityEngine;
using System.Collections;


public class WaveSpawner : MonoBehaviour
{
    public Transform enemyPrefab;

    public Transform spawnPoint;

    public float betweenWaves = 5f;
    private float countDown = 2f;

    private int waveIndex = 0;

    void Update ()
    {
        if (countDown <= 0f) 
        {
            StartCoroutine(SpawnWave());
            countDown = betweenWaves;
        }

        countDown -= Time.deltaTime;
    }

    IEnumerator SpawnWave ()
    {
        waveIndex++;

        for (int i = 0; i < waveIndex; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Wave Incoming!!!");
    }

    void SpawnEnemy ()
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
