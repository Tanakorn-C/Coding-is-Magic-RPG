using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerScripts : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed = 3f;

    private float movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        GameDataPersistence data = GameDataPersistence.Instance;
        
        if (data != null)
        {
            if (!string.IsNullOrEmpty(data.sceneToReturnTo))
            {
                if (data.justWonBattle)
                {
                    transform.position = data.winSpawnPosition;
                }
                else
                {
                    transform.position = data.loseSpawnPosition;
                }

                data.sceneToReturnTo = null;
                data.justWonBattle = false;
            }
        }
    }

    void Update()
    {

        // เช็คว่า DialogueManager ทำงานอยู่
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsTalking)
        {
            // ถ้ากำลังคุย ให้ movement เป็น 0 และหยุด
            movement = 0f;
            animator.SetFloat("Speed", 0f);
            return; // ออกจาก Update()
        }

        // ถ้าไม่ได้คุย
        movement = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(movement));

        // กลับด้านสไปรต์เวลาเดินซ้าย
        if (movement < 0) spriteRenderer.flipX = true;
        else if (movement > 0) spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movement * moveSpeed, rb.linearVelocity.y);
    }
}
