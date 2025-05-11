using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
    private Transform target;
    private int wavepointIndex = 0;

    private Enemy enemy;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        target = Waypoints.points[0];

        enemy.speed = enemy.startSpeed;
    }

    void Update()
    {
        if (Mathf.Abs(enemy.speed - enemy.startSpeed) > 0.01f)
            Debug.Log($"[EnemyMovement] current speed = {enemy.speed}");

        Vector3 direction = target.position - transform.position;
        transform.Translate(direction.normalized * enemy.speed * GameManager.instance.DifficultyModifier * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWaypoint();
        }

        //enemy.speed = enemy.startSpeed;
    }

    void GetNextWaypoint()
    {
        if (wavepointIndex >= Waypoints.points.Length - 1)
        {
            EndPath();
            return;
        }

        wavepointIndex++;
        target = Waypoints.points[wavepointIndex];
    }



    void EndPath()
    {
        PlayerStats.Lives--;
        WaveSpawner.EnemiesAlive--;

        if (GameManager.instance != null)
        {
            Debug.Log("[EnemyMovement] EndPath() called → DecreaseDifficulty next");
            GameManager.instance.DecreaseDifficulty();
        }

        Destroy(gameObject);
    }
}
