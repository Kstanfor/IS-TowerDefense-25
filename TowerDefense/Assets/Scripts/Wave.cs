using UnityEngine;

[System.Serializable]
public class Wave
{
    //public GameObject enemyPrefab;
    //public int enemyCount;
    //public float spawnRate;

    public float spawnRate;
    public WaveGroup[] enemies;
    [System.Serializable]
    public class WaveGroup
    {
        public GameObject enemy;
        public int count;
    }
}
