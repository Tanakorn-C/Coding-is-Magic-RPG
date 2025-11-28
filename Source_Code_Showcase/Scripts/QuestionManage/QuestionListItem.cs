// This script goes on the Prefab for the question item in the list.
// It holds references to its own UI elements and notifies the manager when
// its buttons are clicked.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Required for Action<>

public class QuestionListItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text categoryText; // --- ADD THIS LINE ---
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    // Internal data
    private QuizQuestionPython _currentQuestion;

    /// <summary>
    /// This is called by the QuestionManager to set up the item.
    /// </summary>
    /// <param name="question">The data for this question</param>
    /// <param name="onEditCallback">The function to call when 'Edit' is clicked</param>
    /// <param name="onDeleteCallback">The function to call when 'Delete' is clicked</param>
    public void Setup(QuizQuestionPython question, Action<QuizQuestionPython> onEditCallback, Action<QuizQuestionPython> onDeleteCallback)
    {
        _currentQuestion = question;

        // 1. Set the text
        // Truncate question text if it's too long
        if (question.questionText.Length > 100)
        {
            questionText.text = question.questionText.Substring(0, 100) + "...";
        }
        else
        {
            questionText.text = question.questionText;
        }

        difficultyText.text = question.difficulty.ToString();
        categoryText.text = question.category.ToString();

        // --- 2. นี่คือส่วนที่เพิ่มเข้ามา ---
        // ตั้งค่าสีตัวอักษรตามระดับความยาก
        switch (question.difficulty)
        {
            case QuestionDifficulty.Easy:
                difficultyText.color = Color.green; // สีเขียว (ผมเพิ่มให้ครับ)
                break;
            case QuestionDifficulty.Normal:
                difficultyText.color = new Color32(255, 165, 0, 255); // สีส้ม (Orange)
                break;
            case QuestionDifficulty.Hard: // "็ฟพก" (Hard)
                difficultyText.color = Color.red; // สีแดง
                break;
            default:
                // หากมีระดับความยากอื่น ๆ ให้เป็นสีขาว (หรือสี default ของคุณ)
                difficultyText.color = Color.white;
                break;
        }
        // --- จบส่วนที่เพิ่มเข้ามา ---


        // 3. Set up button listeners
        // We store the callback functions provided by the manager
        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(() => onEditCallback(_currentQuestion));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDeleteCallback(_currentQuestion));
    }
}