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
        float rem = GameManager.instance.RemainingTime;
        int m = Mathf.FloorToInt(rem / 60f);
        int s = Mathf.FloorToInt(rem % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", m, s);
    }

    public void Toggle()
    {
        ui.SetActive(!ui.activeSelf);

        if (ui.activeSelf)
        {
            if (timerText != null)
            {
                UpdateTimerText();
            }
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
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
