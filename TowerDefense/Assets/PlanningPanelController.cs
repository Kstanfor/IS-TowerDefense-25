using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;



public class PlanningPanelController : MonoBehaviour
{
    [Header("References")]
    public WaveSpawner spawner;          // your GameMaster object
    public GameObject planningPanel;     // root panel GameObject
    public Text totalUnitTypesText;      // UI Text for “Total Unit Types: X”
    public Text totalEnemiesText;
    public Text enemyNamesText;          // UI Text for the list of names
    public Button continueButton;        // UI Button “Continue”

    void Start()
    {
        bool showPlanning = GameManager.instance.uiMode == UIMode.PlanningAndPreview
                         || GameManager.instance.uiMode == UIMode.PlanningOnly;

        // 1) If we shouldn’t plan, disable ourselves immediately:
        if (!showPlanning)
        {
            // let spawning start right away
            spawner.EndPlanning();
            gameObject.SetActive(false);
            return;
        }

        // 1) Pause spawning
        spawner.isPlanning = true;

        // 2) Show panel
        planningPanel.SetActive(true);

        // 3) Populate texts
        totalUnitTypesText.text = $"Total Unit Types: {CountDistinctTypes()}";
        totalEnemiesText.text = $"Total Enemies: {CountTotalEnemies()}";
        enemyNamesText.text = FormatDistinctNames();

        // 4) Wire the Continue button
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    void OnContinueClicked()
    {
        // Hide the panel
        planningPanel.SetActive(false);

        // Tell the spawner it can start now
        spawner.EndPlanning();
    }

    int CountDistinctTypes()
    {
        var set = new HashSet<GameObject>();
        foreach (var wave in spawner.waves)
            foreach (var g in wave.enemies)
                set.Add(g.enemy);
        return set.Count;
    }

    int CountTotalEnemies()
    {
        int sum = 0;
        foreach (var wave in spawner.waves)
            foreach (var g in wave.enemies)
                sum += g.count;
        return sum;
    }

    string FormatDistinctNames()
    {
        var sb = new StringBuilder("Enemies in This Level:\n");
        var seen = new HashSet<GameObject>();
        foreach (var wave in spawner.waves)
            foreach (var g in wave.enemies)
                if (seen.Add(g.enemy))
                    sb.AppendLine($"• {g.enemy.name}");
        return sb.ToString();
    }
}
