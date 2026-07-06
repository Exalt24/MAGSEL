using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Transporting;
using FishNet.Managing.Client;

public class GameManager : MonoBehaviour
{
    [Header("Chat Settings")]
    public string playerName = ""; // Leave empty to default to client number.
    public int maxMessages = 25;
    public GameObject chatPanel;          // Parent object for chat messages.
    public GameObject textObject;         // Prefab for chat text element.
    public TMP_InputField chatBox;
    
    [SerializeField]
    List<Message> messageList = new List<Message>();

    [Header("Other Settings")]
    public EmotionAnalyzer emotionAnalyzer;
    public AdaptiveQuiz adaptiveQuiz;
    public GameObject quizPanel;
    public Camera mainCamera;
    public CopyMaterialsGame copyMaterials;
    public ChooseHairSetsGame chooseHairSets;
    public List<GameObject> models;

    public static bool isInputEnabled = true;
    // Reference to the Canvas or UI container GameObject
    [SerializeField]
    private GameObject canvasGameObject; // Reference to the Canvas GameObject
    private ClientManager clientManager; // Reference to the ClientManager
    private GameObject loadingScreen; // Reference to the loading screen GameObject

    private void OnEnable()
    {
        // Register for chat broadcasts on client and server with matching callback signatures.
        InstanceFinder.ClientManager.RegisterBroadcast<ChatBroadcastMessage>(OnMessageReceived);
        InstanceFinder.ServerManager.RegisterBroadcast<ChatBroadcastMessage>(OnServerMessageReceived);


        // Get the reference to the ClientManager
        clientManager = InstanceFinder.ClientManager;

        // Register the connection state change event
        if (clientManager != null)
        {
            clientManager.OnClientConnectionState += OnClientConnectionState;
        }
    }

    private void OnDisable()
    {
        // Unregister the connection state change event
        if (clientManager != null)
        {
            clientManager.OnClientConnectionState -= OnClientConnectionState;
        }

        if (InstanceFinder.ClientManager != null)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<ChatBroadcastMessage>(OnMessageReceived);
        }

        if (InstanceFinder.ServerManager != null)
        {
            InstanceFinder.ServerManager.UnregisterBroadcast<ChatBroadcastMessage>(OnServerMessageReceived);
        }
    }

    // Handle client connection state change
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            // Client has disconnected, so reset the UI
            OnClientDisconnected();
        }
    }

    private void OnClientDisconnected()
    {
        // Deactivate the canvas
        if (canvasGameObject != null && canvasGameObject.activeSelf)
        {
            canvasGameObject.SetActive(false);
        }

        chatBox.text = ""; // Clear the chat input field
        if (emotionAnalyzer != null)
        {
            emotionAnalyzer.StopAllCoroutines(); // Stop any ongoing emotion analysis
        }

        if (adaptiveQuiz != null)
        {
            adaptiveQuiz.StopAllCoroutines(); // Stop any ongoing quiz processes
        }

        // Reinitialize or refresh the canvas and any necessary UI elements
        InitializeCanvas();
    }

    public void InitializeCanvas()
    {
        // Reactivate the canvas
        if (canvasGameObject != null)
        {
            canvasGameObject.SetActive(true);
        }
        
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false); // Hide the loading screen if it was shown
        }

        // Reinitialize the chat panel or any other necessary UI elements
        if (chatPanel != null)
        {
            // Example: Clear and reset the chat panel (if necessary)
            foreach (Transform child in chatPanel.transform)
            {
                Destroy(child.gameObject); // Clear old messages if needed
            }
        }

        // Re-initialize quiz panel or any other components
        if (quizPanel != null)
        {
            // Optionally reset or refresh quizPanel children
            foreach (Transform child in quizPanel.transform)
            {
                Destroy(child.gameObject); // Remove any previous quiz components if necessary
            }
        }

    }





    void Start()
    {
        // Initially disable the Canvas GameObject
        if (canvasGameObject != null)
        {
            Debug.Log("Canvas GameObject found, setting it to inactive.");
            canvasGameObject.SetActive(false); // Hide the Canvas or UI GameObject initially
        }
    }

    void Update()
    {
        if (InstanceFinder.IsClientStarted)
        {
            // If the client has started, activate the canvas
            if (canvasGameObject != null && !canvasGameObject.activeSelf)
            {
                canvasGameObject.SetActive(true); // Activate the Canvas GameObject
            }

            if ((chatBox != null && chatBox.isFocused) || !GenerationProgressTracker.IsComplete())
            {
                isInputEnabled = false;  // Disable input processing when chatBox is focused
            }
            else
            {
                isInputEnabled = true;   // Enable input processing
            }

            // If chatBox is not focused and Enter is pressed, activate it.
            if (chatBox != null && !chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }

            // If chatBox has text and Enter is pressed, send the message.
            if (chatBox != null && !string.IsNullOrEmpty(chatBox.text) && Input.GetKeyDown(KeyCode.Return))
            {
                SendChatMessage(chatBox.text);
                AnalyzeText(chatBox.text);
                chatBox.text = "";
            }
        }
        else
        {
            // If the client hasn't started, ensure the canvas is disabled
            if (canvasGameObject != null && canvasGameObject.activeSelf)
            {
                canvasGameObject.SetActive(false); // Deactivate the Canvas GameObject
            }
        }
    }

    /// <summary>
    /// Sends a chat message using FishNet broadcasting.
    /// </summary>
    private void SendChatMessage(string messageText)
    {
        // Use the client's connection ID as the username if no custom name is set.
        string finalUsername = GlobalUserData.userName;
        
        ChatBroadcastMessage msg = new ChatBroadcastMessage
        {
            username = finalUsername,
            message = messageText,
            timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        // Use the new properties IsServerStarted/IsClientStarted instead of IsServer/IsClient.
        if (InstanceFinder.IsServerStarted)
        {
            InstanceFinder.ServerManager.Broadcast(msg);
        }
        else if (InstanceFinder.IsClientStarted)
        {
            InstanceFinder.ClientManager.Broadcast(msg);
        }
    }

    /// <summary>
    /// Called on every client when a chat broadcast message is received.
    /// Note: The callback signature now includes a Channel parameter.
    /// </summary>
    private void OnMessageReceived(ChatBroadcastMessage msg, Channel channel)
    {
        Debug.Log($"Received message from {msg.username}: {msg.message}");

        // If we've reached our maximum message count, remove the oldest message.
        if (messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }
        
        // Create a new Message instance.
        Message newMessage = new Message();
        newMessage.text = msg.message;
        newMessage.sender = msg.username;
        newMessage.timestamp = msg.timestamp;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        if (msg.username != GlobalUserData.userName)
        {
            newMessage.textObject.text = $"[{newMessage.timestamp}] {newMessage.sender}: {newMessage.text}";

            if (msg.message.Equals("/quiz", StringComparison.OrdinalIgnoreCase))
            {
                newMessage.textObject.text = $"{newMessage.sender} has started a quiz!";
            }
        }
        else
        {
            newMessage.textObject.color = Color.green;
            newMessage.textObject.text = $"[{newMessage.timestamp}] You: {newMessage.text}";

            if(msg.message.Equals("/quiz", StringComparison.OrdinalIgnoreCase))
            {
                newMessage.textObject.text = "You have started a quiz!";
            }
        }

        messageList.Add(newMessage);
    }

    /// <summary>
    /// Called on the server when it receives a chat broadcast message from a client.
    /// The callback signature now includes a NetworkConnection and a Channel.
    /// </summary>
    private void OnServerMessageReceived(NetworkConnection sender, ChatBroadcastMessage msg, Channel channel)
    {
        InstanceFinder.ServerManager.Broadcast(msg);
    }

    private void AnalyzeText(string text)
    {
        if (emotionAnalyzer != null && adaptiveQuiz != null)
        {
            Transform quizPanelTransform = FindChildByName(quizPanel.transform, "QuizQuestionPrefab(Clone)");
            Transform topicChooserTransform = FindChildByName(quizPanel.transform, "TopicChooserPrefab(Clone)");
            Transform resultsTransform = FindChildByName(quizPanel.transform, "ResultsPrefab(Clone)");

            if (text.Equals("/quiz", StringComparison.OrdinalIgnoreCase))
            {
                if (quizPanelTransform != null || topicChooserTransform != null || resultsTransform != null)
                {
                    Debug.Log("Quiz panel is active, not analyzing text.");
                    return;
                }
                adaptiveQuiz.ShowTopicChooser();
                return;
            }

            emotionAnalyzer.AnalyzeEmotion(text);
        }
        else
        {
            Debug.LogWarning("EmotionAnalyzer is not assigned.");
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
}

[System.Serializable]
public class Message
{
    public string text;
    public string sender;
    public string timestamp;
    public Text textObject;
    public MessageType messageType;
    public enum MessageType
    {
        playerMessage,
        info,
    }
}

[System.Serializable]
public class QuizResponseWrapper
{
    public string difficulty;
    public Question[] questions;
}

[System.Serializable]
public class Question
{
    public string question;
    public Answers answers;
}

[System.Serializable]
public class Answers
{
    public string correct;
    public string[] incorrect;
}

/// <summary>
/// A broadcast message for chat that implements IBroadcast.
/// </summary>
[Serializable]
public struct ChatBroadcastMessage : IBroadcast
{
    public string username;
    public string message;
    public string timestamp;
}
