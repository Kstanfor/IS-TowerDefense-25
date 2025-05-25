using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // If using TextMeshPro for dialogue
// using UnityEngine.SceneManagement; // If you want to load another scene after tutorial

// --- Define a Structure for Each Tutorial Step ---
[System.Serializable]
public class TutorialStep
{
    [TextArea(3, 10)]
    public string dialogueText;
    public bool waitForTurretPlacement = false; // True if this step requires a turret to be placed
    public bool isFinalDialogueBeforeWave = false; // True if clicking "Continue" on this step starts the wave
    // Add other flags if needed, e.g., public GameObject objectToHighlight;
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;        // The panel holding the dialogue text and continue button
    public TMP_Text dialogueDisplay;        // The TextMeshPro UI element to show dialogue
    public Button continueButton;           // The button to advance dialogue

    [Header("Tutorial Configuration")]
    public TutorialStep[] steps;            // Array of all tutorial steps in order
    public float typingSpeed = 0.02f;       // Speed at which dialogue text appears

    [Header("Interaction References")]
    // Assign the BuildManager/Shop script that handles turret placement logic.
    // This is a simplified example; you might need a more specific reference or use static events.
    public GameObject turretShopUI;         // The UI panel for selecting turrets
    public GameObject[] turretPlacementNodes; // The nodes where turrets can be placed

    [Header("Wave Spawner Reference")]
    public WaveSpawner waveSpawner;         // Reference to the WaveSpawner in the tutorial scene
    public Wave tutorialWaveConfig;         // A 'Wave' asset defining the tutorial enemies and count

    private int currentStepIndex = 0;
    private bool isTyping = false;
    private bool waitingForAction = false;

    [Header("Post-Tutorial Settings")]
    public float delayBeforeLoadingFirstLevel = 3f;

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (turretShopUI != null) turretShopUI.SetActive(false); // Start with shop closed

        // Disable turret nodes until needed
        SetTurretNodesActive(false);

        continueButton.onClick.AddListener(OnContinueButtonClicked);

        // Disable Planning/Preview panels if they exist in the scene and are managed by PlanningPanelController
        PlanningPanelController planningPanel = FindObjectOfType<PlanningPanelController>();
        if (planningPanel != null)
        {
            planningPanel.gameObject.SetActive(false);
        }

        // --- REVISED: Disable PreviewPanel using its WavePreviewUI script ---
        WavePreviewUI wavePreviewPanelCtrl = FindObjectOfType<WavePreviewUI>();
        if (wavePreviewPanelCtrl != null)
        {
            // This will disable the GameObject that the WavePreviewUI script is attached to.
            wavePreviewPanelCtrl.gameObject.SetActive(false);
        }
        else
        {
            // Optional: Log if you expect WavePreviewUI to always be present in a scene where TutorialManager might run
            // Debug.LogWarning("[TutorialManager] WavePreviewUI component not found in the scene.");
        }
        // --- END REVISED ---

        // Ensure WaveSpawner doesn't start its normal planning phase logic if not desired
        if (waveSpawner != null)
        {
            waveSpawner.isPlanning = true; // Keep it paused initially
            waveSpawner.enabled = false;   // Keep it disabled until we manually start the tutorial wave
        }


        ShowNextStep();
    }

    void Update()
    {
        // If waiting for turret placement, check if a turret has been placed.
        // This check is a placeholder. You'll need to integrate this with your turret placement system.
        if (waitingForAction && steps[currentStepIndex].waitForTurretPlacement)
        {
            // --- INTEGRATION POINT ---
            // Example: Check if any node now has a turret.
            // This is highly dependent on your Node/BuildManager implementation.
            // For instance, if your Node script has an 'isOccupied' flag or 'turretOnNode' reference:
            bool turretWasPlaced = false;
            foreach (GameObject nodeGO in turretPlacementNodes)
            {
                // Node nodeScript = nodeGO.GetComponent<Node>(); // Assuming you have a Node script
                // if (nodeScript != null && nodeScript.turretOnNode != null)
                // {
                //     turretWasPlaced = true;
                //     break;
                // }
            }
            // A better way is to have your BuildManager or Node script call a method on TutorialManager
            // (e.g., public void NotifyTurretPlaced()) when a turret is successfully placed.

            // if (turretWasPlaced)
            // {
            //     ActionCompleted();
            // }
        }
    }


    void ShowNextStep()
    {
        if (currentStepIndex >= steps.Length)
        {
            EndTutorialDialogue();
            return;
        }

        waitingForAction = false; // Reset
        TutorialStep currentStep = steps[currentStepIndex];

        dialoguePanel.SetActive(true);
        StartCoroutine(TypeText(currentStep.dialogueText));

        if (currentStep.waitForTurretPlacement)
        {
            continueButton.gameObject.SetActive(false); // Hide continue, wait for action
            waitingForAction = true;
            // Enable turret shop and placement nodes
            if (turretShopUI != null) turretShopUI.SetActive(true);
            SetTurretNodesActive(true);
            // Provide guidance on what to do, e.g., highlight shop button
        }
        else
        {
            continueButton.gameObject.SetActive(true);
            if (turretShopUI != null) turretShopUI.SetActive(false); // Keep shop closed unless needed
            SetTurretNodesActive(false); // Keep nodes inactive unless needed
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueDisplay.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogueDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void OnContinueButtonClicked()
    {
        if (isTyping) // Don't allow skipping while typing
        {
            // Optional: StopCoroutine(TypeText(...)); dialogueDisplay.text = steps[currentStepIndex].dialogueText; isTyping = false;
            return;
        }

        TutorialStep currentStep = steps[currentStepIndex];
        currentStepIndex++;

        if (currentStep.isFinalDialogueBeforeWave)
        {
            StartTutorialWave();
        }
        else
        {
            ShowNextStep();
        }
    }

    // Call this method from your BuildManager/Shop after a turret is successfully placed during the tutorial
    public void NotifyTurretPlaced()
    {
        if (waitingForAction && steps[currentStepIndex].waitForTurretPlacement)
        {
            ActionCompleted();
        }
    }

    void ActionCompleted()
    {
        waitingForAction = false;
        if (turretShopUI != null) turretShopUI.SetActive(false); // Close shop after placement
        SetTurretNodesActive(false); // Disable nodes after placement

        // Provide feedback if desired
        // StartCoroutine(TypeText("Great! You've placed a turret."));
        // yield return new WaitForSeconds(1.5f); // Wait for feedback to be read

        currentStepIndex++;
        ShowNextStep();
    }

    void SetTurretNodesActive(bool isActive)
    {
        foreach (GameObject node in turretPlacementNodes)
        {
            // You might need to enable/disable a Collider or a specific script on the node
            // to make it clickable or not.
            // For example, if nodes have a BoxCollider:
            // BoxCollider col = node.GetComponent<BoxCollider>();
            // if (col != null) col.enabled = isActive;
            // Or, if they have a script that handles clicks:
            // NodeInteractionScript nis = node.GetComponent<NodeInteractionScript>();
            // if (nis != null) nis.enabled = isActive;
            node.SetActive(isActive); // Simplest form: activate/deactivate the node GameObject
        }
    }

    void StartTutorialWave()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("Tutorial: Starting practice wave.");

        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner not assigned in TutorialManager!");
            return;
        }
        if (tutorialWaveConfig == null)
        {
            Debug.LogError("Tutorial Wave Config not assigned in TutorialManager!");
            return;
        }

        waveSpawner.enabled = true;
        waveSpawner.waves = new Wave[] { tutorialWaveConfig }; // Set the specific wave for the tutorial
        // waveSpawner.waveIndex = 0; // Ensure it starts from this new wave (if waveIndex isn't private or reset elsewhere)
        // It might be necessary to add a public ResetWaveIndex() method to WaveSpawner if it doesn't reset automatically.

        // Subscribe to the event for when all waves (in this case, the single tutorial wave) are complete.
        waveSpawner.OnAllWavesComplete += HandleTutorialWaveCompletion;
        waveSpawner.isPlanning = false; // Ensure planning is over
        waveSpawner.EndPlanning(); // This should trigger the pre-wave countdown for the first (tutorial) wave
    }

    void HandleTutorialWaveCompletion()
    {
        Debug.Log("Tutorial: Practice wave cleared!");
        if (waveSpawner != null) // Check if waveSpawner is not null before accessing it
        {
            waveSpawner.OnAllWavesComplete -= HandleTutorialWaveCompletion; // Unsubscribe to prevent issues
            waveSpawner.enabled = false; // Disable spawner after tutorial wave
        }
        else
        {
            Debug.LogWarning("[TutorialManager] WaveSpawner was null when trying to unsubscribe or disable in HandleTutorialWaveCompletion.");
        }


        // Display tutorial completion message
        dialoguePanel.SetActive(true);
        continueButton.gameObject.SetActive(false); // No more continuation
        StartCoroutine(TypeText("Tutorial Complete! You're ready to play."));

        // After a delay, load the first regular level via GameManager
        StartCoroutine(ProceedToFirstLevelAfterDelay(delayBeforeLoadingFirstLevel));
    }

    IEnumerator ProceedToFirstLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager.instance != null)
        {
            Debug.Log("[TutorialManager] Proceeding to the first regular level.");
            GameManager.instance.StartFirstRegularLevel();
        }
        else
        {
            Debug.LogError("[TutorialManager] GameManager instance not found! Cannot proceed to first level.");
            // Fallback or error handling if GameManager is missing
        }
    }

    void EndTutorialDialogue()
    {
        // This is called if all dialogue steps are done *before* the wave is triggered (e.g. if wave is last step)
        // If isFinalDialogueBeforeWave handles it, this might just be a fallback.
        dialoguePanel.SetActive(false);
        Debug.Log("Tutorial: Dialogue sequence finished.");
    }

    // IEnumerator LoadNextSceneAfterDelay(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     // SceneManager.LoadScene("MainMenu"); // Or your first actual game level
    // }
}