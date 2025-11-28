using UnityEngine;

public class ClickToTalk : MonoBehaviour
{
    // ตัวแปรเดิมของคุณสำหรับเก็บ Dialogue
    [TextArea(3, 5)] 
    public string[] dialogueLines;

    [Header("Interaction UI")]
    [Tooltip("ลาก PromptCanvas ที่เป็นลูกของ NPC มาใส่ในช่องนี้")]
    public GameObject interactionPromptCanvas; // ตัวแปรสำหรับเก็บ UI "Press E"

    private bool playerInRange = false; // ตรวจสอบว่าผู้เล่นอยู่ในระยะหรือไม่
    private bool hasTalked = false;     // เช็คว่าคุยไปหรือยัง

    void Start()
    {
        // ซ่อน UI ตอนเริ่มเกม
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.SetActive(false);
        }
    }

    // ทำงานเมื่อ Player วิ่งเข้ามาในระยะ Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ตรวจสอบว่าเป็น Player
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // แสดง UI "Press E"
            if (interactionPromptCanvas != null && !hasTalked)
            {
                interactionPromptCanvas.SetActive(true);
            }
        }
    }

    // ทำงานเมื่อ Player วิ่งออกจากระยะ Trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // ซ่อน UI "Press E"
            if (interactionPromptCanvas != null)
            {
                interactionPromptCanvas.SetActive(false);
            }
        }
    }

    // ทำงานทุกเฟรม
    void Update()
    {
        // ตรวจสอบว่าผู้เล่นอยู่ในระยะ และ กดปุ่ม "E", และ "ยังไม่เคยคุย"
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !hasTalked)
        {
            // เรียกใช้ Logic การพูดคุย
            StartDialogue();
        }
    }

    // เมธอดสำหรับเริ่มการพูดคุย
    void StartDialogue()
    {
        hasTalked = true;

        // เรียก DialogueManager
        Debug.Log("Start Dialogue with E!");
        DialogueManager.Instance.StartDialogue(dialogueLines);

        // (แนะนำ) ซ่อนปุ่ม "Press E" อีกครั้งเมื่อเริ่มคุย
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.SetActive(false);
        }
    }
}