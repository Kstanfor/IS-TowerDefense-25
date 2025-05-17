using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;


public class GameOver : MonoBehaviour
{
    public Text roundsText;

    void OnEnable ()
    {
        if (GameManager.instance == null)
        {
            roundsText.text = "Error: GameManager not found.";
            Debug.LogError("[GameOver] GameManager.instance is null on OnEnable!");
            return;
        }

        int done = GameManager.instance.levelsCompleted;
        int goal = GameManager.instance.maxLevels; // This now correctly refers to the total unique levels

        if (goal <= 0) // Basic check for valid goal
        {
            Debug.LogWarning("[GameOver] GameManager.maxLevels is not set to a positive value. Displaying basic completion message.");
            roundsText.text = $"{done} levels completed.";
            return;
        }

        if (done >= goal) // This means all unique levels were completed
        {
            roundsText.text = "Thank you for playing!";
            // You could also make this more specific, e.g.:
            // roundsText.text = $"Congratulations! You completed all {goal} levels!";
        }
        else // Game ended before completing all unique levels (e.g., timer, lives out)
        {
            roundsText.text = $"{done}/{goal} levels completed";
        }

    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        Debug.Log("Go To Menu");
    }
}
