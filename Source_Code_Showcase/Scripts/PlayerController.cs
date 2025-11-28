using UnityEngine;
// ไม่ต้องใช้ UI หรือ DOTween ในนี้แล้ว

public class PlayerController : MonoBehaviour
{
    [Header("Initialization")]
    [SerializeField] private CreatureBase playerBase;
    [SerializeField] private int initialPlayerLevel = 5;

    void Start()
    {
        if (GameDataPersistenceMain.Instance == null) return;

        // 1. จัดการจุดเกิด
        HandlePlayerSpawning();

        // 2. สร้าง/โหลดข้อมูล Player
        if (GameDataPersistenceMain.Instance.PlayerCreature == null)
        {
            int levelToUse = GameDataPersistenceMain.Instance.currentPlayerLevel;
            if (levelToUse < 1) levelToUse = initialPlayerLevel;

            Creature newPlayerCreature = new Creature(playerBase, levelToUse);
            GameDataPersistenceMain.Instance.SetPlayerCreature(newPlayerCreature);
            GameDataPersistenceMain.Instance.currentPlayerLevel = levelToUse;
        }
        else
        {
            GameDataPersistenceMain.Instance.PlayerCreature.Level = GameDataPersistenceMain.Instance.currentPlayerLevel;
        }

        // เช็คแค่เรื่อง Battle (ส่วน Level Up ปล่อยให้ Script UI จัดการเอง)
        if (GameDataPersistenceMain.Instance.justWonBattle)
        {
            GameDataPersistenceMain.Instance.justWonBattle = false;
        }
    }

    private void HandlePlayerSpawning()
    {
        if (GameDataPersistenceMain.Instance == null) return;

        if (GameDataPersistenceMain.Instance.justWonBattle)
        {
            if (GameDataPersistenceMain.Instance.winSpawnPosition != Vector2.zero)
            {
                this.transform.position = GameDataPersistenceMain.Instance.winSpawnPosition;
            }
        }
        else
        {
            if (GameDataPersistenceMain.Instance.loseSpawnPosition != Vector2.zero)
            {
                this.transform.position = GameDataPersistenceMain.Instance.loseSpawnPosition;
            }
        }
    }

    public void UsePotion(ItemBase potion)
    {
        Creature player = GameDataPersistenceMain.Instance.PlayerCreature;
        if (player == null) return;

        if (player.HP < player.MaxHP)
        {
            player.Heal(potion.effectAmount);
            Debug.Log($"Player HP: {player.HP}/{player.MaxHP}");
        }
    }
}