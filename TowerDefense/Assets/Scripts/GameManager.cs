using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEngine;
using System.ComponentModel.Design;






//version stuff
public enum UIMode
    {
        PlanningAndPreview,  // show both panels
        PreviewOnly,         // skip planning, show preview
        PlanningOnly,        // show planning, skip preview
        None                 // skip both
    }

public class GameManager : MonoBehaviour
{
    private WaveSpawner waveSpawner;

    // somewhere in the class, expose it in the inspector:
    public UIMode uiMode = UIMode.PlanningAndPreview;

    [Header("Dynamic Difficulty Settings")]
    [Tooltip("Current multiplier applied to all enemy speeds")]
    public float difficultyModifier = 1f;
    public float difficultyIncrease = 0.01f;
    public float difficultyDecrease = 0.1f;
    public float minDifficulty = 0.5f;
    public float maxDifficulty = 2f;

    public float DifficultyModifier => difficultyModifier;

    [Header("End-Game Settings")]
    [Tooltip("How many levels to play before forcing end")]
    public int maxLevels = 5;
    [Tooltip("How many seconds to play before forcing end (30 min = 1800 s)")]
    public float maxTime = 1800f;

    [Header("AFK Pause Settings")]
    [Tooltip("Seconds of no input before auto-pause")]
    public float afkTimeLimit = 15f;
    public PauseMenu pauseMenu;    // assign in inspector or auto-find

    private float elapsedTime;
    private float afkTimer;
    public int levelsCompleted { get; private set; }

    // gameover stuff
    public static bool GameIsOver { get; private set; }

    public GameObject gameOverUI;

    // ─── NEW: store the mTurk Worker ID ─────────────────────────────
    private string workerID = string.Empty;
    public string WorkerID => workerID;            // read-only public accessor

    /// <summary>
    /// Sets the Worker ID (call once, from your login screen).
    /// </summary>
    public void SetWorkerID(string id)
    {
        workerID = id;
        Debug.Log("[GameManager] Worker ID set to: " + workerID);
    }
    // ────────────────────────────────────────────────────────────────

    public void IncreaseDifficulty()
    {
        Debug.Log("[GameManager] IncreaseDifficulty() entered");
        difficultyModifier = Mathf.Clamp(difficultyModifier + difficultyIncrease, minDifficulty, maxDifficulty);
        Debug.Log($"[GameManager] Difficulty ↑ to {difficultyModifier:F2}");
    }

    public void DecreaseDifficulty()
    {
        Debug.Log("[GameManager] DecreaseDifficulty() entered");
        difficultyModifier = Mathf.Clamp(difficultyModifier - difficultyIncrease, minDifficulty, maxDifficulty);
        Debug.Log($"[GameManager] Difficulty ↓ to {difficultyModifier:F2}");
    }

    void Start()
    {
        GameIsOver = false;
        elapsedTime = 0f;
        afkTimer = 0f;
        levelsCompleted = 0;

        waveSpawner?.EndPlanning();
    }

    void Update()
    {
        if (GameIsOver)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        afkTimer += Time.deltaTime;

        if (Input.anyKeyDown)
        {
            afkTimer = 0f;
        }

        if (afkTimer >= afkTimeLimit && pauseMenu != null && !pauseMenu.ui.activeSelf)
        {
            pauseMenu.Toggle();
        }

        if (elapsedTime >= maxTime || levelsCompleted >= maxLevels)
        {
            TriggerEndGame();
        }

        if (Input.GetKeyDown("e"))
        {
            EndGame();
        }
            
       if(PlayerStats.Lives <= 0)
        {
            EndGame();  
        } 
    }

    private void TriggerEndGame()
    {
        GameIsOver = true;
        Debug.Log("Game Over!");
        gameOverUI.SetActive(true);
    }

    void EndGame()
    {
        GameIsOver = true;
        Debug.Log("Game Over!");

        gameOverUI.SetActive(true);
    }

    //possible singleton stuff
    public string CurrentLevelName = string.Empty;

    public static GameManager instance; //{ get; private set; }

    private void Awake()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();

        if (instance == null)
        {
            instance = this;
            //make sure this game manager persists across scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("Trying to instantiate a second" +
                "instance of singleton Game Manager");
        }
        Debug.Log($"[GameManager] Awake: instance set to {instance.name}");

        if (waveSpawner != null)
        {
            waveSpawner.OnAllWavesComplete += HandleLevelComplete;
        }

        if (pauseMenu == null)
        {
            pauseMenu = FindObjectOfType<PauseMenu>();
        }
    }

    private void HandleLevelComplete()
    {
        levelsCompleted++;

        if (levelsCompleted < maxLevels)
        {
            // load next level here (or reload same scene, etc.)
            LoadLevel("Level0" + (levelsCompleted + 1));
            waveSpawner.enabled = true;
            waveSpawner.EndPlanning();
        } else
        {
            TriggerEndGame();
        }
    }

    public void LoadLevel(string levelName)
    {
        levelsCompleted++;

        Time.timeScale = 1f;
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }
        CurrentLevelName = levelName;
    }

    public void UnloadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }
    }

    public void UnloadCurrentLevel()
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(CurrentLevelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + CurrentLevelName);
            return;
        }
    }

    public void UnloadCurrentLevelAndLoadLevel(string levelName)
    {
        StartCoroutine(UnloadCurrentLevelAndLoadLevelCoroutine(levelName));
    }

    public IEnumerator UnloadCurrentLevelAndLoadLevelCoroutine(string levelName)
    {
        //Time.timeScale = 1;

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        AsyncOperation ao = SceneManager.UnloadSceneAsync(CurrentLevelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + CurrentLevelName);
            yield return null;
        }

        while (!ao.isDone)
        {
            yield return new WaitForSeconds(0.1f);
        }

        LoadLevel(levelName);
    }

    

}
