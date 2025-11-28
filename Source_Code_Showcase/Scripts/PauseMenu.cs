using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel; // ลาก Panel มาใส่ที่นี่

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // ชื่อฉากเมนูของคุณ

    private bool isPaused = false;

    void Update()
    {
        // กด Esc เพื่อ สลับสถานะ หยุด/เล่น
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false); // ซ่อนเมนู
        Time.timeScale = 1f;             // เวลาเดินปกติ
        isPaused = false;
    }

    void PauseGame()
    {
        pauseMenuPanel.SetActive(true);  // โชว์เมนู
        Time.timeScale = 0f;             // หยุดเวลา (Freeze Time)
        isPaused = true;
    }

    public void QuitToMenu()
    {
        // สำคัญ! ต้องปรับเวลาให้กลับมาเดินก่อนโหลดฉากใหม่ 
        // ไม่งั้นฉากหน้าเมนูจะค้าง
        Time.timeScale = 1f; 
        
        // โหลดฉากเมนู (ตรวจสอบชื่อฉากใน Build Settings ด้วยนะ)
        SceneManager.LoadScene(mainMenuSceneName);
    }
}