using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    [SerializeField] private Text resultsText;   // Reference to the Text component for displaying results.
    [SerializeField] private float displayDuration = 4f; // How long the results will be shown (in seconds).

    /// <summary>
    /// Sets the results text using the given score and total number of questions.
    /// </summary>
    /// <param name="score">The user's score.</param>
    /// <param name="totalQuestions">The total number of questions in the quiz.</param>
    public void SetResults(int score, int totalQuestions)
    {
        if (resultsText != null)
        {
            resultsText.text = $"Quiz Complete!\nScore: {score} / {totalQuestions}";
            resultsText.color = Color.green;
            // Enable Best Fit so the text adjusts to fit the container.
            resultsText.resizeTextForBestFit = true;
            resultsText.resizeTextMinSize = 10;
            resultsText.resizeTextMaxSize = 14;
        }
        // Optionally, start a coroutine to auto-hide the results after a delay.
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
