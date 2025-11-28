using UnityEngine;
using UnityEngine.SceneManagement;

// นี่คือสคริปต์สำหรับ "วาป" ไปฉากอื่นแบบง่ายๆ
// โดย "ไม่" ยุ่งเกี่ยวกับระบบ Checkpoint หรือ GameDataPersistence
public class SimpleSceneLoader : MonoBehaviour
{
    [Tooltip("เลข Index ของฉากที่จะโหลด (ดูได้จาก File > Build Settings)")]
    public int sceneBuildIndex;

    private void Start()
    {
        // บังคับให้ Collider เป็น Trigger เสมอ
        // (เผื่อเราลืมติ๊กใน Inspector)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้า Player เดินมาชน
        if (other.CompareTag("Player"))
        {
            // โหลดฉากใหม่ทันที
            SceneManager.LoadScene(sceneBuildIndex);
        }
    }
}