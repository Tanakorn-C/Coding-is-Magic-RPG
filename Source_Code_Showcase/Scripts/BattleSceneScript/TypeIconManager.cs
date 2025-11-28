using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// 1. สร้าง helper class เพื่อจับคู่ Type กับ Sprite ใน Inspector
[System.Serializable]
public class TypeIconMapping
{
    public CreatureType type;
    public Sprite icon;
}

// 2. สร้าง Manager แบบ Singleton เพื่อเป็น "คลัง" เก็บไอคอน
public class TypeIconManager : MonoBehaviour
{
    // --- Singleton Setup ---
    public static TypeIconManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // (Optional) ถ้าคุณมีหลายฉาก
        }
    }
    // --- End Singleton Setup ---

    // 3. สร้าง List ให้คุณลาก Sprite 5 ธาตุมาใส่ใน Inspector
    [SerializeField] private List<TypeIconMapping> typeIcons;

    // 4. ฟังก์ชันหลักสำหรับให้สคริปต์อื่นมาดึงไอคอน
    public Sprite GetIconForType(CreatureType type)
    {
        // ค้นหาไอคอนที่ตรงกับ type ที่ขอมา
        TypeIconMapping mapping = typeIcons.FirstOrDefault(m => m.type == type);

        if (mapping != null)
        {
            return mapping.icon;
        }

        // ถ้าหาไม่เจอ (เช่น ลืมใส่ใน Inspector)
        Debug.LogWarning($"TypeIconManager: No icon found for type '{type}'.");
        return null;
    }
}