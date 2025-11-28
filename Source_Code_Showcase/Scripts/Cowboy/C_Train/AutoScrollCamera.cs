using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    [Tooltip("ความเร็วที่กล้องจะเลื่อนไปทางขวา")]
    public float scrollSpeed = 1.0f;

    void Update()
    {
        // สั่งให้กล้องเลื่อนไปทางขวา (แกน X) ตลอดเวลา
        // โดยใช้ scrollSpeed และ Time.deltaTime เพื่อให้ความเร็วคงที่
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
    }
}