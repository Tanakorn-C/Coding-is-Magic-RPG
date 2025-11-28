using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuizQuestionPython
{
    [TextArea(2, 5)]
    public string questionText;

    [Tooltip("List of 4 possible answers")]
    public List<string> answers;

    [Tooltip("index (0, 1, 2, or 3) of the correct answer in the list above")]
    public int correctAnswerIndex;

    [Tooltip("What category is this question?")]
    public QuestionCategory category;

    [Tooltip("What is the difficulty of this question?")]
    public QuestionDifficulty difficulty;

    // --- ADD THIS LINE ---
    [Tooltip("Explanation shown after the answer")]
    public string explanation;
    // ---------------------
}