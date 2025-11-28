using System;
using UnityEngine;

// 1. สร้าง Enum เพื่อแยกประเภทท่า (ใส่ไว้เหนือ Class หรือแยกไฟล์ก็ได้)
public enum MoveCategory
{
    Physical,   // กายภาพ: ใช้ Attack vs Defense
    Special,    // เวทมนตร์: ใช้ SpAttack vs SpDefense
    Status      // ท่าสถานะ: เพิ่มพลัง/ลดพลัง (ไม่มีดาเมจ)
}

[CreateAssetMenu(fileName = "Attack", menuName = "Creature/New Attack")]
public class AttackBase : ScriptableObject
{
    [SerializeField] private string name;
    public string Name => name;

    [TextArea]
    [SerializeField] private string description;
    public string Description => description;

    [SerializeField] private CreatureType type;
    public CreatureType Type => type;

    // 2. เพิ่มช่องเลือกประเภทท่า
    [SerializeField] private MoveCategory category;
    public MoveCategory Category => category;

    [SerializeField] private int power;
    public int Power => power;

    [SerializeField] private int accuracy;
    public int Accuracy => accuracy;

    [SerializeField] private int pp;
    public int PP => pp;
}