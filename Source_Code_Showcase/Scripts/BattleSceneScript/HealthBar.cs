using System;
using System.Collections;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public GameObject healthBar;
    [SerializeField] private float updateSpeed = 1f; // ความเร็วในการเลื่อนหลอด


    public void SetHP(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        healthBar.transform.localScale = new Vector3(normalizedValue, 1, 1);
    }

    public IEnumerator SetSmoothHP(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        
        float currentScale = healthBar.transform.localScale.x;

        // Loop นี้จะทำงานจนกว่าค่าปัจจุบันจะ "เกือบเท่ากับ" ค่าเป้าหมาย
        while (Mathf.Approximately(currentScale, normalizedValue) == false)
        {
            // "เลื่อน" ค่า currentScale ไปหา normalizedValue ด้วยความเร็ว
            currentScale = Mathf.MoveTowards(currentScale, normalizedValue, updateSpeed * Time.deltaTime);
            healthBar.transform.localScale = new Vector3(currentScale, 1, 1);
            
            yield return null; // รอเฟรมถัดไป
        }
        healthBar.transform.localScale = new Vector3(normalizedValue, 1, 1);
    }
}