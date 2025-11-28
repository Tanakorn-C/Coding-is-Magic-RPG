using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E; // ปุ่มที่จะใช้กด (เช่น E หรือ F)

    private Checkpoint currentCheckpoint = null; // เสาที่อยู่ใกล้ที่สุด

    void Update()
    {
        // เช็คว่า: 1. อยู่ใกล้เสา และ 2. กดปุ่ม
        if (currentCheckpoint != null && Input.GetKeyDown(interactKey))
        {
            // เรียกฟังก์ชันที่เสาต้นนั้น
            currentCheckpoint.OnPlayerInteract();
        }
    }

    // เมื่อผู้เล่นเดินไปชน Trigger ของเสา
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            // เก็บไว้ว่าตอนนี้อยู่ใกล้เสาต้นนี้นะ
            currentCheckpoint = other.GetComponent<Checkpoint>();
        }
    }

    // เมื่อผู้เล่นเดินออกจาก Trigger ของเสา
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            // ถ้าเดินออกจากเสาที่เคยอยู่ใกล้ ก็เคลียร์ค่าทิ้ง
            if(other.GetComponent<Checkpoint>() == currentCheckpoint)
            {
                currentCheckpoint = null;
            }
        }
    }
}