//This script will be the "brain" of your new UI panel. It links your UI elements to the save system.

using UnityEngine;
using UnityEngine.UI;
using System; // Needed for Enum
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for .ToList()

// Use TMP_Dropdown and TMP_InputField if using TextMeshPro
using TMPro;

public class QuestionCreator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField questionTextInput;
    [SerializeField] private TMP_InputField answer1Input;
    [SerializeField] private TMP_InputField answer2Input;
    [SerializeField] private TMP_InputField answer3Input;
    [SerializeField] private TMP_InputField answer4Input;
    
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_Dropdown correctAnswerDropdown;
    
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_Text feedbackText;

    private void Start()
    {
        // Populate the dropdowns with values from your Enums
        PopulateCategoryDropdown();
        PopulateDifficultyDropdown();
        
        // Add a listener to the save button
        saveButton.onClick.AddListener(OnSaveQuestion);
        
        // Clear feedback text
        feedbackText.text = "";
    }

    private void PopulateCategoryDropdown()
    {
        // Get all the names from the QuestionCategory enum
        string[] categoryNames = Enum.GetNames(typeof(QuestionCategory));
        List<string> options = new List<string>(categoryNames);
        
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(options);
    }

    private void PopulateDifficultyDropdown()
    {
        // Get all the names from the QuestionDifficulty enum
        string[] difficultyNames = Enum.GetNames(typeof(QuestionDifficulty));
        List<string> options = new List<string>(difficultyNames);
        
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(options);
    }

    public void OnSaveQuestion()
    {
        // --- 1. Validation ---
        if (string.IsNullOrWhiteSpace(questionTextInput.text) ||
            string.IsNullOrWhiteSpace(answer1Input.text) ||
            string.IsNullOrWhiteSpace(answer2Input.text) ||
            string.IsNullOrWhiteSpace(answer3Input.text) ||
            string.IsNullOrWhiteSpace(answer4Input.text))
        {
            ShowFeedback("Error: Please fill in all fields.", true);
            return;
        }

        // --- 2. Create the Question Object ---
        QuizQuestionPython newQuestion = new QuizQuestionPython();
        
        newQuestion.questionText = questionTextInput.text;
        
        // Create the list of answers
        newQuestion.answers = new List<string>
        {
            answer1Input.text,
            answer2Input.text,
            answer3Input.text,
            answer4Input.text
        };
        
        // Get the enum values from the dropdowns
        newQuestion.category = (QuestionCategory)categoryDropdown.value;
        newQuestion.difficulty = (QuestionDifficulty)difficultyDropdown.value;
        
        // The value of the dropdown (0, 1, 2, 3) directly matches the index
        newQuestion.correctAnswerIndex = correctAnswerDropdown.value;

        // --- 3. Save the Question ---
        try
        {
            QuizDataHandler.AddQuestion(newQuestion);
            ShowFeedback("Question saved successfully!", false);
            ClearInputFields();
        }
        catch (Exception e)
        {
            ShowFeedback($"Error saving file: {e.Message}", true);
            Debug.LogError(e);
        }
    }

    private void ShowFeedback(string message, bool isError)
    {
        feedbackText.text = message;
        feedbackText.color = isError ? Color.red : Color.green;
    }

    private void ClearInputFields()
    {
        questionTextInput.text = "";
        answer1Input.text = "";
        answer2Input.text = "";
        answer3Input.text = "";
        answer4Input.text = "";
        
        // Reset dropdowns to the first option
        categoryDropdown.value = 0;
        difficultyDropdown.value = 0;
        correctAnswerDropdown.value = 0;
    }
}