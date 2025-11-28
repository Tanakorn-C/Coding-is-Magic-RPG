// This script manages the entire Question Editor UI
// It loads, displays, edits, adds, and deletes questions.
// This REPLACES your old QuestionCreator.cs

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Use TextMeshPro
using UnityEngine.SceneManagement; // <-- 1. เพิ่มบรรทัดนี้

public class QuestionManager : MonoBehaviour
{
    [Header("Main List Panel")]
    [SerializeField] private GameObject questionListPanel;
    [SerializeField] private Transform questionListContainer; // The "Content" object in your ScrollView
    [SerializeField] private GameObject questionListItemPrefab; // Your new prefab
    [SerializeField] private Button openAddPanelButton; // The "+ Add New" button
    [SerializeField] private Button backButton; // <-- 2. เพิ่มบรรทัดนี้
    [Header("List Filtering & Sorting")]
    [SerializeField] private TMP_Dropdown filterCategoryDropdown;
    [SerializeField] private TMP_Dropdown filterDifficultyDropdown;
    [SerializeField] private TMP_Dropdown sortDropdown;

    [Header("Add/Edit Panel")]
    [SerializeField] private GameObject addEditPanel;
    [SerializeField] private TMP_Text panelTitleText; // "Add Question" or "Edit Question"
    [SerializeField] private TMP_InputField questionTextInput;
    [SerializeField] private TMP_InputField answer1Input;
    [SerializeField] private TMP_InputField answer2Input;
    [SerializeField] private TMP_InputField answer3Input;
    [SerializeField] private TMP_InputField answer4Input;

    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_Dropdown correctAnswerDropdown;

    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text feedbackText;

    // --- Internal Data ---
    private List<QuizQuestionPython> allCustomQuestions;
    private QuizQuestionPython currentQuestionToEdit; // null if we are ADDING, not null if EDITING

    private void Start()
    {
        // Populate the dropdowns with values from your Enums
        PopulateCategoryDropdown();
        PopulateDifficultyDropdown();



        // --- 3. เพิ่ม 4 บรรทัดนี้ ---
        PopulateFilterDropdowns(); // <-- เมธอดใหม่
        PopulateSortDropdown();     // <-- เมธอดใหม่
        filterCategoryDropdown.onValueChanged.AddListener(delegate { LoadAndDisplayQuestions(); });
        filterDifficultyDropdown.onValueChanged.AddListener(delegate { LoadAndDisplayQuestions(); });
        sortDropdown.onValueChanged.AddListener(delegate { LoadAndDisplayQuestions(); });
        // --- จบส่วนที่เพิ่ม ---

        // Add button listeners
        openAddPanelButton.onClick.AddListener(ShowAddPanel);
        saveButton.onClick.AddListener(OnSaveQuestion);
        cancelButton.onClick.AddListener(CloseAddEditPanel);
        backButton.onClick.AddListener(GoToMenu); // <-- 3. เพิ่มบรรทัดนี้

        // Start by loading all questions and displaying the list
        addEditPanel.SetActive(false);
        questionListPanel.SetActive(true);
        LoadAndDisplayQuestions();
    }

    private void GoToMenu()
    {
        // ตรวจสอบให้แน่ใจว่าฉากเมนูของคุณชื่อ "Menu"
        SceneManager.LoadScene("Menu");
    }

    #region List Panel Logic

    /// <summary>
    /// Loads questions from the JSON file and displays them in the list.
    /// </summary>
    // --- 5. แทนที่เมธอดนี้ทั้งเมธอด ---
    /// <summary>
    /// Loads questions from the JSON file, filters, sorts, and displays them.
    /// </summary>
    private void LoadAndDisplayQuestions()
    {
        // 1. Load data
        allCustomQuestions = QuizDataHandler.LoadCustomQuestions();

        // 2. Apply Filtering
        // เราใช้ IEnumerable เพื่อประสิทธิภาพที่ดีกว่า (LINQ)
        IEnumerable<QuizQuestionPython> filteredQuestions = allCustomQuestions;

        int categoryFilterIndex = filterCategoryDropdown.value;
        if (categoryFilterIndex > 0) // Index 0 คือ "All Categories"
        {
            // เราต้อง -1 เพราะ Enum ของเราไม่มี "All"
            QuestionCategory category = (QuestionCategory)(categoryFilterIndex - 1);
            filteredQuestions = filteredQuestions.Where(q => q.category == category);
        }

        int difficultyFilterIndex = filterDifficultyDropdown.value;
        if (difficultyFilterIndex > 0) // Index 0 คือ "All Difficulties"
        {
            // เราต้อง -1 เพราะ Enum ของเราไม่มี "All"
            QuestionDifficulty difficulty = (QuestionDifficulty)(difficultyFilterIndex - 1);
            filteredQuestions = filteredQuestions.Where(q => q.difficulty == difficulty);
        }

        // 3. Apply Sorting
        int sortIndex = sortDropdown.value;
        switch (sortIndex)
        {
            case 0: // Latest (เพิ่มล่าสุด)
                filteredQuestions = filteredQuestions.Reverse();
                break;
            case 1: // Oldest (เก่าสุด)
                    // ไม่ต้องทำอะไร (ใช้ลำดับเดิมที่โหลดมา)
                break;
            case 2: // Difficulty (ระดับความยาก) (เดิมคือ 1)
                filteredQuestions = filteredQuestions.OrderBy(q => q.difficulty);
                break;
            case 3: // Category (หมวดหมู่) (เดิมคือ 2)
                filteredQuestions = filteredQuestions.OrderBy(q => q.category);
                break;
        }

        // 4. Clear old list items
        foreach (Transform child in questionListContainer)
        {
            Destroy(child.gameObject);
        }

        // 5. Instantiate new prefabs for each question (จาก List ที่ Filter และ Sort แล้ว)
        foreach (QuizQuestionPython question in filteredQuestions)
        {
            GameObject itemGO = Instantiate(questionListItemPrefab, questionListContainer);
            QuestionListItem itemScript = itemGO.GetComponent<QuestionListItem>();

            // Pass the question data and the functions to call on button press
            itemScript.Setup(question, OnEditQuestionClicked, OnDeleteQuestionClicked);
        }
    }

    /// <summary>
    /// Called by the "Edit" button on a QuestionListItem prefab.
    /// </summary>
    private void OnEditQuestionClicked(QuizQuestionPython question)
    {
        currentQuestionToEdit = question;
        panelTitleText.text = "Edit Question";
        feedbackText.text = "";

        // 1. Populate the panel with the question's data
        questionTextInput.text = question.questionText;
        if (question.answers != null && question.answers.Count == 4)
        {
            answer1Input.text = question.answers[0];
            answer2Input.text = question.answers[1];
            answer3Input.text = question.answers[2];
            answer4Input.text = question.answers[3];
        }

        categoryDropdown.value = (int)question.category;
        difficultyDropdown.value = (int)question.difficulty;
        correctAnswerDropdown.value = question.correctAnswerIndex;

        // 2. Show the panel
        questionListPanel.SetActive(false);
        addEditPanel.SetActive(true);
    }

    /// <summary>
    /// Called by the "Delete" button on a QuestionListItem prefab.
    /// </summary>
    private void OnDeleteQuestionClicked(QuizQuestionPython question)
    {
        // NOTE: You might want to add a "Are you sure?" confirmation popup here!
        // For now, we just delete it.

        allCustomQuestions.Remove(question);
        QuizDataHandler.SaveAllQuestions(allCustomQuestions);

        // Refresh the list
        LoadAndDisplayQuestions();
    }

    #endregion

    #region Add/Edit Panel Logic

    /// <summary>
    /// Called by the "+ Add New Question" button.
    /// </summary>
    private void ShowAddPanel()
    {
        currentQuestionToEdit = null; // Set to null because we are ADDING
        panelTitleText.text = "เพิ่มคำถามใหม่";
        feedbackText.text = "";

        ClearInputFields();

        questionListPanel.SetActive(false);
        addEditPanel.SetActive(true);
    }

    /// <summary>
    /// Closes the Add/Edit panel and returns to the list.
    /// </summary>
    private void CloseAddEditPanel()
    {
        addEditPanel.SetActive(false);
        questionListPanel.SetActive(true);
        currentQuestionToEdit = null; // Clear the editing state
    }

    /// <summary>
    /// Called by the main "Save" button in the Add/Edit panel.
    /// </summary>
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

        // --- 2. Check if we are EDITING or ADDING ---
        if (currentQuestionToEdit != null)
        {
            // We are EDITING an existing question
            UpdateQuestionData(currentQuestionToEdit);
            ShowFeedback("Question updated successfully!", false);
        }
        else
        {
            // We are ADDING a new question
            QuizQuestionPython newQuestion = new QuizQuestionPython();
            UpdateQuestionData(newQuestion);
            allCustomQuestions.Add(newQuestion); // Add it to our local list
            ShowFeedback("Question saved successfully!", false);
        }

        // --- 3. Save the ENTIRE list to the file ---
        try
        {
            QuizDataHandler.SaveAllQuestions(allCustomQuestions);

            // --- 4. Close panel and refresh list ---
            CloseAddEditPanel();
            LoadAndDisplayQuestions(); // Refresh the list to show changes
        }
        catch (Exception e)
        {
            ShowFeedback($"Error saving file: {e.Message}", true);
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// A helper function to apply the UI fields to a question object.
    /// </summary>
    private void UpdateQuestionData(QuizQuestionPython question)
    {
        question.questionText = questionTextInput.text;

        // Ensure the answers list exists
        if (question.answers == null)
        {
            question.answers = new List<string>(4);
        }

        // Clear and add new answers
        question.answers.Clear();
        question.answers.Add(answer1Input.text);
        question.answers.Add(answer2Input.text);
        question.answers.Add(answer3Input.text);
        question.answers.Add(answer4Input.text);

        question.category = (QuestionCategory)categoryDropdown.value;
        question.difficulty = (QuestionDifficulty)difficultyDropdown.value;
        question.correctAnswerIndex = correctAnswerDropdown.value;
    }

    private void ShowFeedback(string message, bool isError)
    {
        feedbackText.text = message;
        feedbackText.color = isError ? Color.red : Color.green;
    }

    private void ClearInputFields()
    //_comment_("This is the 'QuestionListItem.cs' script. It goes on the prefab for the list item.")
    {
        questionTextInput.text = "";
        answer1Input.text = "";
        answer2Input.text = "";
        answer3Input.text = "";
        answer4Input.text = "";

        categoryDropdown.value = 0;
        difficultyDropdown.value = 0;
        correctAnswerDropdown.value = 0;
    }

    #endregion

    #region Dropdown Population (Same as QuestionCreator)

    // --- 4. เพิ่ม 2 เมธอดนี้ ---
    private void PopulateFilterDropdowns()
    {
        // Category
        string[] categoryNames = Enum.GetNames(typeof(QuestionCategory));
        List<string> categoryOptions = new List<string>(categoryNames);
        categoryOptions.Insert(0, "All Categories"); // เพิ่ม "All"
        filterCategoryDropdown.ClearOptions();
        filterCategoryDropdown.AddOptions(categoryOptions);

        // Difficulty
        string[] difficultyNames = Enum.GetNames(typeof(QuestionDifficulty));
        List<string> difficultyOptions = new List<string>(difficultyNames);
        difficultyOptions.Insert(0, "All Difficulties"); // เพิ่ม "All"
        filterDifficultyDropdown.ClearOptions();
        filterDifficultyDropdown.AddOptions(difficultyOptions);
    }

    private void PopulateSortDropdown()
    {
        sortDropdown.ClearOptions();
        sortDropdown.AddOptions(new List<string>
    {
        "Sort by: Latest",  // Index 0
        "Sort by: Oldest",  // Index 1 <-- เพิ่มบรรทัดนี้
        "Sort by: Difficulty", // Index 2
        "Sort by: Category"  // Index 3
    });
    }
    // --- จบส่วนที่เพิ่ม ---

    private void PopulateCategoryDropdown()
    {
        string[] categoryNames = Enum.GetNames(typeof(QuestionCategory));
        List<string> options = new List<string>(categoryNames);
        categoryDropdown.ClearOptions();
        categoryDropdown.AddOptions(options);
    }

    private void PopulateDifficultyDropdown()
    {
        string[] difficultyNames = Enum.GetNames(typeof(QuestionDifficulty));
        List<string> options = new List<string>(difficultyNames);
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(options);
    }

    #endregion
}