// WorkerIDInput.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class WorkerIDInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InputField workerIDInputField;  // drag your InputField here
    [SerializeField] private Button submitButton;            // drag your Submit Button here

    [Header("Scene Camera")]
    [SerializeField] private Camera startSceneCamera;


    private void Start()
    {

        // Ensure we have a GameManager instance
        if (GameManager.instance == null)
        {
            Debug.LogError("[WorkerIDInput] No GameManager found in scene!");
            submitButton.interactable = false;
            return;
        }

        if (startSceneCamera == null)
        {
            startSceneCamera = Camera.main;
            if (startSceneCamera == null)
            {
                Debug.LogWarning("[WorkerIDInput] Main Camera not found in the scene. Please assign it in the Inspector.");
            }
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
        GameManager.instance.LoadTutorialLevel("TutorialLevel");

        if (startSceneCamera != null)
        {
            startSceneCamera.gameObject.SetActive(false);
            Debug.Log("[WorkerIDInput] Start scene camera disabled.");
        }
        else
        {
            Debug.LogWarning("[WorkerIDInput] Could not disable start scene camera because it's not assigned or found.");
        }

    }
}
