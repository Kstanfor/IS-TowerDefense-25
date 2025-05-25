using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TestDataMiner : MonoBehaviour
{
    public void TriggerDataMine()
    {
        if (DataMiner.instance != null)
        {
            Debug.Log("TestDataMinerButton: Triggering DataMiner.CollectAndSendData()...");
            DataMiner.instance.CollectAndSendData();
        }
        else
        {
            Debug.LogError("TestDataMinerButton: DataMiner instance not found! Make sure DataMiner is active in the scene.");
        }
    }
}
