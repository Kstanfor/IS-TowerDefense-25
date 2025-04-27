// WorkerIDInput.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class WorkerIDInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InputField workerIDInputField;  // drag your InputField here
    [SerializeField] private Button submitButton;            // drag your Submit Button here

    private void Start()
    {
        // Ensure we have a GameManager instance
        if (GameManager.instance == null)
        {
            Debug.LogError("[WorkerIDInput] No GameManager found in scene!");
            submitButton.interactable = false;
            return;
        }

        // Wire up the button click
        submitButton.onClick.AddListener(SubmitWorkerID);
    }

    /// <summary>
    /// Reads the text, validates it, tells GameManager, and proceeds.
    /// </summary>
    private void SubmitWorkerID()
    {
        string id = workerIDInputField.text.Trim();

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[WorkerIDInput] Worker ID is empty—please enter a valid ID.");
            // Optionally show on-screen error feedback here
            return;
        }

        // Store the ID in your singleton GameManager
        GameManager.instance.SetWorkerID(id);

        // Load your next scene (replace "MainMenu" with whatever comes next)
        SceneManager.LoadScene("MainMenu");
    }
}
