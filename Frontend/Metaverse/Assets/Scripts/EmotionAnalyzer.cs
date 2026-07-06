using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EmotionAnalyzer : MonoBehaviour
{
    // URL of your Flask API
    private string apiUrl = "https://humbly-magical-whippet.ngrok-free.app/predict";
    public WaveFunctionCollapse waveFunctionCollapse;
    public SkyboxController skyboxController;
    public AdaptiveQuiz adaptiveQuiz;
    public GameObject quizPanel;
    public NetworkTerrainManager networkTerrainManager;
    public string GeneratedTopicText { get; set; }
    public string[] GeneratedChunks { get; set; }
    // Method to analyze emotion; call this from UI or other scripts
    public void AnalyzeEmotion(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("Input text is empty. Please provide valid text.");
            return;
        }
        StartCoroutine(SendRequest(text));
    }

    // Coroutine to send a POST request
    private IEnumerator SendRequest(string text)
    {
        // Create JSON payload
        string jsonData = "{\"text\":\"" + text + "\"}";

        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Handle the response
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}\nResponse Code: {request.responseCode}\nResponse: {request.downloadHandler.text}");
            }
            else
            {
                HandleEmotionResponse(JsonUtility.FromJson<EmotionResponse>(request.downloadHandler.text));
            }
        }
    }


    // Process the API response
    private void HandleEmotionResponse(EmotionResponse response)
    {
        if (response != null)
        {
            Debug.Log($"Text: {response.text}, Predicted Class: {response.predicted_class}, Predicted Label: {response.predicted_label}");
            
            if(response.predicted_label == "surprise")
            {
                response.predicted_label = "joy";
            }

            networkTerrainManager.RequestTerrainUpdateServerRpc(response.predicted_label, GlobalUserData.userName);

            Transform quizPanelTransform = FindChildByName(quizPanel.transform, "QuizQuestionPrefab(Clone)");
            if (quizPanelTransform != null)
            {
                adaptiveQuiz.updateQuizForEmotion(GeneratedTopicText, response.predicted_label, GeneratedChunks);
            } 
        }
        else
        {
            Debug.LogWarning("Received an empty or invalid response from the API.");
        }
    }

     private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    [System.Serializable]
    private class EmotionResponse
    {
        public string text;
        public int predicted_class;
        public string predicted_label;
    }
}
