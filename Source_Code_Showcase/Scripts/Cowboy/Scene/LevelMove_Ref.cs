using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMove_Ref : MonoBehaviour 
{
    public int sceneBuildIndex;
    public string encounterID;
    
    [Header("Spawn Points")]
    public Transform winSpawnPoint;  
    public Transform loseSpawnPoint; 

    [Header("Final Encounter Settings")]
    [Tooltip("ติ๊กถ้าเป็น Encounter สุดท้ายที่ชนะแล้วจะไปฉากอื่น")]
    public bool isFinalEncounter = false;

    [Tooltip("ชื่อฉากที่จะโหลด 'ถ้า' ชนะ Encounter สุดท้าย")]
    public string nextSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameDataPersistence data = GameDataPersistence.Instance;

            if (data.defeatedEnemies.Contains(encounterID))
            {
                // ถ้าชนะแล้ว และ 'ไม่ใช่' บอสตัวสุดท้าย ก็ไม่ต้องทำอะไร
                // (แต่ถ้าคุณอยากให้บอสตัวสุดท้ายที่ชนะแล้วยังขวางทางอยู่ ก็ต้องแก้ตรงนี้)
                // (สมมติว่าถ้าชนะแล้วก็คือผ่านได้เลย)
                return;
            }

            // lose pos
            data.sceneToReturnTo = SceneManager.GetActiveScene().name;

            // win pos
            if (isFinalEncounter)
            {
                // ถ้าเป็นบอส: บอก GameData ว่าถ้าชนะ ให้ไปฉาก nextSceneName
                if (string.IsNullOrEmpty(nextSceneName))
                {
                    Debug.LogError($"!!! ERROR: {gameObject.name} ถูกติ๊กว่าเป็น Final Encounter แต่ไม่ได้ใส่ 'Next Scene Name'!");
                }
                data.sceneToLoadOnWin = nextSceneName;
            }
            else
            {
                // ถ้าเป็นศัตรูทั่วไป: ต้องล้างค่านี้ให้เป็น null เสมอ!
                data.sceneToLoadOnWin = null; 
            }
            
            data.justWonBattle = false; 
            data.currentEncounterID = this.encounterID;

            if (winSpawnPoint != null) { data.winSpawnPosition = winSpawnPoint.position; }
            else { Debug.LogError("!!! ERROR: ลืมลาก 'Win Spawn Point'"); }

            if (loseSpawnPoint != null) { data.loseSpawnPosition = loseSpawnPoint.position; }
            else { Debug.LogError("!!! ERROR: ลืมลาก 'Lose Spawn Point'"); }

            SceneManager.LoadScene(sceneBuildIndex);
        }
    }
}