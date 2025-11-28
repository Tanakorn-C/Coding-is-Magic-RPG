using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class LevelUpUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2.0f, 0);
    [SerializeField] private float displayDuration = 3.0f;

    [Header("References")]
    [SerializeField] private TMP_Text levelUpText;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioSource audioSource;

    private Transform playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        bool shouldShow = false;

        // ตรวจสอบข้อมูลจาก GameData
        if (GameDataPersistenceMain.Instance != null)
        {
            if (GameDataPersistenceMain.Instance.justLeveledUp)
            {
                shouldShow = true;
                GameDataPersistenceMain.Instance.justLeveledUp = false;
            }
        }

        if (shouldShow)
        {
            Debug.Log("✅ Showing Level Up UI!");

            // 1. เปิด GameObject ทันที
            gameObject.SetActive(true);

            // 2. 🔥 บังคับขนาดเป็น 1 ทันที (แก้ปัญหา Scale 0 ในรูป)
            // ทำตรงนี้เลย ไม่ต้องรอ Coroutine เพื่อกันเหนียว
            transform.localScale = Vector3.one;

            // 3. เริ่ม Coroutine
            StartCoroutine(ShowLevelUpSequence());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (gameObject.activeSelf && playerTransform != null)
        {
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position + uiOffset);

                // 🔥 สำคัญ: ต้องบังคับ Z เป็น 0 เสมอ ไม่งั้น UI จะลอยไปหลังกล้อง
                screenPos.z = 0;

                transform.position = screenPos;
            }
        }
    }

    IEnumerator ShowLevelUpSequence()
    {
        // ใช้ Realtime เพื่อกันกรณีเกม Pause (TimeScale = 0)
        yield return new WaitForSecondsRealtime(0.1f);

        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        if (levelUpText != null && GameDataPersistenceMain.Instance != null)
        {
            int currentLv = GameDataPersistenceMain.Instance.currentPlayerLevel;
            levelUpText.text = $"LEVEL UP!\nLv. {currentLv}";
            Debug.Log($"Text Updated to Lv. {currentLv}");
        }

        // ถ้าอยากใช้ Animation ให้เอา comment ออก
        // (แต่ต้องมั่นใจว่า DOTween setup ผ่านแล้ว ไม่งั้นมันจะค้างที่ 0)
        // transform.localScale = Vector3.zero;
        // transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true); // SetUpdate(true) เพื่อให้ขยับแม้เกม Pause

        // รอเวลา (ใช้ Realtime เผื่อเกม Pause)
        yield return new WaitForSecondsRealtime(displayDuration);

        // ปิด Panel
        gameObject.SetActive(false);
    }
}