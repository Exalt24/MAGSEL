using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    public void Awake()
    {
        if (settingsButton != null)
        {
            settingsButton.interactable = false;
        }
        
    }
    public void ProceedToRegisterLogin()
    {
        Debug.Log("Proceeding to Register/Login Scene");
        SceneManager.LoadScene("RegisterLogin");
    }
    public void ProceedToSettings()
    {
        Debug.Log("Proceeding to Settings Scene");
        // SceneManager.LoadScene("Settings");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}
