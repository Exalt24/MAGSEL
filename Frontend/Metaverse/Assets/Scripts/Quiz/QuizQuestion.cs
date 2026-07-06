using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class QuizQuestionUI : MonoBehaviour
{
    public Text questionText;
    public Button[] answerButtons;

    public int correctAnswerIndex { get; private set; }

    public UnityEvent onCorrectAnswerSelected;

    private bool isAnswerLocked = false; // Flag to prevent multiple clicks during response evaluation

    void Awake()
    {
        if (questionText != null)
        {
            questionText.alignment = TextAnchor.UpperCenter;
            questionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            questionText.verticalOverflow = VerticalWrapMode.Overflow;
            questionText.resizeTextForBestFit = false;
            questionText.fontSize = 13;
            questionText.color = Color.red; // Changed to red

            // Position at top of container
            RectTransform rectTransform = questionText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1); // Top-left anchor
            rectTransform.anchorMax = new Vector2(1, 1); // Top-right anchor
            rectTransform.anchoredPosition = new Vector2(0, 0); // No offset from anchor
            rectTransform.sizeDelta = new Vector2(0, 50); // Wi
        }

        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                if (btn != null)
                {
                    Text btnText = btn.GetComponentInChildren<Text>();
                    if (btnText != null)
                    {
                        btnText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        btnText.verticalOverflow = VerticalWrapMode.Overflow; // Changed from Truncate
                        btnText.alignment = TextAnchor.MiddleCenter;
                        btnText.resizeTextForBestFit = false;
                        btnText.fontSize = 12; // Fixed size for testing
                        btnText.color = Color.black;
                    }
                }
            }
        }
    }

    public void SetQuestionData(string question, string correct, string[] incorrect)
    {
        if (questionText != null)
        {
            questionText.text = question;
            LayoutRebuilder.ForceRebuildLayoutImmediate(questionText.GetComponent<RectTransform>());
        }

        List<string> choices = new List<string> { correct };
        choices.AddRange(incorrect);

        if (incorrect.Length < 3)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                int randomIndex = Random.Range(i, choices.Count);
                string temp = choices[i];
                choices[i] = choices[randomIndex];
                choices[randomIndex] = temp;
            }
            while (choices.Count < 4)
            {
                choices.Add("");
            }
        }
        else
        {
            for (int i = 0; i < choices.Count; i++)
            {
                int randomIndex = Random.Range(i, choices.Count);
                string temp = choices[i];
                choices[i] = choices[randomIndex];
                choices[randomIndex] = temp;
            }
        }

        correctAnswerIndex = choices.IndexOf(correct);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                Button btn = answerButtons[i];
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = choices[i];
                }
                btn.onClick.RemoveAllListeners();
                int index = i;
                btn.onClick.AddListener(() => OnAnswerSelected(index));
            }
            else
            {
                Button btn = answerButtons[i];
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                    btnText.text = "";
                btn.onClick.RemoveAllListeners();
            }
        }
    }

    public void OnAnswerSelected(int index)
    {
        if (isAnswerLocked) return; // Prevent multiple clicks while the response is being processed

        isAnswerLocked = true;  // Lock further clicks until the response is handled.

        Button selectedButton = answerButtons[index];
        Text selectedButtonText = selectedButton.GetComponentInChildren<Text>();
        DisableAnswerButtons();

        if (index == correctAnswerIndex)
        {
            Debug.Log("Correct answer selected!");
            selectedButtonText.color = Color.green;
            if (onCorrectAnswerSelected != null)
                onCorrectAnswerSelected.Invoke();
        }
        else
        {
            Debug.Log("Incorrect answer selected!");
            StartCoroutine(ResetAnswerColorsAfterDelay(1f));
            EnableAnswerButtons();
            selectedButtonText.color = Color.red;
            isAnswerLocked = false;
        }
    }

    private IEnumerator ResetAnswerColorsAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);

    foreach (Button btn in answerButtons)
    {
        Text btnText = btn.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.color = Color.black; // Reset color to default (black)
        }
    }

    // Re-enable the answer buttons and unlock further clicks
    EnableAnswerButtons();
    isAnswerLocked = false;
}

    private void DisableAnswerButtons()
    {
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.interactable = false; // Disable buttons after answering
            }
        }
    }

    private void EnableAnswerButtons()
    {
        foreach (Button btn in answerButtons)
        {
            if (btn != null)
            {
                btn.interactable = true; // Re-enable buttons when the next question is ready
            }
        }
    }
}
