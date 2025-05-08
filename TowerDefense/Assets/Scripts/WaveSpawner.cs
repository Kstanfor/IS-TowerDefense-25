using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;



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

    [Header("UI References")]
    public Text waveCountdownText;
    
    public event Action<int> OnWaveStart;

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

        foreach (var group in wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
            {
                Instantiate(group.enemy, spawnPoint.position, spawnPoint.rotation);
                EnemiesAlive++;
                yield return new WaitForSeconds(1f / wave.spawnRate);
            }
        }

        waveIndex++;

        if (waveIndex < waves.Length)
        {
            BeginPreWave();
        }
        else {
            this.enabled = false;
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

    void SpawnEnemy (GameObject enemyPrefab)
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        EnemiesAlive++;
    }
}
