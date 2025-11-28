using UnityEngine;

public class AutoParallax : MonoBehaviour
{
    [Tooltip("ลาก Main Camera มาใส่ในช่องนี้")]
    public Transform cameraTransform;
    
    [Tooltip("ยิ่งเลขน้อย (เช่น 0.1) ยิ่งดูไกล / ยิ่งเลขมาก (เช่น 0.9) ยิ่งดูใกล้")]
    [Range(0f, 1f)]
    public float parallaxFactor;

    private Vector3 startPosition; // ตำแหน่งเริ่มต้นของ BG

    void Start()
    {
        // ถ้าลืมลากกล้องมาใส่ ให้มันหากล้องหลักเอง
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // จำตำแหน่งเริ่มต้นของ BG นี้ไว้
        startPosition = transform.position;
    }

    void Update()
    {
        // คำนวณว่ากล้องเคลื่อนที่ไปจากจุดเริ่มต้นเท่าไหร่ แล้วคูณด้วย parallaxFactor
        // (เราไม่ต้องสนใจตำแหน่งเริ่มต้นของกล้อง สนใจแค่ตำแหน่งปัจจุบันของมัน)
        float distance = (cameraTransform.position.x) * parallaxFactor;

        // ขยับ BG ไปตามระยะทางที่คำนวณได้ (เทียบจากจุดเริ่มต้นของ BG)
        transform.position = new Vector3(startPosition.x + distance, startPosition.y, transform.position.z);
    }
}