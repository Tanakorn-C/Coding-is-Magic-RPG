using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class GameSaveData
{
    public List<string> defeatedEnemiesList;
    public int playerLevel;
    public int killCount;
    public List<string> solvedQuestionsList; // เพิ่ม: เซฟคำถามที่ตอบถูกด้วย
}

public class GameDataPersistenceMain : MonoBehaviour
{
    public static GameDataPersistenceMain Instance;

    [Header("Config")]
    public int maxLevel = 10;
    public int enemiesPerLevel = 2;

    public CreatureBase creatureToLoad;
    public int enemyLevelToLoad = 1;
    public string sceneToReturnTo;
    public List<string> solvedQuestions = new List<string>();

    public Creature PlayerCreature { get; private set; }
    public int currentEnemyKillCount = 0;
    public int currentPlayerLevel = 5;

    public void SetPlayerCreature(Creature creature) { PlayerCreature = creature; }

    public Vector2 winSpawnPosition;
    public Vector2 loseSpawnPosition;

    // --- (รวมธงทั้งหมดไว้ที่นี่) ---
    public bool justWonBattle = false;     // สำหรับนับ Kill / Level Up
    public bool returningFromBattle = false; // สำหรับย้ายตำแหน่ง Player
    public bool justLeveledUp = false;   // สำหรับแสดง UI Level Up

    public string currentEncounterID;
    public List<string> defeatedEnemies = new List<string>();
    private string saveFilePath;

    void Awake()
    {
        Debug.Log($"🟢 GameData AWAKE! ID: {gameObject.GetInstanceID()}");

        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
            LoadGame();
        }
        else
        {
            Debug.Log($"🔴 Found Duplicate GameData (ID: {gameObject.GetInstanceID()}) -> Destroying it.");
            Destroy(gameObject);
        }
    }


    public void PlayerLostOrRan()
    {
        Debug.Log("Player lost or ran. Returning to MainScene...");

        if (PlayerCreature != null)
        {
            // 1. ฮีลเลือด + รีเซ็ต PP
            PlayerCreature.Heal(PlayerCreature.MaxHP);
            foreach (var attack in PlayerCreature.Attacks)
            {
                attack.Pp = attack.Base.PP;
            }
            Debug.Log("❤️ Player HP & PP fully restored.");
        }

        // 2. ตั้งธง
        justWonBattle = false;
        returningFromBattle = true;

        // 3. 🔥 (สำคัญมาก!) บันทึกข้อมูล HP ที่เต็มแล้วลงไฟล์ทันที
        // ไม่งั้นตอนโหลดฉาก MainScene มันอาจจะไปโหลดไฟล์เก่าที่ HP=0 มาทับ
        SaveGame();

        // 4. เล่นเพลง
        if (GetComponent<AudioSource>() != null && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().Play();
        }

        // 5. โหลดฉาก
        SceneManager.LoadScene(sceneToReturnTo);
    }

    // ==========================================================
    // --- PlayerWonBattle() ---
    // ==========================================================
    public void PlayerWonBattle()
    {
        Debug.Log("⚔️ Processing Victory Logic...");

        justWonBattle = true;
        returningFromBattle = true;

        if (!string.IsNullOrEmpty(currentEncounterID) && !defeatedEnemies.Contains(currentEncounterID))
        {
            defeatedEnemies.Add(currentEncounterID);
        }

        HandleExperienceAndLevelUp();
        SaveGame();

        if (this.currentEncounterID == "bigboss")
        {
            Debug.Log("FINAL BOSS DEFEATED! Loading End Credits...");
            if (GetComponent<AudioSource>() != null) GetComponent<AudioSource>().Stop();
            SceneManager.LoadScene("C_Main");
        }
        else
        {
            Debug.Log("Normal enemy defeated. Returning to MainScene...");
            if (GetComponent<AudioSource>() != null && !GetComponent<AudioSource>().isPlaying)
            {
                GetComponent<AudioSource>().Play();
            }
            SceneManager.LoadScene(sceneToReturnTo);
        }
    }

    private void HandleExperienceAndLevelUp()
    {
        currentEnemyKillCount++;
        Debug.Log($"Enemy Defeated! Kill Count: {currentEnemyKillCount}/{enemiesPerLevel}");

        if (currentEnemyKillCount >= enemiesPerLevel)
        {
            if (PlayerCreature != null && PlayerCreature.Level < maxLevel)
            {
                currentEnemyKillCount = 0;
                PlayerCreature.Level++;
                currentPlayerLevel = PlayerCreature.Level;
                justLeveledUp = true;
                Debug.Log($"🎉 LEVEL UP! Player is now Level {currentPlayerLevel}");

                // (Optional) ฮีลเต็มตอน Level Up ก็ดีนะ
                PlayerCreature.Heal(PlayerCreature.MaxHP);
            }
        }
    }

    // ==========================================================
    // --- (FIXED) StartNewGame() แก้ไขเรื่องรีเซ็ตตำแหน่ง ---
    // ==========================================================
    public void ResetGameData() // เปลี่ยนชื่อให้ตรงกับ Interface หรือใช้ StartNewGame ก็ได้
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        Debug.Log("🗑️ Starting New Game: Clearing all data...");

        // 1. เคลียร์ข้อมูลการเล่น
        defeatedEnemies.Clear();
        solvedQuestions.Clear(); // เคลียร์คำถามที่ตอบแล้ว
        currentEnemyKillCount = 0;

        // 2. รีเซ็ตตำแหน่งเกิด (ให้กลับไปจุดเริ่มต้น)
        winSpawnPosition = Vector2.zero;
        loseSpawnPosition = Vector2.zero;
        returningFromBattle = false;
        justWonBattle = false;

        // 3. รีเซ็ต Player (แก้ตรงนี้ครับ)
        currentPlayerLevel = 5; // ✅ แก้เป็น 5 ตามที่ต้องการ
        PlayerCreature = null;  // สั่งให้ PlayerController สร้างตัวใหม่ที่เลเวล 5

        // 4. ลบไฟล์เซฟ
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
                Debug.Log("✅ Save file deleted.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to delete save file: {e.Message}");
            }
        }
    }

    public void SaveGame()
    {
        try
        {
            GameSaveData dataToSave = new GameSaveData();
            dataToSave.defeatedEnemiesList = this.defeatedEnemies;
            dataToSave.solvedQuestionsList = this.solvedQuestions; // บันทึกคำถามที่ตอบแล้ว

            if (this.PlayerCreature != null)
                dataToSave.playerLevel = this.PlayerCreature.Level;
            else
                dataToSave.playerLevel = this.currentPlayerLevel;

            dataToSave.killCount = this.currentEnemyKillCount;

            string json = JsonUtility.ToJson(dataToSave, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found. Starting fresh.");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(json);

            this.defeatedEnemies = loadedData.defeatedEnemiesList ?? new List<string>();
            this.solvedQuestions = loadedData.solvedQuestionsList ?? new List<string>(); // โหลดคำถามที่ตอบแล้ว
            this.currentPlayerLevel = loadedData.playerLevel;
            this.currentEnemyKillCount = loadedData.killCount;

            if (PlayerCreature != null)
            {
                PlayerCreature.Level = this.currentPlayerLevel;
                // PlayerCreature.Heal(PlayerCreature.MaxHP); // (Optional) ฮีลตอนโหลดเกม
            }

            Debug.Log("Game loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            this.defeatedEnemies = new List<string>();
        }
    }

    public bool IsEnemyDefeated(string enemyID)
    {
        if (string.IsNullOrEmpty(enemyID)) return false;
        return defeatedEnemies.Contains(enemyID);
    }
}