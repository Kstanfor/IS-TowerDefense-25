using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DataMiner : MonoBehaviour
{
    public static DataMiner instance; // Changed from dataMiner for conventional singleton naming

    public int numberOfExpectedLevelEntries = 30; // e.g., if you expect up to Level30

    private bool dataHasBeenSent = false;
    private string phpPostURL = "https://gamesux.com/fromunity_elementalbarrage.php"; // Your PHP endpoint

    // Variables from original DataMiner that might be used by WriteTextViaPHP or its setup
    // string logString = ""; // This will be built by CollectAndSendData now
    // List<string> LevelLogStrings = new List<string>(); // Replaced by direct access to GameManager.allLevelStats
    // static string workerID = ""; // Will get this from GameManager

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectAndSendData()
    {
        if (dataHasBeenSent)
        {
            Debug.LogWarning("[DataMiner] Data has already been sent. Aborting.");
            return;
        }

        if (GameManager.instance == null)
        {
            Debug.LogError("[DataMiner] GameManager instance not found. Cannot log data.");
            return;
        }

        GameManager gm = GameManager.instance;
        StringBuilder sb = new StringBuilder();

        // 1. Global Game Information
        sb.Append(gm.WorkerID + ","); //
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",");
        sb.Append(gm.uiMode.ToString() + ","); //

        // 2. Global Gameplay Stats from GameManager
        sb.Append(gm.levelsCompleted + ","); //
        sb.Append(gm.totalPauseTime.ToString("F2") + ",");
        sb.Append(gm.elapsedTime.ToString("F2") + ",");
        sb.Append(gm.totalEnemiesKilled + ",");
        sb.Append(gm.turretsPurchasedTotal + ",");
        sb.Append(gm.healthLossTotal + ",");
        sb.Append(gm.totalGoldEarned + ",");
        sb.Append(gm.totalGoldSpent + ",");

        // 3. Per-Level Stats from GameManager
        int levelsLogged = 0;
        foreach (LevelStats levelStat in gm.allLevelStats)
        {
            sb.Append(levelStat.livesLost_level + ",");
            sb.Append(levelStat.pauseTime_level.ToString("F2") + ",");
            sb.Append(levelStat.enemiesKilled_level + ",");
            sb.Append(levelStat.turretsPurchased_level + ",");
            sb.Append(levelStat.complete.ToString() + ",");
            sb.Append(levelStat.goldEarned_level + ",");
            sb.Append(levelStat.goldSpent_level + ",");
            sb.Append(levelStat.playTime_level.ToString("F2") + ",");
            levelsLogged++;
        }

        // 4. Padding for Expected Level Entries (Optional)
        int statsPerLevel = 8;
        if (numberOfExpectedLevelEntries > 0)
        {
            for (int i = levelsLogged; i < numberOfExpectedLevelEntries; i++)
            {
                for (int j = 0; j < statsPerLevel; j++)
                {
                    sb.Append("-1,");
                }
            }
        }

        if (sb.Length > 0 && sb[sb.Length - 1] == ',')
        {
            sb.Length--;
        }

        string finalLogString = sb.ToString(); // This replaces the global 'logString' field for local use here
        Debug.Log("[DataMiner] Final Log String: " + finalLogString);

        // Call your original WriteTextViaPHP
        StartCoroutine(WriteTextViaPHP(finalLogString, phpPostURL));
        dataHasBeenSent = true;
    }

    IEnumerator WriteTextViaPHP(string data, string destination)
    {
        WWWForm form = new WWWForm();
        form.AddField("data", data);
        UnityWebRequest www = UnityWebRequest.Post(destination, form);
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            www.SetRequestHeader("User-Agent", "Unity 2022");
        www.SendWebRequest();
        yield return www.isDone;
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("good");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Logable Levels: " + numberOfLogableLevels);
    }

    // Update is called once per frame
    void Update()
    {

    }
}


