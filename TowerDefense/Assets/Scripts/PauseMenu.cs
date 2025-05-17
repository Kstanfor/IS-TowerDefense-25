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
            Toggle();
        }

        if (ui.activeSelf && timerText != null) 
        {
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        var gm = GameManager.instance;
        if (gm.RemainingTime <= 0f && gm.levelsCompleted < gm.maxLevels)
        {
            timerText.text = $"{gm.levelsCompleted}/{gm.maxLevels} levels completed";
        }
        else
        {
            float rem = GameManager.instance.RemainingTime;
            int m = Mathf.FloorToInt(rem / 60f);
            int s = Mathf.FloorToInt(rem % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", m, s);
        }

    }

    public void Toggle()
    {
        ui.SetActive(!ui.activeSelf);
        Debug.Log($"[PauseMenu.Toggle] UI active: {ui.activeSelf}"); // Log UI state

        if (ui.activeSelf)
        {
            if (timerText != null)
            {
                UpdateTimerText();
            }
            Time.timeScale = 0f;
            Debug.Log("[PauseMenu.Toggle] Time.timeScale SET TO 0f"); // Log change
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseMenu.Toggle] Time.timeScale SET TO 1f"); // Log change
        }
    }

    public void Retry()
    {
        Toggle();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Menu()
    {
        SceneManager.LoadScene(levelToLoad);
        Debug.Log("Go To Menu.");
    }
}
