using UnityEngine;

[CreateAssetMenu(fileName = "Encounter_", menuName = "QuizGame/Encounter Data")]
public class EncounterData : ScriptableObject
{
    public GameObject enemyPrefab;
    public string enemyName;
    
    // --- WE REMOVED THIS ---
    // public QuestionBank questionBank; 

    // --- AND ADDED THIS ---
    [Header("Quiz Settings")]
    [Tooltip("What category of questions will this enemy ask?")]
    public QuestionCategory categoryToAsk;
    
    [Tooltip("What difficulty of questions will this enemy ask?")]
    public QuestionDifficulty difficultyToAsk;
}