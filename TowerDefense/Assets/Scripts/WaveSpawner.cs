using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine.SceneManagement;






public class WaveSpawner : MonoBehaviour
{
    [HideInInspector] public bool isPlanning = true;
    public static int EnemiesAlive = 0;

    [Header("Wave Configuration")]
    public Wave[] waves;
    public Transform spawnPoint;

    [Header("Between-Wave Settings")]
    public float betweenWaves = 5f;

    [Header("Pre-Wave CountDown")]
    public float preWaveTime = 30f;
    public Button readyButton;

    [Header("Randomizer")]
    public bool randomizeEnemyOrder = false;

    [Header("UI References")]
    public Text waveCountdownText;
    
    public event Action<int> OnWaveStart;
    public event Action OnAllWavesComplete;

    private int waveIndex = 0;
    private float countDown;
    private bool awaitingPreWave = false;

    void Start()
    {
        waveCountdownText.text = "";
        readyButton.gameObject.SetActive(false);
    }

    public void EndPlanning()
    {
        isPlanning = false;
        BeginPreWave();      // force the first wave to start immediately
    }

    private void BeginPreWave()
    {
        awaitingPreWave = true;
        countDown = preWaveTime;

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(() => countDown = 0f);
        readyButton.gameObject.SetActive(true);
    }

    void Update ()
    {

        // don't proceed if still planning, or enemies are alive, or no more waves
        if (isPlanning || EnemiesAlive > 0 || waveIndex >= waves.Length)
            return;

        if (awaitingPreWave)
        {
            // count down
            countDown -= Time.deltaTime;
            countDown = Mathf.Max(countDown, 0f);
            waveCountdownText.text = string.Format("{0:00.00}", countDown);

            if (countDown <= 0f)
            {
                // hide button, start wave
                awaitingPreWave = false;
                readyButton.gameObject.SetActive(false);
                StartCoroutine(SpawnWave());
            }
        }

        // ← block all spawning until planning is done
        // if (isPlanning) 
        // { 
        //    return; 
        //}

        // if (EnemiesAlive > 0)
        // {
        //     return;
        // }

        // if (waveIndex >= waves.Length) //new
        // {
        //     return;
        // }

        // if (countDown <= 0f) 
        // {
        //    StartCoroutine(SpawnWave());
        //    countDown = betweenWaves;
        //     return;
        // }

        // countDown -= Time.deltaTime;

        // countDown = Mathf.Clamp(countDown, 0f, Mathf.Infinity);

        // waveCountdownText.text = string.Format("{0:00.00}", countDown);
    }

    IEnumerator SpawnWave ()
    {
        OnWaveStart?.Invoke(waveIndex);
        PlayerStats.Rounds++;

        Wave wave = waves[waveIndex];

        //Create spawn Queue
        List<GameObject> spawnQueue = new List<GameObject>();
        foreach (var group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
            {
                spawnQueue.Add(group.enemy);
            }
        }

        if (randomizeEnemyOrder)
        {
            for(int i = spawnQueue.Count - 1; i >= 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var tmp = spawnQueue[i];
                spawnQueue[i] = spawnQueue[j];
                spawnQueue[j] = tmp;
            }
        }

        foreach (var enemyPrefab in spawnQueue)
        {
                Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                EnemiesAlive++;
                yield return new WaitForSeconds(1f / wave.spawnRate);
        }

        waveIndex++;

        if (waveIndex < waves.Length)
        {
            BeginPreWave();
        }
        else {
            // Debug.Log("WaveSpawner: All waves processed! Firing OnAllWavesComplete. Current EnemiesAlive: " + EnemiesAlive); // <-- ADD THIS LINE
            // GameManager.instance.HandleLevelComplete();
            // OnAllWavesComplete?.Invoke();
            //this.enabled = false;
            Debug.Log("WaveSpawner: All waves processed! Waiting for all enemies to die. EnemiesAlive = " + EnemiesAlive);
            StartCoroutine(WaitForAllEnemiesToDie());

            //int currentIndex = GetActiveScene();
            //int nextIndex = currentIndex + 1;

            //GameManager.instance.LoadLevel(nextIndex);
            //GameManager.instance.UnloadLevel(currentIndex);
        }
      
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


        //Best working code was used last before new prewave stuff
        // for (int z = 0; z < wave.enemies.Length; z++)
        //{
        //   for (int i = 0; i < wave.enemies[z].count; i++)
        //   {
        //       SpawnEnemy(wave.enemies[z].enemy);
        //      yield return new WaitForSeconds(1f / wave.spawnRate);
        //  }
        //  if (waveIndex == waves.Length)
        //  {
        //     Debug.Log("TODO - End Level");
        //      this.enabled = false;
        // }
        // }
    }

    /// <summary>
    /// After the last wave spawns, wait until all enemies have been destroyed
    /// before telling the GameManager to transition levels.
    /// </summary>
    private IEnumerator WaitForAllEnemiesToDie()
    {
        // loop every frame until no enemies remain
        while (EnemiesAlive > 0)
            yield return null;

        Debug.Log("WaveSpawner: All enemies dead. Completing level.");
        GameManager.instance.HandleLevelComplete();
        OnAllWavesComplete?.Invoke();
        this.enabled = false;
    }

    void SpawnEnemy (GameObject enemyPrefab)
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemiesAlive++;
    }
}
