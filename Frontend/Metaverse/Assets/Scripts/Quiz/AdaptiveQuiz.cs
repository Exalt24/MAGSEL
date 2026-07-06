using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class AdaptiveQuiz : MonoBehaviour
{
    private string apiUrl = "https://humbly-magical-whippet.ngrok-free.app/generate_quiz";
    private const float TimeoutDuration = 500f;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject quizPrefab;
    [SerializeField] private GameObject progressTextPrefab;
    [SerializeField] private GameObject resultsPrefab;
    [SerializeField] private GameObject topicChooserPrefab;
    [SerializeField] private GameObject loadingSpinner;

    private Text progressTextInstance;
    private QuizResponseWrapper storedResponse;
    public string globalDifficulty = "Easy";
    private int currentQuestionIndex = 0;
    private int score = 0;

    void Start()
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }
    }

    public void FetchQuizQuestion(string topic = "Photosynthesis", string difficulty = "Easy", string[] chunkText = null)
    {
        if (chunkText == null)
        {
            chunkText = new string[]
            {
                "Photosynthesis is the process by which green plants and some other organisms use sunlight to synthesize nutrients from carbon dioxide and water.",
                "Photosynthesis in plants generally involves the green pigment chlorophyll and generates oxygen as a byproduct."
            };
        }
        if (string.IsNullOrEmpty(topic))
        {
            Debug.LogWarning("Topic text is empty. Please provide valid text.");
            return;
        }

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true);
        }

        StartCoroutine(SendRequest(topic, difficulty, chunkText));
    }

    public void updateQuizForEmotion(string topic, string emotion, string[] chunkText)
    {
        if (quizPanel != null)
            quizPanel.SetActive(false);

        ClearQuizPanel();

        switch (emotion.ToLower())
        {
            case "sadness":
                globalDifficulty = "Easy";
                break;
            case "joy":
                globalDifficulty = "Medium";
                break;
            case "love":
                globalDifficulty = "Hard";
                break;
            case "anger":
                globalDifficulty = "Medium";
                break;
            case "fear":
                globalDifficulty = "Easy";
                break;
            case "surprise":
                globalDifficulty = "Medium";
                break;
            default:
                globalDifficulty = "Easy";
                break;
        }

        FetchQuizQuestion(topic, globalDifficulty, chunkText);
    }

    private IEnumerator SendRequest(string topic, string difficulty, string[] chunkText)
    {
        QuizRequest requestData = new QuizRequest
        {
            topic = topic,
            difficulty = difficulty,
            chunk_text = chunkText
        };
        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("Sending JSON: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)TimeoutDuration;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Network Error: " + request.error);
                HandleRequestError();
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Response: " + responseText);
                
                try
                {
                    // Add validation for the JSON parsing
                    if (string.IsNullOrEmpty(responseText))
                    {
                        Debug.LogError("Empty response received from server");
                        HandleRequestError();
                        yield break;
                    }

                    storedResponse = JsonUtility.FromJson<QuizResponseWrapper>(responseText);
                    
                    // Validate the parsed response
                    if (storedResponse == null)
                    {
                        Debug.LogError("Failed to parse JSON response");
                        HandleRequestError();
                        yield break;
                    }

                    if (storedResponse.questions == null || storedResponse.questions.Length == 0)
                    {
                        Debug.LogError("No questions found in response");
                        HandleRequestError();
                        yield break;
                    }

                    currentQuestionIndex = 0;
                    score = 0;

                    if (loadingSpinner != null)
                    {
                        loadingSpinner.SetActive(false);
                    }

                    DisplayCurrentQuestion();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing response: " + e.Message);
                    HandleRequestError();
                }
            }
        }
    }

    private void HandleRequestError()
    {
        // Hide the loading spinner
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false);
        }

        // Clear quiz panel and reset state
        ClearQuizPanel();
        
        // Reset stored response
        storedResponse = null;

        // Return to main menu/canvas
        if (gameManager != null)
        {
            gameManager.InitializeCanvas();
        }
    }

    private void ClearQuizPanel()
    {
        if (quizPanel != null)
        {
            // Clear all children
            for (int i = quizPanel.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = quizPanel.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            quizPanel.SetActive(false);
        }
        
        // Reset progress text reference
        progressTextInstance = null;
    }

    private void DisplayCurrentQuestion()
    {
        // Double-check that we have valid data before proceeding
        if (storedResponse == null || storedResponse.questions == null || storedResponse.questions.Length == 0)
        {
            Debug.LogError("Invalid quiz data in DisplayCurrentQuestion");
            HandleRequestError();
            return;
        }

        // Activate the quiz panel
        quizPanel.SetActive(true);

        // Create progress text if needed
        if (progressTextInstance == null && progressTextPrefab != null)
        {
            GameObject progressObj = Instantiate(progressTextPrefab, quizPanel.transform);
            progressTextInstance = progressObj.GetComponent<Text>();
            if (progressTextInstance != null)
            {
                progressTextInstance.alignment = TextAnchor.MiddleCenter;
                progressTextInstance.resizeTextForBestFit = true;
                progressTextInstance.resizeTextMinSize = 10;
                progressTextInstance.resizeTextMaxSize = 20;
            }
        }

        // Update progress text safely
        if (progressTextInstance != null)
        {
            progressTextInstance.text = $"Question {currentQuestionIndex + 1} of {storedResponse.questions.Length}";
        }

        // Clear previous quiz question items while preserving the progress text
        for (int i = quizPanel.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = quizPanel.transform.GetChild(i);
            if (child.gameObject != progressTextInstance?.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        if (currentQuestionIndex < storedResponse.questions.Length)
        {
            // Instantiate and populate a UI element for the current question
            GameObject quizItem = Instantiate(quizPrefab, quizPanel.transform);
            QuizQuestionUI questionUI = quizItem.GetComponent<QuizQuestionUI>();
            if (questionUI != null)
            {
                Question currentQuestion = storedResponse.questions[currentQuestionIndex];
                
                // Validate question data
                if (currentQuestion == null || string.IsNullOrEmpty(currentQuestion.question))
                {
                    Debug.LogError($"Invalid question data at index {currentQuestionIndex}");
                    HandleRequestError();
                    return;
                }

                questionUI.SetQuestionData(currentQuestion.question, currentQuestion.answers.correct, currentQuestion.answers.incorrect);
                questionUI.onCorrectAnswerSelected.RemoveAllListeners();
                questionUI.onCorrectAnswerSelected.AddListener(() => OnCorrectAnswer());
            }
            else
            {
                Debug.LogWarning("QuizQuestionUI component not found on the quiz prefab.");
            }
        }
        else
        {
            ShowResults();
        }
    }

    private void OnCorrectAnswer()
    {
        Debug.Log("Correct answer event received.");
        score++;
        StartCoroutine(AdvanceAfterDelay(1f));
    }

    private IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentQuestionIndex++;
        
        if (storedResponse != null && storedResponse.questions != null && currentQuestionIndex < storedResponse.questions.Length)
        {
            DisplayCurrentQuestion();
        }
        else
        {
            ShowResults();
        }
    }

    private void ShowResults()
    {
        // Clear all quiz content
        for (int i = quizPanel.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = quizPanel.transform.GetChild(i);
            Destroy(child.gameObject);
        }

        // Instantiate the results prefab
        if (resultsPrefab != null)
        {
            GameObject resultsObj = Instantiate(resultsPrefab, quizPanel.transform);
            Text resultsText = resultsObj.GetComponentInChildren<Text>();
            if (resultsText != null)
            {
                resultsText.text = $"Quiz Complete!";
                resultsText.alignment = TextAnchor.MiddleCenter;
                resultsText.color = Color.green;
                resultsText.resizeTextForBestFit = true;
                resultsText.resizeTextMinSize = 10;
                resultsText.resizeTextMaxSize = 20;
                
                ContentSizeFitter csf = resultsText.GetComponent<ContentSizeFitter>();
                if (csf == null)
                {
                    csf = resultsText.gameObject.AddComponent<ContentSizeFitter>();
                }
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        
        StartCoroutine(HidePanelAfterDelay(3f));
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearQuizPanel();
    }

    public void ShowTopicChooser()
    {
        if (topicChooserPrefab != null && quizPanel != null)
        {
            ClearQuizPanel();
            Instantiate(topicChooserPrefab, quizPanel.transform);
            quizPanel.SetActive(true);
        }
    }

    public void HideTopicChooser()
    {
        Debug.Log("Hiding topic chooser and quiz panel.");
        ClearQuizPanel();
    }

    [Serializable]
    private class QuizRequest
    {
        public string topic;
        public string difficulty;
        public string[] chunk_text;
    }

    [Serializable]
    private class QuizResponseWrapper
    {
        public string difficulty;
        public Question[] questions;
    }

    [Serializable]
    private class Question
    {
        public string question;
        public Answers answers;
    }

    [Serializable]
    private class Answers
    {
        public string correct;
        public string[] incorrect;
    }
}