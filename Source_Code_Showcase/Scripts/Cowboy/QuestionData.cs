using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string category;
    public string difficulty;
    public string questionText;
    public List<string> answers;
    public int correctAnswerIndex;
}

[System.Serializable]
public class QuestionCollection
{
    public List<QuestionData> questions;
}