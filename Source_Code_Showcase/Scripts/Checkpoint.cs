using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite activeSprite;     // ภาพตอนที่เสานี้ทำงาน
    public Sprite inactiveSprite;   // ภาพตอนที่เสานี้ไม่ทำงาน

    [Header("State")]
    public bool isActivated = false; // สถานะว่าทำงานอยู่หรือไม่

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // ดึง SpriteRenderer ของเสานี้มาเก็บไว้
        spriteRenderer = GetComponent<SpriteRenderer>();
        // เริ่มต้นให้เป็นภาพไม่ทำงาน
        spriteRenderer.sprite = inactiveSprite;
    }

    void Start()
    {
        // เมื่อเริ่มเกม ให้ไปลงทะเบียนกับตัวจัดการ
        CheckpointManager.instance.RegisterCheckpoint(this);
    }

    // ฟังก์ชันสำหรับเปิดการทำงาน (เรียกโดย Manager)
    public void ActivateCheckpoint()
    {
        isActivated = true;
        spriteRenderer.sprite = activeSprite;
    }

    // ฟังก์ชันสำหรับปิดการทำงาน (เรียกโดย Manager)
    public void DeactivateCheckpoint()
    {
        isActivated = false;
        spriteRenderer.sprite = inactiveSprite;
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย Player เมื่อผู้เล่นมากด
    public void OnPlayerInteract()
    {
        // ถ้าเสานี้ยังไม่ทำงาน ให้สั่ง Manager เปิดใช้งานเสานี้
        if (!isActivated)
        {
            CheckpointManager.instance.SetActiveCheckpoint(this);
        }
    }
}