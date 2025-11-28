using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    public Transform spawnPoint;    // (ลากจุดเกิดตอนแพ้มาใส่)
    public Transform Checkpoint1;   // (ลากจุดเกิดตอนชนะมาใส่)

    // --- (MODIFY) เปลี่ยนจาก string เป็น CreatureBase ---
    public CreatureBase enemyToEncounter; // (ลาก Samurai.asset มาใส่ตรงนี้)

    private void StartBattle()
    {
        // 1. ตั้งค่า "ธง" ทั้งหมดใน Singleton
        GameDataPersistenceMain.Instance.sceneToReturnTo = "MainScene"; // (หรือชื่อฉากหลักของคุณ)

        // --- (MODIFY) ---
        // 1B. ส่ง "Asset" ทั้งก้อนไปเก็บไว้ใน Singleton
        GameDataPersistenceMain.Instance.creatureToLoad = enemyToEncounter;

        // (เราไม่จำเป็นต้องใช้ currentEncounterID ที่เป็น string อีกต่อไป
        // แต่ถ้าคุณยังต้องการใช้เพื่อบันทึกว่า "ชนะตัวไหน" ก็ใส่ไว้ได้)
        GameDataPersistenceMain.Instance.currentEncounterID = enemyToEncounter.CreatureName;

        // 2. (สำคัญ!) บันทึกตำแหน่งที่จะใช้หลังต่อสู้เสร็จ
        GameDataPersistenceMain.Instance.winSpawnPosition = Checkpoint1.position;
        GameDataPersistenceMain.Instance.loseSpawnPosition = spawnPoint.position;

        // 3. โหลดฉากต่อสู้
        SceneManager.LoadScene("BattleScene");
    }
}