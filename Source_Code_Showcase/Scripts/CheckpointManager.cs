using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // Singleton Pattern
    public static CheckpointManager instance;

    public Transform initialSpawnPoint; // จุดเกิดแรกสุดของเกม

    private List<Checkpoint> allCheckpoints; // ลิสต์เก็บเสาทุกต้น
    private Vector3 currentSpawnPoint;     // ตำแหน่งจุดเกิดล่าสุด

    void Awake()
    {
        // ตั้งค่า Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        allCheckpoints = new List<Checkpoint>();
        // ตั้งค่าจุดเกิดแรกสุด
        if (initialSpawnPoint != null)
        {
            currentSpawnPoint = initialSpawnPoint.position;
        }
        else
        {
            currentSpawnPoint = transform.position; // ถ้าลืมตั้ง ก็ให้ใช้ตำแหน่งของ Manager แทน
        }
    }

    // เมธอดให้เสาต่างๆ มาลงทะเบียน
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (!allCheckpoints.Contains(checkpoint))
        {
            allCheckpoints.Add(checkpoint);
        }
    }

    // เมธอดสำคัญ: สั่งเปิดใช้งานเสาต้นใหม่
    public void SetActiveCheckpoint(Checkpoint newActiveCheckpoint)
    {
        // 1. อัปเดตตำแหน่งจุดเกิดใหม่
        currentSpawnPoint = newActiveCheckpoint.transform.position;

        // 2. วนลูปเสาทุกต้นในลิสต์
        foreach (Checkpoint cp in allCheckpoints)
        {
            if (cp == newActiveCheckpoint)
            {
                // ถ้าเป็นเสาต้นที่เพิ่งกด ให้เปิดใช้งาน
                cp.ActivateCheckpoint();
            }
            else
            {
                // ถ้าเป็นเสาต้นอื่น ให้ปิดการใช้งาน
                cp.DeactivateCheckpoint();
            }
        }
    }

    // เมธอดสำหรับให้ Player เรียกใช้ตอนตาย เพื่อดึงจุดเกิดล่าสุด
    public Vector3 GetCurrentSpawnPoint()
    {
        return currentSpawnPoint;
    }
}