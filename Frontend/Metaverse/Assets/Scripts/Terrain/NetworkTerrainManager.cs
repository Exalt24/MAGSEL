using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class NetworkTerrainManager : NetworkBehaviour
{
    // SyncVar for the terrain seed.
    private readonly SyncVar<int> terrainSeed = new SyncVar<int>(0);
    // SyncVar for the current emotion.
    private readonly SyncVar<string> currentEmotion = new SyncVar<string>("joy");

    public GameObject EmotionPanel;
    public Text EmotionPanelText;
    public WaveFunctionCollapse wfc; // Reference to your WaveFunctionCollapse component

    public GameObject loadingScreen; // Reference to the loading screen GameObject
    public static bool toSpawn = false;

    public override void OnStartServer()
    {
        currentEmotion.Value = "joy";
        terrainSeed.Value = UnityEngine.Random.Range(0, int.MaxValue);
        // Initialize the terrain with the seed and emotion.
    }

    public override void OnStartClient()
    {
        // Subscribe to SyncVar changes.
        terrainSeed.OnChange += OnTerrainSeedChanged;
        currentEmotion.OnChange += OnEmotionChanged;
        GlobalUserData.currentEmotion = currentEmotion.Value;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the OnChange callbacks.
        terrainSeed.OnChange -= OnTerrainSeedChanged;
        currentEmotion.OnChange -= OnEmotionChanged;
    }

    // Callback when the terrain seed changes.
    private void OnTerrainSeedChanged(int oldSeed, int newSeed, bool asServer)
    {
        // We no longer reinitialize terrain here.
        Debug.Log("Terrain seed changed to: " + newSeed + " while emotion is: " + currentEmotion.Value);
    }

    // Callback when the current emotion changes.
    private void OnEmotionChanged(string oldEmotion, string newEmotion, bool asServer)
    {
        GlobalUserData.currentEmotion = newEmotion;
        wfc.InitializeTerrain(terrainSeed.Value, newEmotion);
    }

    // Called by clients to request a terrain update with a new emotion.
    [ServerRpc(RequireOwnership = false)]
    public void RequestTerrainUpdateServerRpc(string emotion, string userName)
    {

        Debug.Log("Requesting terrain update with emotion: " + emotion + " by " + userName);
        if (currentEmotion.Value != emotion)
        {
            currentEmotion.Value = emotion;
            terrainSeed.Value = UnityEngine.Random.Range(0, int.MaxValue);
            toSpawn = true;
            UpdateAllClientsWithEmotionAndUsernameClientRpc(emotion, userName);
        }
        else
        {
            Debug.Log("Requested emotion is the same as current (" + emotion + "), not updating.");
        }
    }

[ObserversRpc]
private void UpdateAllClientsWithEmotionAndUsernameClientRpc(string emotion, string userName)
{
    Debug.Log("Emotion updated on all clients: " + emotion + " by " + userName);
    StopAllCoroutines();

    if (loadingScreen != null)
    {
        loadingScreen.SetActive(false); // Hide the loading screen if it was shown
    }

    if (EmotionPanel != null)
    {
        // Set the panel to visible
        EmotionPanel.SetActive(true);

        // Change the color of the EmotionPanelText based on the emotion
        EmotionPanelText.color = emotion switch
        {
            "joy" => Color.yellow,
            "sadness" => Color.blue,
            "anger" => Color.red,
            "fear" => Color.black,
            "love" => Color.magenta,
            _ => throw new System.NotImplementedException()
        };

        EmotionPanelText.text = "Detected " + emotion + " from " + userName + ". Changing terrain...";

        // Start a coroutine to check the progress and hide the panel once the generation is complete
        StartCoroutine(CheckProgressAndHidePanel());
    }
    else
    {
        Debug.LogWarning("EmotionPanel is not assigned in the inspector.");
    }
}

// Coroutine to continuously check if the generation is complete and hide the panel
private IEnumerator CheckProgressAndHidePanel()
{
    // Continuously check progress until complete
    while (!GenerationProgressTracker.IsComplete())
    {
        yield return null; // Wait for the next frame
    }

    // Once the progress is complete, hide the EmotionPanel
    EmotionPanel.SetActive(false);
}

}
