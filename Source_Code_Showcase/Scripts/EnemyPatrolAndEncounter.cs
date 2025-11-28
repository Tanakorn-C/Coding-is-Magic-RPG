using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolAndEncounter : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float battleDistance = 0.5f; // 🔥 ระยะห่างที่จะเริ่มสู้ (เช่น 0.5 เมตร)

    [Header("Encounter Settings")]
    public CreatureBase enemyToEncounter;
    public int enemyLevel = 5;
    public string enemyEncounterID;
    [SerializeField] private string sceneToLoad = "BattleScene";

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public Transform Checkpoint1;

    private Transform playerTransform;
    private bool isChasing = false;
    private Animator anim;
    private Rigidbody2D rb;
    private bool isBattling = false;
    private bool isStartingBattle = false;

    void Start()
    {
        if (GameDataPersistenceMain.Instance != null &&
            GameDataPersistenceMain.Instance.IsEnemyDefeated(enemyEncounterID))
        {
            isBattling = true;
            gameObject.SetActive(false);
            return;
        }

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isBattling)
        {
            Debug.Log("เห็น Player แล้ว! เริ่มไล่");
            playerTransform = other.transform;
            isChasing = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player หนีไปแล้ว! หยุดไล่");
            isChasing = false;
            playerTransform = null;
            if (anim != null) anim.SetBool("isWalking", false);
        }
    }

    void Update()
    {
        if (isChasing && playerTransform != null && !isBattling && !isStartingBattle)
        {
            // 1. เช็คระยะทาง
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // 2. ถ้าใกล้พอ -> เริ่มสู้ทันที (ไม่ต้องรอชน)
            if (distance <= battleDistance)
            {
                StartBattle();
                return; // หยุดเดินทันที
            }

            // 3. เดินเข้าหา
            if (anim != null) anim.SetBool("isWalking", true);

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ลบ OnCollisionEnter2D ทิ้งไปได้เลย หรือเก็บไว้กันเหนียวก็ได้
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBattling)
        {
            StartBattle();
        }
    }

    private void StartBattle()
    {
        if (isStartingBattle) return;
        isStartingBattle = true;
        isBattling = true; // หยุด AI ทันที

        // หยุดการเคลื่อนที่
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isWalking", false);

        // Setup GameData
        if (GameDataPersistenceMain.Instance != null)
        {
            GameDataPersistenceMain.Instance.creatureToLoad = enemyToEncounter;
            GameDataPersistenceMain.Instance.enemyLevelToLoad = enemyLevel;
            GameDataPersistenceMain.Instance.sceneToReturnTo = SceneManager.GetActiveScene().name;
            GameDataPersistenceMain.Instance.currentEncounterID = enemyEncounterID;

            // ใช้ตำแหน่ง playerTransform ล่าสุดที่จับได้
            if (playerTransform != null)
            {
                GameDataPersistenceMain.Instance.winSpawnPosition = playerTransform.position;
                GameDataPersistenceMain.Instance.loseSpawnPosition = playerTransform.position;
            }

            if (GameDataPersistenceMain.Instance.GetComponent<AudioSource>() != null)
            {
                GameDataPersistenceMain.Instance.GetComponent<AudioSource>().Stop();
            }
        }

        Debug.Log("🚀 Loading Battle Scene...");
        SceneManager.LoadScene(sceneToLoad);
    }
}