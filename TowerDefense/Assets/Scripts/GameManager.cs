using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.ComponentModel.Design;




//version stuff - Study Design
public enum UIMode
    {
        PlanningAndPreview,  // show both panels
        PreviewOnly,         // skip planning, show preview
        PlanningOnly,        // show planning, skip preview
        None                 // skip both
    }


public class GameManager : MonoBehaviour
{
    //SINGLETON & STATE
    public static GameManager instance; //{ get; private set; }
    //possible singleton stuff
    public string CurrentLevelName = string.Empty;
    // gameover stuff
    public static bool GameIsOver { get; private set; }
    public int levelsCompleted { get; private set; }


    //REFERENCES
    private WaveSpawner waveSpawner;
    public GameObject workerIDCanvas; // Reference to the canvas
    public GameObject gameOverUI;
    public PauseMenu pauseMenu;    // assign in inspector or auto-find


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

    [Header("Study Design UI Mode")]
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

    [Header("End-Game Settings")]
    [Tooltip("How many levels to play before forcing end")]
    public int maxLevels = 5;
    [Tooltip("How many seconds to play before forcing end (30 min = 1800 s)")]
    public float maxTime = 1800f;
    private float elapsedTime;

     [Header("New End-Game & Loop Settings")]
    [Tooltip("Minimum total levels completed for the game to end when maxTime is reached.")]
    public int minLevelsForTimerEndCondition = 5;

    //[Tooltip("The total number of unique game levels available (e.g., if you have Level01 to Level05, this is 5). Set this accurately in the Inspector.")]
    //public int totalUniqueLevels = 5; // EXAMPLE: Set this to how many actual unique level scenes you have

    //[Tooltip("Minimum total levels that must be played to enable looping after all unique levels are complete.")]
    //public int minTotalLevelsForLooping = 5;


    [Header("AFK Pause Settings")]
    [Tooltip("Seconds of no input before auto-pause")]
    public float afkTimeLimit = 15f;
    private float afkTimer;

    public float RemainingTime => Mathf.Max(0f, maxTime - elapsedTime);

    // ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        Debug.Log("[GameManager] Awake CALLED. Attempting to find WaveSpawner."); // <-- ADD THIS LINE
        waveSpawner = FindObjectOfType<WaveSpawner>();

        if (instance == null)
        {
            instance = this;
            //make sure this game manager persists across scenes
            DontDestroyOnLoad(gameObject);

            GameIsOver = false;
            elapsedTime = 0f;
            afkTimer = 0f;
            levelsCompleted = 0;

            SceneManager.sceneLoaded += OnSceneLoaded; // <-- KEY CHANGE: Subscribe here
            Debug.Log($"[GameManager] Awake: Singleton instance created. Subscribed to SceneManager.sceneLoaded.");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("Trying to instantiate a second" +
                "instance of singleton Game Manager");
            return;
        }
        Debug.Log($"[GameManager] Awake: instance set to {instance.name}");

       // if (waveSpawner != null)
       // {
        //    Debug.Log($"[GameManager] Found WaveSpawner: {waveSpawner.gameObject.name} in scene {waveSpawner.gameObject.scene.name}. Subscribing to OnAllWavesComplete."); // <-- ADD THIS LINE
        //    waveSpawner.OnAllWavesComplete += HandleLevelComplete;
       // }
       // else 
       // {
        //    Debug.LogError("[GameManager] Awake: Did NOT find WaveSpawner. Level transitions will LIKELY FAIL."); // <-- ADD THIS LINE
       // }

        if (pauseMenu == null)
        {
            pauseMenu = FindObjectOfType<PauseMenu>();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] OnSceneLoaded: Scene '{scene.name}' loaded with mode '{mode}'.");
        if (scene.name.StartsWith("Level0"))
        {
            WaveSpawner newWaveSpawner = FindObjectOfType<WaveSpawner>();

            if (newWaveSpawner != null)
            {
                Debug.Log($"[GameManager] OnSceneLoaded: Found WaveSpawner '{newWaveSpawner.gameObject.name}' in scene '{scene.name}'.");

                if (waveSpawner != null && waveSpawner != newWaveSpawner)
                {
                    Debug.Log($"[GameManager] OnSceneLoaded: Unsubscribing from old WaveSpawner '{waveSpawner.gameObject.name}'.");
                    waveSpawner.OnAllWavesComplete -= HandleLevelComplete;
                }

                waveSpawner = newWaveSpawner;
                waveSpawner.OnAllWavesComplete += HandleLevelComplete;
                Debug.Log($"[GameManager] OnSceneLoaded: Subscribed to new WaveSpawner '{waveSpawner.gameObject.name}'.");

                if (waveSpawner.isPlanning)
                {
                    Debug.Log($"[GameManager] OnSceneLoaded: WaveSpawner for '{scene.name}' is in planning. Calling EndPlanning().");
                    waveSpawner.enabled = true; // Ensure it's active
                    waveSpawner.EndPlanning();

                }
                else
                {
                    Debug.Log($"[GameManager] OnSceneLoaded: WaveSpawner for '{scene.name}' is NOT in planning. (isPlanning: {waveSpawner.isPlanning})");
                }
            }
            else
            {
                Debug.LogError($"[GameManager] OnSceneLoaded: Did NOT find WaveSpawner in loaded game scene '{scene.name}'. Level transitions will FAIL.");
                // If a previous waveSpawner existed, and this new level has none, clear the old subscription.
                if (waveSpawner != null)
                {
                    waveSpawner.OnAllWavesComplete -= HandleLevelComplete;
                    waveSpawner = null; // Clear the reference as it's no longer valid for the current scene context
                }
            }

            pauseMenu = FindObjectOfType<PauseMenu>();

            if (pauseMenu == null)
            {

                Debug.LogError("[GameManager] Couldn’t find PauseMenu in the newly loaded level!");

            }
            else
            {
                Debug.Log($"[GameManager.OnSceneLoaded] Found PauseMenu. Setting UI inactive. Current ui.activeSelf: {pauseMenu.ui.activeSelf}");
                pauseMenu.ui.SetActive(false);
                Debug.Log($"[GameManager.OnSceneLoaded] After SetActive(false), ui.activeSelf: {pauseMenu.ui.activeSelf}");
            }

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(false);
            }
            else 
            {
                Debug.LogError("[GameManager] gameOverUI wasn’t wired in the inspector or found in scene!");
            }

        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe when GameManager is destroyed to prevent errors
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (waveSpawner != null)
        {
            Debug.Log($"[GameManager] OnDestroy: Unsubscribing from WaveSpawner '{waveSpawner.gameObject.name}'.");
            waveSpawner.OnAllWavesComplete -= HandleLevelComplete;
        }
    }

    void Start()
    {
        if (instance != this) return;
        //waveSpawner?.EndPlanning();
    }

    void Update()
    {
        Debug.Log($"[GameManager.Update] START - GameIsOver: {GameIsOver}, Time.timeScale: {Time.timeScale}");
        if (GameIsOver)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        afkTimer += Time.deltaTime;
        Debug.Log($"[Timer] elapsed={elapsedTime:F1}s  afk={afkTimer:F1}s");
        Debug.Log($"[GameManager.Update] Current Time.timeScale: {Time.timeScale}");

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            afkTimer = 0f;
        }

        if (afkTimer >= afkTimeLimit && pauseMenu != null && !pauseMenu.ui.activeSelf && !GameIsOver)
        {
            pauseMenu.Toggle();
        }

        if (elapsedTime >= maxTime && levelsCompleted >= maxLevels && !GameIsOver)
        {
            TriggerEndGame();
        }

        if (Input.GetKeyDown("e") && !GameIsOver)
        {
            Debug.LogWarning("[GameManager.Update] 'E' key pressed, calling EndGame()");
            EndGame();
        }
            
       if(PlayerStats.Lives <= 0 && !GameIsOver)
        {
            Debug.LogWarning($"[GameManager.Update] PlayerStats.Lives is {PlayerStats.Lives}, calling EndGame()");
            EndGame();  
        } 
    }

    private void TriggerEndGame()
    {
        if (GameIsOver) return; // Prevent multiple calls
        GameIsOver = true;
        Debug.Log("[GameManager] TriggerEndGame: GAME OVER!");
        gameOverUI.SetActive(true);
    }

    void EndGame()
    {
        if (GameIsOver) return; // Prevent multiple calls
        GameIsOver = true;
        Debug.LogWarning("[GameManager] EndGame: GAME OVER! - GameIsOver set to true.");
        gameOverUI.SetActive(true);
    }

    public void HandleLevelComplete()
    {
        Debug.Log("GameManager: HandleLevelComplete CALLED."); // <-- ADD THIS LINE
        levelsCompleted++;
        Debug.Log($"GameManager: levelsCompleted is now {levelsCompleted}. maxLevels is {maxLevels}."); // <-- ADD THIS LINE

        if (levelsCompleted >= maxLevels)
        {
            // All unique levels (as defined by maxLevels) have been completed.
            Debug.Log($"[GameManager.HandleLevelComplete] All {maxLevels} unique levels completed. Triggering End Game.");
            TriggerEndGame(); // End the game

            //waveSpawner.enabled = true;
            //waveSpawner.EndPlanning();
        } else
        {
            // There are more unique levels to play. Load the next one.
            // levelsCompleted is a 1-based count of completed levels.
            // If 1 level is completed, the next level to load is "Level02".
            string nextSequentialLevel = "Level0" + (levelsCompleted + 1);
            Debug.Log($"[GameManager.HandleLevelComplete] Proceeding to next sequential level: {nextSequentialLevel}");
            UnloadCurrentLevelAndLoadLevel(nextSequentialLevel);
        }
    }

    public void LoadLevel(string levelName)
    {
        //levelsCompleted++;

        Time.timeScale = 1f;
        Debug.Log($"[GameManager.LoadLevel] Time.timeScale SET TO 1f for level: {levelName}"); // Log change
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }
       // CurrentLevelName = levelName;
    }

    public void UnloadLevel(string levelName)
    {
        Debug.Log($"[GameManager] UnloadLevel: Unloading '{levelName}'.");
        // Before unloading, if this level contains the active waveSpawner, unsubscribe
        if (waveSpawner != null && waveSpawner.gameObject.scene.name == levelName)
        {
            Debug.Log($"[GameManager] UnloadLevel: Unsubscribing from WaveSpawner in '{levelName}' before unload.");
            waveSpawner.OnAllWavesComplete -= HandleLevelComplete;
            waveSpawner = null; // Clear the reference
        }

        AsyncOperation ao = SceneManager.UnloadSceneAsync(levelName);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to unload level " + levelName);
            return;
        }

        if (CurrentLevelName == levelName) CurrentLevelName = string.Empty; // Clear if it was the current
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

    public IEnumerator UnloadCurrentLevelAndLoadLevelCoroutine(string levelNameToLoad)
    {
        Time.timeScale = 1;
        Debug.Log($"[GameManager.Coroutine] Time.timeScale SET TO 1 before loading: {levelNameToLoad}"); // Log change

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        if (!string.IsNullOrEmpty(CurrentLevelName))
        {
            Debug.Log($"[GameManager] Coroutine: Unloading '{CurrentLevelName}'.");
            // Unsubscribe before unload, similar to UnloadLevel method
            if (waveSpawner != null && waveSpawner.gameObject.scene.name == CurrentLevelName)
            {
                Debug.Log($"[GameManager] Coroutine: Unsubscribing from WaveSpawner in '{CurrentLevelName}' before unload.");
                waveSpawner.OnAllWavesComplete -= HandleLevelComplete;
                waveSpawner = null;
            }
            AsyncOperation aoUnload = SceneManager.UnloadSceneAsync(CurrentLevelName);
            if (aoUnload == null)
            {
                Debug.LogError($"[GameManager] Coroutine: Unable to begin unloading level '{CurrentLevelName}'.");
            }
            else
            {
                while (!aoUnload.isDone)
                {
                    yield return null; // Wait a frame
                }
                Debug.Log($"[GameManager] Coroutine: Finished unloading '{CurrentLevelName}'.");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Coroutine: CurrentLevelName is empty, nothing to unload.");
        }

        CurrentLevelName = levelNameToLoad; // Set the name of the level we are about to load

        LoadLevel(levelNameToLoad);
    }

    public void LoadLevelOne()
    {
        //disable the panel
        if (workerIDCanvas != null)
        {
            workerIDCanvas.SetActive(false);
        }
        else { Debug.LogError("Canvas wont Disable"); }

        Debug.Log("[GameManager] LoadLevelOne: Loading Level01.");
        CurrentLevelName = "Level01"; // Set this so UnloadCurrentLevelAndLoadLevel works if called next

        // Load your next scene (replace "MainMenu" with whatever comes next)
        GameManager.instance.LoadLevel("Level01");
    }
    

}





