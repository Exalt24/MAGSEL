using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    public Material customSkybox; // Assign in the Inspector

    void Start()
    {
        if (customSkybox != null)
        {
            RenderSettings.skybox = customSkybox; // Apply the material as the active skybox
        }
    }

    public void UpdateSkyboxForEmotion(string emotion)
    {
        if (customSkybox == null)
        {
            Debug.LogWarning("Custom skybox material is not assigned.");
            return;
        }

        switch (emotion.ToLower())
        {
            case "sadness":
                Debug.Log("Updating skybox for sadness");
                customSkybox.SetFloat("_SunSize", 0.1f); // Smaller sun
                customSkybox.SetFloat("_SunSizeConvergence", 10f); // More focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 1.5f); // Increase haze
                customSkybox.SetColor("_SkyTint", Color.blue); // Darker blue for calmness
                customSkybox.SetFloat("_Exposure", 1.5f); // Increase exposure
                break;

            case "joy":
                Debug.Log("Updating skybox for joy");
                customSkybox.SetFloat("_SunSize", 0.5f); // Larger sun
                customSkybox.SetFloat("_SunSizeConvergence", 5f); // Less focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 0.8f);
                customSkybox.SetColor("_SkyTint", Color.yellow); // Bright yellow sky
                customSkybox.SetFloat("_Exposure", 1.2f); // Slightly increase exposure
                break;

            case "love":
                Debug.Log("Updating skybox for love");
                customSkybox.SetFloat("_SunSize", 0.3f); // Smaller sun
                customSkybox.SetFloat("_SunSizeConvergence", 7f); // Moderately focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 1.2f);
                customSkybox.SetColor("_SkyTint", new Color(1.0f, 0.3f, 0.5f)); // Pinkish tone
                customSkybox.SetFloat("_Exposure", 1.3f); // Increase exposure slightly
                break;

            case "anger":
                Debug.Log("Updating skybox for anger");
                customSkybox.SetFloat("_SunSize", 0.7f); // Large sun
                customSkybox.SetFloat("_SunSizeConvergence", 3f); // Highly focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 2f);
                customSkybox.SetColor("_SkyTint", Color.red); // Intense red sky
                customSkybox.SetFloat("_Exposure", 1.8f); // Increase exposure significantly
                break;

            case "fear":
                Debug.Log("Updating skybox for fear");
                customSkybox.SetFloat("_SunSize", 0.2f); // Smaller sun
                customSkybox.SetFloat("_SunSizeConvergence", 8f); // Moderately focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 3f);
                customSkybox.SetColor("_SkyTint", Color.black); // Dark theme
                customSkybox.SetFloat("_Exposure", 1.5f); // Increase exposure
                break;

            case "surprise":
                Debug.Log("Updating skybox for surprise");
                customSkybox.SetFloat("_SunSize", 0.4f); // Medium sun
                customSkybox.SetFloat("_SunSizeConvergence", 6f); // Moderately focused sun
                customSkybox.SetFloat("_AtmosphereThickness", 0.9f);
                customSkybox.SetColor("_SkyTint", Color.cyan); // A surreal, glowing look
                customSkybox.SetFloat("_Exposure", 1.4f); // Increase exposure slightly
                break;

            default:
                Debug.LogWarning("Unknown emotion detected. Keeping default skybox settings.");
                break;
        }
    }
}
