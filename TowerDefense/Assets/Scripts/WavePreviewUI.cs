using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class WavePreviewUI : MonoBehaviour
{
    [Header("References")]
    public WaveSpawner spawner;
    public Text totalUnitTypesText;
    public Text currentWaveTypesText;
    public Text nextWaveTypesText;

    void Start()
    {
        bool showPreview = GameManager.instance.uiMode == UIMode.PlanningAndPreview
                        || GameManager.instance.uiMode == UIMode.PreviewOnly;

        if (!showPreview)
        {
            // disable the entire preview panel
            gameObject.SetActive(false);
            return;
        }

        // 1) Show total distinct unit types at level start
        totalUnitTypesText.text = $"Total Unit Types: {CalculateDistinctTypes()}";

        // 2) Subscribe to wave-start events
        spawner.OnWaveStart += OnWaveStart;

        // 3) Initialize the preview for the very first wave
        UpdatePreview(0);
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnWaveStart -= OnWaveStart;
        }
    }

    // Event handler: updates when a new wave actually begins
    void OnWaveStart(int waveIndex)
    {
        UpdatePreview(waveIndex);
    }

    // Calculate how many distinct enemy prefabs appear across all waves
    int CalculateDistinctTypes()
    {
        var set = new HashSet<GameObject>();
        foreach (var wave in spawner.waves)
            foreach (var group in wave.enemies)
                set.Add(group.enemy);
        return set.Count;
    }

    // Update both Current and Next counts
    void UpdatePreview(int currentIndex)
    {
        // --- Current Wave ---
        if (currentIndex < spawner.waves.Length)
            currentWaveTypesText.text = FormatWaveGroupList(
                "Current Wave", spawner.waves[currentIndex].enemies);
        else
            currentWaveTypesText.text = "Current Wave:\n—";

        // --- Next Wave ---
        int nextIndex = currentIndex + 1;
        if (nextIndex < spawner.waves.Length)
            nextWaveTypesText.text = FormatWaveGroupList(
                "Next Wave", spawner.waves[nextIndex].enemies);
        else
            nextWaveTypesText.text = "Next Wave:\n—";
    }

    string FormatWaveGroupList(string title, Wave.WaveGroup[] groups)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{title}:");
        foreach (var g in groups)
        {
            // Use the prefab’s name; you can override this by adding a `displayName` field if you like
            sb.AppendLine($"• {g.enemy.name}: {g.count}");
        }
        return sb.ToString();
    }
}
