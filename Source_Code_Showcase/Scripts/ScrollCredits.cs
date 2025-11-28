using UnityEngine;
using UnityEngine.SceneManagement; // (ต้องมีเผื่อใช้ตอนกลับเมนู)

public class ScrollCredits : MonoBehaviour
{
    // ความเร็วในการเลื่อน (ปรับได้ใน Inspector)
    public float scrollSpeed = 50f;

    // (ส่วนแถม)
    public float timeToReturn = 30f; // ตั้งเวลา (วินาที) ที่จะกลับไปหน้าเมนู
    public string menuSceneName = "Menu"; // ชื่อฉากเมนูของคุณ

    private float timer = 0f;

    void Update()
    {
        // 1. สั่งให้ Object นี้ "เลื่อนขึ้น" ตลอดเวลา
        transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);


        // 2. (ส่วนแถม) เริ่มนับเวลา
        timer += Time.deltaTime;

        // 3. (ส่วนแถม) ถ้าเวลาถึงที่กำหนด ให้กลับไปหน้าเมนู
        if (timer >= timeToReturn)
        {
            Debug.Log("Credits finished, returning to menu.");
            SceneManager.LoadScene(menuSceneName);
        }
    }
}