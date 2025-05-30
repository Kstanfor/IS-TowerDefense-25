﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.ComponentModel.Design;




[System.Serializable]
public class LevelStats
{
    public string levelName;
    public int livesLost_level;
    public float pauseTime_level; // Pause time accumulated during this level
    public int enemiesKilled_level;
    public int turretsPurchased_level;
    public bool complete;
    public int goldEarned_level;
    public int goldSpent_level;
    public float playTime_level; // Active play time for this level

    public LevelStats(string name)
    {
        levelName = name;
        livesLost_level = 0;
        pauseTime_level = 0f;
        enemiesKilled_level = 0;
        turretsPurchased_level = 0;
        complete = false;
        goldEarned_level = 0;
        goldSpent_level = 0;
        playTime_level = 0f;
    }
}


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

    [Header("Datamining Stats - Global")]
    public float totalPauseTime = 0f;
    // public float totalPlayTime; // Existing 'elapsedTime' can serve this if it tracks unpaused time
    public int totalEnemiesKilled = 0;
    public int turretsPurchasedTotal = 0;
    public int healthLossTotal = 0; // Total lives lost across all levels
    public int totalGoldEarned = 0;
    public int totalGoldSpent = 0;
    // 'levelsCompleted' is already tracked
    // 'WorkerID' is already tracked
    // 'uiMode' is already tracked

    [Header("Datamining Stats - Per Level")]
    public List<LevelStats> allLevelStats = new List<LevelStats>();
    private LevelStats currentLevelStats;
    private float currentLevelStartTime; // Time.time when level began
    private int livesAtLevelStart;
    private float currentLevelPauseStartTime; // Time.unscaledTime when pause begins for current level
    private bool isCurrentlyPausedForDataTracking = false;
    private int previousPlayerLives; // For tracking health loss


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
    //[Tooltip("How many levels to play before forcing end")]
    //public int maxLevels = 5;
    [Tooltip("How many seconds to play before forcing end (30 min = 1800 s)")]
    public float maxTime = 1800f;
    public float elapsedTime;

     [Header("New End-Game & Loop Settings")]
    [Tooltip("Minimum total levels completed for the game to end when maxTime is reached.")]
    public int minLevelsForTimerEndCondition = 5;

    [Tooltip("The total number of unique game levels in a sequence (e.g., if you have Level01 to Level05, this is 5). Set this accurately in the Inspector.")]
    public int uniqueLevelsInSequence = 10; // EXAMPLE: Set this to how many actual unique level scenes you have

    private int currentLevelInSequenceIndex;

    [Tooltip("Minimum total levels that must be played to enable looping after all unique levels are complete.")]
    public int minLevelsPlayedForLooping = 5;


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

            // Initialize global datamining stats that are not reset per level
            totalPauseTime = 0f;
            totalEnemiesKilled = 0;
            turretsPurchasedTotal = 0;
            healthLossTotal = 0;
            totalGoldEarned = 0;
            totalGoldSpent = 0;
            previousPlayerLives = PlayerStats.Lives; // Initialize with starting lives

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

        if (scene.name == "TutorialLevel") // Replace "TutorialLevel" if your scene name is different
        {
            Debug.Log($"[GameManager] OnSceneLoaded: Tutorial scene '{scene.name}' has loaded.");
            // TutorialManager should handle its own WaveSpawner setup.
            // We might not need to find the WaveSpawner for the GameManager here,
            // as the TutorialManager uses its own reference.
            // However, ensure PauseMenu and GameOverUI are handled if they exist in the tutorial scene.
            pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null) pauseMenu.ui.SetActive(false);
            else Debug.LogWarning($"[GameManager] PauseMenu not found in Tutorial scene: {scene.name}");

            gameOverUI = GameObject.FindGameObjectWithTag("GameOverUI");
            if (gameOverUI != null) gameOverUI.SetActive(false);
            // Note: The tutorial scene typically wouldn't trigger a "game over" via GameManager.

            // Skip formal LevelStats initialization for the tutorial if not needed
        }
        else if (scene.name.StartsWith("Level0"))
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

            // Find the GameOverUI in the loaded scene
            gameOverUI = GameObject.FindGameObjectWithTag("GameOverUI");

            if (gameOverUI != null)
            {
                gameOverUI.SetActive(false);
            }
            else 
            {
                Debug.LogError("[GameManager] gameOverUI wasn’t wired in the inspector or found in scene!");
            }

            Debug.Log($"[GameManager_Data] Initializing stats for level: {scene.name}");
            currentLevelStats = new LevelStats(scene.name);
            allLevelStats.Add(currentLevelStats);

            currentLevelStartTime = Time.time;
            livesAtLevelStart = PlayerStats.Lives; //
            previousPlayerLives = PlayerStats.Lives; // Reset for the new level, to correctly track lives lost *within* this level
                                                     // any other level-specific temporary trackers could be reset here
                                                     // For example, ensure isCurrentlyPausedForDataTracking is false if it's level-specific.
            isCurrentlyPausedForDataTracking = false;

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

   // void Start()
   // {
     //   if (instance != this) return;
        //waveSpawner?.EndPlanning();
   // }

    void Update()
    {
        Debug.Log($"[GameManager.Update] START - GameIsOver: {GameIsOver}, Time.timeScale: {Time.timeScale}");
        if (GameIsOver)
        {
            return;
        }

        // Accumulate play time for the current level if it's active and game is not paused
        if (currentLevelStats != null && Time.timeScale > 0f)
        {
            currentLevelStats.playTime_level += Time.deltaTime;
        }

        // Track health loss
        if (PlayerStats.Lives < previousPlayerLives)
        {
            int livesLostThisFrame = previousPlayerLives - PlayerStats.Lives;
            RecordHealthLost(livesLostThisFrame);
        }
        previousPlayerLives = PlayerStats.Lives; // Update for the next frame

        elapsedTime += Time.deltaTime;
        afkTimer += Time.deltaTime;
       // Debug.Log($"[Timer] elapsed={elapsedTime:F1}s  afk={afkTimer:F1}s");
        Debug.Log($"[GameManager.Update] Current Time.timeScale: {Time.timeScale}");

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            afkTimer = 0f;
        }

        if (afkTimer >= afkTimeLimit && pauseMenu != null && !pauseMenu.ui.activeSelf && !GameIsOver)
        {
            pauseMenu.Toggle();
        }

        if (elapsedTime >= maxTime && levelsCompleted >= minLevelsForTimerEndCondition && !GameIsOver)
        {
            TriggerEndGame(true);
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

    private void TriggerEndGame(bool loadEndScene)
    {
        if (GameIsOver) return; // Prevent multiple calls
        GameIsOver = true;
        if (loadEndScene)
        {
            Debug.Log("[GameManager] TriggerEndGame: Conditions met for EndScene transition. Loading EndScene.");
            Time.timeScale = 0f; // Stop game time
            SceneManager.LoadScene("EndScene"); // Ensure "EndScene" is in your Build Settings
        }
        else
        {
            Debug.Log("[GameManager] TriggerEndGame: General game over (not specific EndScene condition).");
            // This will be handled by EndGame(false) or other logic if needed.
            // For now, if TriggerEndGame is called without loadEndScene true, it means game over by other means
            // that should use the standard EndGame() logic.
            EndGame(); // Default to standard game over if not specifically for EndScene
        }
    }

    void EndGame()
    {
        if (GameIsOver) return; // Prevent multiple calls
        GameIsOver = true;
        gameOverUI.transform.SetAsLastSibling();
        Debug.LogWarning("[GameManager] EndGame: GAME OVER! - GameIsOver set to true.");
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogError("[GameManager] EndGame: gameOverUI is null!");
        }
        Time.timeScale = 0f;
    }

    public void HandleLevelComplete()
    {
        if (GameIsOver) return; // Don't process if game already ended

        Debug.Log("GameManager: HandleLevelComplete CALLED.");

        if (currentLevelStats != null)
        {
            currentLevelStats.complete = true;
            // playTime_level is already accumulated in Update.
            // livesLost_level is accumulated by RecordHealthLost.
            // Other stats (enemies, turrets, gold) are accumulated by their respective methods.
            Debug.Log($"[GameManager_Data] Finalized stats for {currentLevelStats.levelName}: PlayTime={currentLevelStats.playTime_level}, LivesLost={currentLevelStats.livesLost_level}, GoldEarned={currentLevelStats.goldEarned_level}");
        }

        levelsCompleted++; // Increment total levels completed
        currentLevelInSequenceIndex++; // Increment progress in the current unique sequence

        Debug.Log($"GameManager: Total levelsCompleted is now {levelsCompleted}. Current level in sequence index is {currentLevelInSequenceIndex} (out of {uniqueLevelsInSequence}).");

        // Check if a full sequence of unique levels has been completed
        if (currentLevelInSequenceIndex >= uniqueLevelsInSequence)
        {
            Debug.Log($"[GameManager.HandleLevelComplete] Completed a full sequence of {uniqueLevelsInSequence} unique levels (total played: {levelsCompleted}). Checking loop conditions.");

            // Conditions for looping:
            // 1. Timer not done (elapsedTime < maxTime)
            // 2. Minimum total levels played for looping met (levelsCompleted >= minLevelsPlayedForLooping)
            if (elapsedTime < maxTime && levelsCompleted >= minLevelsPlayedForLooping)
            {
                Debug.Log($"[GameManager.HandleLevelComplete] LOOP CONDITIONS MET: elapsedTime ({elapsedTime:F1}s < {maxTime}s) AND total levelsCompleted ({levelsCompleted} >= {minLevelsPlayedForLooping}). Resetting sequence.");
                currentLevelInSequenceIndex = 0; // Reset for the new loop
                string firstLevelInSequence = "Level01"; // Assuming levels always start/loop to Level01
                Debug.Log($"[GameManager.HandleLevelComplete] Looping back to {firstLevelInSequence}.");
                UnloadCurrentLevelAndLoadLevel(firstLevelInSequence);
            }
            else
            {
                Debug.Log($"[GameManager.HandleLevelComplete] Loop conditions NOT met. elapsedTime: {elapsedTime:F1}s (maxTime: {maxTime}s), levelsCompleted: {levelsCompleted} (minForLooping: {minLevelsPlayedForLooping}). Triggering End Game.");
                TriggerEndGame(true); // End the game if loop conditions are not met after sequence completion
            }
        }
        else
        {

            if (elapsedTime >= maxTime && levelsCompleted >= minLevelsForTimerEndCondition)
            {
                Debug.Log($"[GameManager.HandleLevelComplete] Time up condition met after level completion. Triggering End Game for EndScene.");
                TriggerEndGame(true); // Time up + levels condition met
            }
            // Proceed to the next level in the current sequence
            // currentLevelInSequenceIndex is 1-based for the *next* level number after increment.
            // E.g., if currentLevelInSequenceIndex is 1, it means Level01 was just completed, next is Level02.
            else
            {
                string nextSequentialLevelName = $"Level{(currentLevelInSequenceIndex + 1).ToString("D2")}"; // e.g. Level02, Level10
                Debug.Log($"[GameManager.HandleLevelComplete] Proceeding to next sequential level in sequence: {nextSequentialLevelName}");
                UnloadCurrentLevelAndLoadLevel(nextSequentialLevelName);
            }
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
        if (string.IsNullOrEmpty(CurrentLevelName))
        {
            Debug.LogWarning("[GameManager] UnloadCurrentLevel: CurrentLevelName is empty, nothing to unload.");
            return;
        }
        Debug.Log($"[GameManager] UnloadCurrentLevel: Attempting to unload '{CurrentLevelName}'.");
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

    public void LoadTutorialLevel(string tutorialSceneName)
    {
        if (workerIDCanvas != null)
        {
            workerIDCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("[GameManager] WorkerIDCanvas not found, cannot disable.");
        }

        Debug.Log($"[GameManager] LoadTutorialLevel: Loading {tutorialSceneName}.");
        CurrentLevelName = tutorialSceneName; // Set CurrentLevelName so it can be unloaded later
        LoadLevel(tutorialSceneName);         // LoadLevel is additive
    }

    // --- Method to be called by TutorialManager to start the main game ---
    public void StartFirstRegularLevel()
    {
        Debug.Log("[GameManager] StartFirstRegularLevel: Transitioning from Tutorial to Level01.");

        // Reset progression indices if the tutorial shouldn't count towards main game level sequence
        currentLevelInSequenceIndex = 0;
        levelsCompleted = 0; // Consider if tutorial completion should count. If not, uncomment.
        // If tutorial completion *is* the first level, then this should not be 0.
        // Based on current setup, TutorialManager handles its own wave completion,
        // so levelsCompleted (GameManager's) isn't incremented by the tutorial.
        // Thus, Level01 will be the first to increment it.

        // Unload the tutorial level and load the first regular level
        UnloadCurrentLevelAndLoadLevel("Level01");
    }

    public void StartLevelPauseTracking()
    {
        if (!isCurrentlyPausedForDataTracking && currentLevelStats != null)
        {
            currentLevelPauseStartTime = Time.unscaledTime; // Use unscaledTime as Time.timeScale will be 0
            isCurrentlyPausedForDataTracking = true;
        }
    }

    public void EndLevelPauseTracking()
    {
        if (isCurrentlyPausedForDataTracking && currentLevelStats != null)
        {
            float pauseDuration = Time.unscaledTime - currentLevelPauseStartTime;
            totalPauseTime += pauseDuration;
            currentLevelStats.pauseTime_level += pauseDuration;
            isCurrentlyPausedForDataTracking = false;
            Debug.Log($"[GameManager_Data] Level pause ended. Duration: {pauseDuration}. LevelTotalPause: {currentLevelStats.pauseTime_level}. GameTotalPause: {totalPauseTime}");
        }
    }

    public void RecordEnemyKilled(int goldValueFromEnemy)
    {
        totalEnemiesKilled++;
        totalGoldEarned += goldValueFromEnemy; // Assuming enemy value is direct gold earned

        if (currentLevelStats != null)
        {
            currentLevelStats.enemiesKilled_level++;
            currentLevelStats.goldEarned_level += goldValueFromEnemy;
        }
    }

    public void RecordTurretPurchased(int cost)
    {
        turretsPurchasedTotal++;
        totalGoldSpent += cost;

        if (currentLevelStats != null)
        {
            currentLevelStats.turretsPurchased_level++;
            currentLevelStats.goldSpent_level += cost;
        }
    }

    public void RecordHealthLost(int amount)
    {
        healthLossTotal += amount;
        if (currentLevelStats != null)
        {
            currentLevelStats.livesLost_level += amount;
        }
    }

}





