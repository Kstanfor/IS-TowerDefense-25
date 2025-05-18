using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataMiner : MonoBehaviour
{
    public static DataMiner dataMiner;

    List<KeyValuePair<int, int>> LevelLog = new List<KeyValuePair<int, int>>();
    public List<string> LevelLogStrings = new List<string>();
    string logString = "";
    StreamWriter file;
    int LevelDeaths = 0;
    int LevelKills = 0;
    int totalDeaths = 0;
    int levelDeaths = 0;
    int totalKills = 0;
    string turretTypes;
    KeyValuePair<int, int> currentTime;
    string adjustedTimeStamp = "";
    public int numberOfLogableLevels = 0;
    public static string workerID = "";
    bool dataHasBeenSent = false;
    private static int levelsCompleted;

    public Dictionary<int, int> NPC_Conversations = new Dictionary<int, int>();
    public List<Vector2> positionLog;


    private void Awake()
    {
        dataMiner = GameObject.Find("DataMiner").GetComponent<DataMiner>();
    }

    public void AddDeathToLog()
    {
        totalDeaths++;
        levelDeaths++;
    }

    public void AddKillToLog()
    {
        totalKills++;
    }

    public void LogLevelCompletion() //call after every level finish
    {
        //AdjustedTimeStamp(ref LevelLog);

        string LevelString = adjustedTimeStamp + ",";
        LevelString += LevelDeaths + ",";
        LevelString += LevelKills + ",";
        List<float> difficultyLevels = new List<float>();
        //LevelString += GameManager.gameManager.GetDifficultyAverage(difficultyLevels) + ",";
        //LevelString += completed + ",";
        //if (completed) { LevelCompleted++; }

        LevelLogStrings.Add(LevelString);
        LevelDeaths = 0;
        LevelKills = 0;
    }

    public void LogData()
    {
        string positionLogString = workerID + "," + DateTime.Now + ",";
        foreach (Vector2 pos in positionLog)
        {
            int tempX = Mathf.RoundToInt(pos.x);
            int tempY = Mathf.RoundToInt(pos.y);
            positionLogString += tempX + ":" + tempY + "|";
        }

        Debug.Log(positionLogString);

        if (!dataHasBeenSent)
        {
            dataHasBeenSent = true;
            logString += workerID + "," + DateTime.Now + "," + GameManager.instance.uiMode.ToString() + ",";
            List<KeyValuePair<int, int>> blank = new List<KeyValuePair<int, int>>();
            //AdjustTimeStamp(ref blank);

            logString += levelsCompleted + ",";

            //For all Levels incomplete, Adds default values in their place
           // if (LevelLogStrings.Count < numberOfLogableLevels)
           // {
                //LogLevelCompletion(false);
           //     while (LevelLogStrings.Count < numberOfLogableLevels)
             //   {
             //       LevelLogStrings.Add(-1 + "," + -1 + "," + -1 + "," + -1 + "," + "False, ");
              //  }
           // }

            //Adds Level data to final log string
            foreach (string s in LevelLogStrings) { logString += s; }

            StartCoroutine(WriteTextViaPHP(logString, "https://gamesux.com/fromunity_elementalbarrage.php"));
            //StartCoroutine(WriteTextViaPHP(positionLogString, "https://gamesux.com/fromunity_elementalbarrage_location.php"));
        }


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
        Debug.Log("Logable Levels: " + numberOfLogableLevels);
    }

    // Update is called once per frame
    void Update()
    {

    }
}


