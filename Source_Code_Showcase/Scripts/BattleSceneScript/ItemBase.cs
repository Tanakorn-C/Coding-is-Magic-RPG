using UnityEngine;

// Enum เพื่อกำหนดประเภทของไอเทม
public enum ItemEffect
{
    None,
    Heal
}

// [CreateAssetMenu] จะทำให้เราสร้างไฟล์ไอเทมจากเมนูใน Unity ได้
[CreateAssetMenu(fileName = "Item", menuName = "BackpackItem/Create new Item")]
public class ItemBase : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;

    [Header("Item Effect")]
    public ItemEffect effectType;
    public int effectAmount; // เช่น 10 (สำหรับ 10 HP)

    // เราสามารถเพิ่มตัวแปรอื่นๆ ได้ในอนาคต เช่น
    // public Sprite itemSprite;
    // public bool isUsableInBattle;
}