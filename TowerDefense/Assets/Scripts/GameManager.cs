using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UnityEngine;



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

    // somewhere in the class, expose it in the inspector:
    public UIMode uiMode = UIMode.PlanningAndPreview;


    // gameover stuff
    public static bool GameIsOver;

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

    void Start()
    {
        GameIsOver = false;
    }

    void Update()
    {
        if (GameIsOver)
        {
            return;
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

    void EndGame()
    {
        GameIsOver = true;
        Debug.Log("Game Over!");

        gameOverUI.SetActive(true);
    }

    //possible singleton stuff
    public string CurrentLevelName = string.Empty;

    public static GameManager instance;
    private void Awake()
    {
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
    }

    public void LoadLevel(string levelName)
    {
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
