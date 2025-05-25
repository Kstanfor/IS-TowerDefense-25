using UnityEngine.SceneManagement;
using UnityEngine;
using System.Globalization;
using UnityEngine.UI;



public class PauseMenu : MonoBehaviour
{
    public GameObject ui;

    public Text timerText;

    public string levelToLoad = "MainMenu";

    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (!GameManager.GameIsOver)
            {
                Toggle();
            }
        }

        if (ui.activeSelf && timerText != null) 
        {
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null || GameManager.instance == null) return;

        var gm = GameManager.instance;

        if (gm.RemainingTime <= 0f)
        {
            timerText.text = "Time: 00:00"; ;
        }
        else
        {
            float rem = gm.RemainingTime;
            int m = Mathf.FloorToInt(rem / 60f);
            int s = Mathf.FloorToInt(rem % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", m, s);
        }
        timerText.text += $"\nLevels Completed: {gm.levelsCompleted}";

    }

    public void Toggle()
    {
        if (GameManager.GameIsOver) return;

        ui.SetActive(!ui.activeSelf);
        Debug.Log($"[PauseMenu.Toggle] UI active: {ui.activeSelf}"); // Log UI state

        if (ui.activeSelf)
        {
            UpdateTimerText(); // Update text when pause menu is shown
            Time.timeScale = 0f;
            Debug.Log("[PauseMenu.Toggle] Time.timeScale SET TO 0f");
            if (GameManager.instance != null) GameManager.instance.StartLevelPauseTracking();
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseMenu.Toggle] Time.timeScale SET TO 1f"); // Log change
            if (GameManager.instance != null) GameManager.instance.EndLevelPauseTracking();
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        //Toggle();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        SceneManager.LoadScene(levelToLoad);
        Debug.Log("Go To Menu.");
    }
}
