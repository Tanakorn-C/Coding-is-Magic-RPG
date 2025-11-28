// QuizDataHandler.cs (Modified for Editing/Saving)

using UnityEngine;
using System.IO; // Required for Path, File, Directory
using System.Collections.Generic;

public static class QuizDataHandler
{
    private static string GetCustomDataDirectory()
    {
        // In the Unity Editor, this will be your project folder, above "Assets".
        // In a build, it will be next to the .exe file.
        return Path.Combine(Application.dataPath, "..", "GameQuestions");
    }

    private static string GetCustomDataFilePath()
    {
        return Path.Combine(GetCustomDataDirectory(), "QuizTest.json");
    }

    [System.Serializable]
    private class QuestionListWrapper
    {
        // This MUST match the variable name in your JSON (e.g., "questions")
        public List<QuizQuestionPython> questions;
    }

    /// <summary>
    /// Loads all custom questions from the JSON file.
    /// </summary>
    public static List<QuizQuestionPython> LoadCustomQuestions()
    {
        string filePath = GetCustomDataFilePath();

        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                QuestionListWrapper wrapper = JsonUtility.FromJson<QuestionListWrapper>(json);
                if (wrapper != null && wrapper.questions != null)
                {
                    Debug.Log($"Loaded {wrapper.questions.Count} custom questions from {filePath}");
                    return wrapper.questions;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading questions: {e.Message}");
            }
        }

        // If file doesn't exist or is empty, return an empty list
        return new List<QuizQuestionPython>();
    }

    /// <summary>
    /// Saves a *new* question by adding it to the existing list.
    /// </summary>
    public static void AddQuestion(QuizQuestionPython newQuestion)
    {
        List<QuizQuestionPython> allQuestions = LoadCustomQuestions();
        allQuestions.Add(newQuestion);
        SaveAllQuestions(allQuestions); // Use the new save function
    }

    // --- NEW METHOD ---
    /// <summary>
    /// Saves the ENTIRE list of questions, overwriting the old file.
    /// This is used when editing or deleting.
    /// </summary>
    public static void SaveAllQuestions(List<QuizQuestionPython> allQuestions)
    {
        string directoryPath = GetCustomDataDirectory();
        string filePath = GetCustomDataFilePath();

        try
        {
            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"Created new directory at: {directoryPath}");
            }

            QuestionListWrapper wrapper = new QuestionListWrapper { questions = allQuestions };
            string json = JsonUtility.ToJson(wrapper, true); // 'true' for pretty print

            File.WriteAllText(filePath, json);

            Debug.Log($"Successfully saved {allQuestions.Count} questions to {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all questions: {e.Message}");
        }
    }
}