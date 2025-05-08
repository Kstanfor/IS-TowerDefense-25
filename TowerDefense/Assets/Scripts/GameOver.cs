using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public Text roundsText;

    void OnEnable ()
    {
        int done = GameManager.instance.levelsCompleted;
        int goal = GameManager.instance.maxLevels;

        if (done >= goal)
        {
            roundsText.text = "Thank you for playing!";
        }else
        {
            roundsText.text = $"{done}/{goal} levels completed";
        }

        roundsText.text = PlayerStats.Rounds.ToString();
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
