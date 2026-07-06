using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public class RegisterResponse
{
    public string name;
    // Add other fields if needed.
}

public class RegisterUser : MonoBehaviour
{   
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField confirmPasswordInputField;
    [SerializeField] TextMeshProUGUI errorText;  // Assign this in the Inspector
    [SerializeField] GameObject loadingSpinner; // The loading spinner UI element

    string registerURL = "https://magsel.vercel.app/user/register";

    void Start()
    {
        if (passwordInputField != null)
        {
            passwordInputField.inputType = TMP_InputField.InputType.Password;
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordInputField.ForceLabelUpdate(); // Update the label to show the password field as "*****"
        }

        if (confirmPasswordInputField != null)
        {
            confirmPasswordInputField.inputType = TMP_InputField.InputType.Password;
            confirmPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            confirmPasswordInputField.ForceLabelUpdate(); // Update the label to show the password field as "*****"
        }

        // Ensure the loading spinner is hidden at the start
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false); // Hide the loading spinner initially
        }
    }

    public void RegisterButton()
    {
        // Clear any previous error message.
        if (errorText != null)
            errorText.text = "";

        if (emailInputField == null || usernameInputField == null || passwordInputField == null || confirmPasswordInputField == null)
        {
            Debug.LogError("One or more input fields are not assigned");
            if (errorText != null)
                errorText.text = "One or more input fields are missing.";
            return;
        }

        if (passwordInputField.text != confirmPasswordInputField.text)
        {
            Debug.LogError("Passwords do not match");
            if (errorText != null)
                errorText.text = "Passwords do not match.";
            return;
        }

        // Show the loading spinner before starting the registration request
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true); // Show the loading spinner
        }

        StartCoroutine(Register(emailInputField.text, usernameInputField.text, passwordInputField.text));
    }

    IEnumerator Register(string email, string name, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("name", name);    
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(registerURL, form))
        {
            yield return www.SendWebRequest();

            // Hide the loading spinner once the request finishes
            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(false); // Hide the loading spinner
            }

            // Check for errors.
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
                if (errorText != null)
                {
                    ServerError err = JsonUtility.FromJson<ServerError>(www.downloadHandler.text);
                    if (err != null && !string.IsNullOrEmpty(err.message))
                    {
                        errorText.text = err.message;
                    }
                    else
                    {
                        errorText.text = www.downloadHandler.text;
                    }
                }
            }
            else
            {
                Debug.Log("Response: " + www.downloadHandler.text);
                RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                if (response != null && !string.IsNullOrEmpty(response.name))
                {
                    GlobalUserData.userName = response.name;
                    Debug.Log("Registered as: " + GlobalUserData.userName);
                }
                else
                {
                    Debug.LogWarning("Could not extract user name from response.");
                    if (errorText != null)
                        errorText.text = "Could not extract user name from response.";
                }
                SceneManager.LoadScene("CharacterCustomizer");
            }
        }
    }
}
