using UnityEngine;

[System.Serializable] // ทำให้โชว์ใน Inspector ได้
public class Quiz_Question
{
    [TextArea(3, 5)] // ทำให้พิมพ์คำถามยาวๆ ใน Inspector ง่ายขึ้น
    public string questionText;
    
    public string[] answers = new string[4]; // เก็บ 4 ตัวเลือก
    public int correctAnswerIndex; // Index ของคำตอบที่ถูก (0, 1, 2, หรือ 3)
    
    [TextArea(2, 4)]
    public string explanation; // ข้อความเฉลย เวลาตอบผิด
}