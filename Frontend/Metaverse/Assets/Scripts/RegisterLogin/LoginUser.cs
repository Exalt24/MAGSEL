using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public class LoginResponse
{
    public string name;
}

public class LoginUser : MonoBehaviour
{   
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TextMeshProUGUI errorText;  // Assign in Inspector
    [SerializeField] GameObject loadingSpinner; // The loading spinner UI element

    string loginURL = "https://magsel.vercel.app/user/login";

    void Start()
    {
        // Set the password input field to Password input type (***** visible)
        if (passwordInputField != null)
        {
            passwordInputField.inputType = TMP_InputField.InputType.Password;
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordInputField.ForceLabelUpdate(); // Update the label to show the password field as "*****"
        }

        // Ensure the loading spinner is hidden at the start
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(false); // Hide the loading spinner initially
        }
    }

    public void LoginButton()
    {
        if (errorText != null)
            errorText.text = "";

        if (emailInputField == null || passwordInputField == null)
        {
            Debug.LogError("One or more input fields are not assigned");
            if (errorText != null)
                errorText.text = "Missing email or password field.";
            return;
        }

        // Show the loading spinner before starting the network request
        if (loadingSpinner != null)
        {
            loadingSpinner.SetActive(true); // Show the loading spinner
        }

        StartCoroutine(Login(emailInputField.text, passwordInputField.text));
    }

    IEnumerator Login(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form))
        {
            yield return www.SendWebRequest();

            // Hide the loading spinner once the request finishes
            if (loadingSpinner != null)
            {
                loadingSpinner.SetActive(false); // Hide the loading spinner
            }

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
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
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                if (response != null && !string.IsNullOrEmpty(response.name))
                {
                    GlobalUserData.userName = response.name;
                    Debug.Log("Logged in as: " + GlobalUserData.userName);
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
