using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class PlayerScripts : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed = 3f;

    private float movement;              // -1,0,1 จาก A/D หรือ ลูกศร
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        movement = Input.GetAxisRaw("Horizontal");      // ต้องมี "Horizontal" ใน Input Manager
        animator.SetFloat("Speed", Mathf.Abs(movement)); // ใช้กับ Blend Tree (Idle=0, Run=1)

        // กลับด้านสไปรต์เวลาเดินซ้าย
        if (movement < 0) spriteRenderer.flipX = true;
        else if (movement > 0) spriteRenderer.flipX = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movement * moveSpeed, rb.linearVelocity.y);
    }
}
