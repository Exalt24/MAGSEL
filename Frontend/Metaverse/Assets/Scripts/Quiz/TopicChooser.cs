using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using TMPro;
using Unity.Mathematics;
using System.Linq;


/// <summary>
/// Attach this script to your TopicChooserPrefab.
/// It references your UI elements (InputField, Texts, Buttons) and sends a request
/// to generate a topic and chunks. The result is displayed in the UI by instantiating a prefab for each chunk.
/// The user can then proceed to the quiz.
/// </summary>
public class TopicChooserUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InputField userInputField;   // Input field for user text.
    [SerializeField] private Text generatedTopicText;     // Displays the generated topic.
    [SerializeField] private Button generateButton;       // Button to trigger generation.
    [SerializeField] private Button proceedButton;        // Button to proceed to the quiz.
    [SerializeField] private Button exitButton;           // Button to close the panel (Newly added).
    
    
    [Header("Chunk Panel")]
    [Tooltip("The GameObject that will contain the chunk items (e.g., the Content GameObject of your ScrollView).")]
    [SerializeField] private GameObject generatedChunksPanelGO;
    [Tooltip("A prefab that contains a Text component for displaying a single chunk.")]
    [SerializeField] private GameObject chunkTextPrefab;

    private string topicChunkEndpoint = "https://humbly-magical-whippet.ngrok-free.app/generate_topic_chunk";

    [Header("Quiz Manager Reference")]
    public AdaptiveQuiz adaptiveQuiz;
    public EmotionAnalyzer emotionAnalyzer;

    // We'll store the latest generated topic and chunks so we can pass them to the quiz if needed.
    private string latestTopic = "";
    private string[] latestChunks = new string[0];

    // Data structure for the JSON response from the server.
    [Serializable]
    private class TopicChunkResponse
    {
        public string topic;
        public string[] chunks;
    }

    void Awake()
    {
        // Hook up the Generate and Proceed button events.
        if (generateButton != null)
        {
            generateButton.onClick.RemoveAllListeners();
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
        }
        
        if (proceedButton != null)
        {
            proceedButton.onClick.RemoveAllListeners();
            proceedButton.onClick.AddListener(OnProceedButtonClicked);
            proceedButton.interactable = false; // Disabled until we have a valid topic.
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitButtonClicked);  // Add listener for the exit button
        }
    }

    void Update()
    {

        if (userInputField != null && userInputField.isFocused)
        {
            GameManager.isInputEnabled = false;
            if (userInputField.text.Length > 0)
            {
                generateButton.interactable = true; // Enable the button if there's text.
            }
            else
            {
                generateButton.interactable = false; // Disable the button if no text.
            }
        }
        else if (userInputField != null && !userInputField.isFocused && userInputField.text.Length > 0)
        {
            GameManager.isInputEnabled = true; // Enable input when the field is not focused.
        }
        else
        {
        
            generateButton.interactable = false;
        }
    }

    /// <summary>
    /// Called when the user clicks the "Generate" button.
    /// Sends the user-input text to the server to generate a topic and chunks.
    /// </summary>
    private void OnGenerateButtonClicked()
    {
        if (userInputField == null || string.IsNullOrEmpty(userInputField.text))
        {
            Debug.LogWarning("No input text provided.");
            return;
        }
        // Optionally disable the button while request is in progress.
        generateButton.interactable = false;

        // Clear old results.
        if (generatedTopicText != null)
            generatedTopicText.text = "Generating topic...";
        ClearChunkItems();
        Debug.Log("Generating topic with input: " + userInputField.text);
        // Start the coroutine to send the request.
        StartCoroutine(SendTopicChunkRequest(userInputField.text));
    }

    /// <summary>
    /// Coroutine that sends the text to the /generate_topic_chunk endpoint and handles the response.
    /// </summary>
    private const float TimeoutDuration = 50f; // 30 seconds timeout for the request.

private IEnumerator SendTopicChunkRequest(string userText)
{
    string jsonData = "{\"text\":\"" + userText + "\"}";
    using (UnityWebRequest request = new UnityWebRequest(topicChunkEndpoint, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = (int)TimeoutDuration; // Set the timeout duration.
        yield return request.SendWebRequest();



        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            if (generatedTopicText != null)
                generatedTopicText.text = "Error: " + request.error;
        }
        else
        {
            // Parse the response JSON and update the UI.
            ParseTopicChunkResponse(request.downloadHandler.text);
        }

        // Re-enable the generate button after request is done.
        generateButton.interactable = true;
    }
}


    /// <summary>
    /// Parse the JSON response from the server to get the topic and chunks,
    /// then display them in the UI by instantiating a prefab for each chunk.
    /// </summary>
    private void ParseTopicChunkResponse(string responseJson)
    {
        try
        {
            TopicChunkResponse response = JsonUtility.FromJson<TopicChunkResponse>(responseJson);
            if (response != null)
            {
                latestTopic = response.topic;
                latestChunks = response.chunks;

                if (generatedTopicText != null)
                {
                    generatedTopicText.text = $"Generated Topic: {latestTopic}";
                }

                // Clear any previous chunk items.
                ClearChunkItems();

                //Check first if each chunk has text before creating the item.



                if (latestChunks != null && latestChunks.Length > 0)
                {
                    foreach (string chunk in latestChunks)
                    {
                        if (string.IsNullOrEmpty(chunk))
                        {
                            latestChunks = latestChunks.Where(c => c != chunk).ToArray();
                            continue;
                        }

                        CreateChunkItem(chunk);
                    }
                }

                // Enable the proceed button now that we have a valid topic.
                if (proceedButton != null && latestChunks.Length > 0)
                    proceedButton.interactable = true;
            }
            else
            {
                Debug.LogWarning("Failed to parse topic-chunk response as TopicChunkResponse.");
                if (generatedTopicText != null)
                    generatedTopicText.text = "Invalid JSON response.";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing topic-chunk response: " + e.Message);
            if (generatedTopicText != null)
                generatedTopicText.text = "Error parsing response.";
        }
    }

    /// <summary>
    /// Instantiates a chunkTextPrefab under the generatedChunksPanelGO and sets its text.
    /// </summary>
    private void CreateChunkItem(string chunkText)
    {
        if (chunkTextPrefab == null || generatedChunksPanelGO == null)
        {
            Debug.LogWarning("chunkTextPrefab or generatedChunksPanelGO is not assigned.");
            return;
        }
        GameObject chunkObj = Instantiate(chunkTextPrefab, generatedChunksPanelGO.transform);
        Text chunkTxt = chunkObj.GetComponentInChildren<Text>();
        if (chunkTxt != null)
        {
            chunkTxt.text = "- " + chunkText;
            chunkTxt.alignment = TextAnchor.MiddleLeft; // Align text to the left.
        }
    }

    /// <summary>
    /// Clears all child objects from generatedChunksPanelGO.
    /// </summary>
    private void ClearChunkItems()
    {
        if (generatedChunksPanelGO == null) return;
        for (int i = generatedChunksPanelGO.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(generatedChunksPanelGO.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Called when the user clicks the "Proceed" button.
    /// Calls AdaptiveQuiz to fetch a quiz using the generated topic and chunks.
    /// </summary>
    private void OnProceedButtonClicked()
    {
        Debug.Log("Proceeding with topic: " + latestTopic);

        // If adaptiveQuiz is not already assigned, try to find it in the scene.
        if (adaptiveQuiz == null)
        {
            GameObject quizManagerGO = GameObject.Find("QuizManager");
            if (quizManagerGO != null)
            {
                adaptiveQuiz = quizManagerGO.GetComponent<AdaptiveQuiz>();
            }
        }

        if (emotionAnalyzer == null)
        {
            GameObject emotionAnalyzerGO = GameObject.Find("EmotionManager");
            if (emotionAnalyzerGO != null)
            {
                emotionAnalyzer = emotionAnalyzerGO.GetComponent<EmotionAnalyzer>();
            }
        }

        emotionAnalyzer.GeneratedTopicText = latestTopic;
        emotionAnalyzer.GeneratedChunks = latestChunks;
        
        if (adaptiveQuiz != null)
        {
            // For now, we pass "Easy" as the difficulty and latestChunks as the chunkText.
            adaptiveQuiz.FetchQuizQuestion(latestTopic, "Easy", latestChunks);
        }
        else
        {
            Debug.LogError("QuizManager not found in the scene!");
        }

        // Hide or disable this TopicChooser UI.
        gameObject.SetActive(false);
    }

     private void OnExitButtonClicked()
    {
         // If adaptiveQuiz is not already assigned, try to find it in the scene.
        if (adaptiveQuiz == null)
        {
            GameObject quizManagerGO = GameObject.Find("QuizManager");
            if (quizManagerGO != null)
            {
                adaptiveQuiz = quizManagerGO.GetComponent<AdaptiveQuiz>();
            }
        }
        adaptiveQuiz.HideTopicChooser();
    }

}
