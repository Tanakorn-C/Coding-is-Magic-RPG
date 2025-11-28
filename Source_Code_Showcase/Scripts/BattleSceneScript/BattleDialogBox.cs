using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;

    [SerializeField] private GameObject actionSelect;
    [SerializeField] private GameObject attackSelect;
    [SerializeField] private GameObject attackDescription;
    [SerializeField] private List<Button> actionTexts;
    [SerializeField] private List<Text> attackTexts;
    [SerializeField] private List<GameObject> attackbuttons;
    [SerializeField] private Text ppText;

    // --- (START) MODIFICATION ---
    // [SerializeField] private Text typeText; // 1. ลบ Text เก่าทิ้ง
    [SerializeField] private Image attackTypeIcon; // 2. เพิ่ม Image ใหม่สำหรับไอคอน
    // --- (END) MODIFICATION ---
    [SerializeField] private BattleUnit playerUnit;
    public bool isWriting = false;
    public float characterPerSecond;
    public Button primaryAttackbutton;

    // --- ส่วนที่เพิ่มเข้ามาสำหรับ QUIZ UI ---
    [Header("Quiz Panel UI")]
    [SerializeField] private GameObject quizPanel; 
    [SerializeField] private Text quizQuestionText; 
    [SerializeField] private List<Text> quizAnswerTexts; 
    [SerializeField] private List<Button> quizAnswerButtons; 
    
    // 🔥 เพิ่มตัวแปรนี้สำหรับ Explanation (อย่าลืมลากใส่ใน Inspector)
    [SerializeField] private Text quizExplanationText; 

    //Backpack UI
    [Header("Backpack UI")]
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private List<Text> itemTexts; // Text บนปุ่ม Item
    [SerializeField] private List<GameObject> itemButtons; // ปุ่ม Item

    public void ShowQuizPanel(QuizQuestionPython question)
    {
        // ซ่อน UI อื่นๆ ที่ไม่เกี่ยวข้อง
        actionSelect.SetActive(false);
        attackSelect.SetActive(false);
        attackDescription.SetActive(false);
        dialogText.gameObject.SetActive(false); // ซ่อนกรอบข้อความหลัก

        // แสดง Quiz Panel
        quizPanel.SetActive(true);
        quizQuestionText.text = question.questionText;

        // 🔥 อัปเดต Text คำอธิบาย (ถ้ามี)
        if (quizExplanationText != null)
        {
            // เช็คก่อนว่ามีคำอธิบายไหม ถ้าไม่มีก็ซ่อนไป
            if (!string.IsNullOrEmpty(question.explanation))
            {
                quizExplanationText.text = question.explanation;
                quizExplanationText.gameObject.SetActive(true);
            }
            else
            {
                quizExplanationText.gameObject.SetActive(false);
            }
        }

        // ตั้งค่าข้อความบนปุ่มคำตอบ
        for (int i = 0; i < quizAnswerTexts.Count; i++)
        {
            if (i < question.answers.Count)
            {
                // ถ้ามีคำตอบใน List
                quizAnswerTexts[i].text = question.answers[i];
                quizAnswerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // ถ้าคำถามมีคำตอบน้อยกว่า 4 ให้ซ่อนปุ่มที่เหลือ
                quizAnswerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideQuizPanel()
    {
        quizPanel.SetActive(false);
        // เมื่อ Quiz จบ ค่อยให้ BattleManager สั่งเปิด dialogText อีกที
        dialogText.gameObject.SetActive(true);
    }

    public IEnumerator SetDialog(string message)
    {
        isWriting = true;
        dialogText.text = "";
        foreach (var character in message)
        {
            dialogText.text += character;
            yield return new WaitForSeconds(1 / characterPerSecond);
        }
        yield return new WaitForSeconds(0.5f);
        isWriting = false;
    }

    public void ToggleDialogText(bool activated)
    {
        dialogText.enabled = activated;
    }

    public void ToggleActions(bool activated)
    {
        actionSelect.SetActive(activated);
    }

    public void ToggleAttacks(bool activated)
    {
        attackSelect.SetActive(activated);
        attackDescription.SetActive(activated);

        if (activated)
        {
            primaryAttackbutton.Select();
        }
    }

    public void SetCreatureAtacks(List<Attack> attacks)
    {
        for (int i = 0; i < attackTexts.Count; i++)
        {
            if (i < attacks.Count)
            {
                attackTexts[i].text = attacks[i].Base.Name;
                attackbuttons[i].SetActive(true);
            }
            else
            {
                attackTexts[i].text = "----";
                attackbuttons[i].SetActive(false);
            }
        }
    }

    public void SetAttackDescription()
    {
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        if (selectedButton == null) return;

        // หาว่าปุ่มที่เลือกคือ index ที่เท่าไหร่ (0, 1, 2, หรือ 3)
        int selectedIndex = attackbuttons.IndexOf(selectedButton);

        // ตรวจสอบว่าปุ่มที่เลือกคือหนึ่งในปุ่มโจมตีของเรา
        if (selectedIndex != -1)
        {
            List<Attack> attacks = playerUnit.Creature.Attacks;
            if (selectedIndex < attacks.Count)
            {
                ppText.text = $"{attacks[selectedIndex].Pp} / {attacks[selectedIndex].Base.PP}";
                // --- (START) MODIFICATION ---
                // typeText.text = $"{attacks[selectedIndex].Base.Type}"; // 3. ลบบรรทัดเก่า
                
                // 4. ตั้งค่าไอคอนธาตุของท่าโจมตี
                if (attackTypeIcon != null && TypeIconManager.Instance != null)
                {
                    attackTypeIcon.sprite = TypeIconManager.Instance.GetIconForType(attacks[selectedIndex].Base.Type);
                    attackTypeIcon.gameObject.SetActive(attackTypeIcon.sprite != null);
                }
                // --- (END) MODIFICATION ---
            }
        }
    }
    public void ToggleBackpack(bool activated)
    {
        backpackPanel.SetActive(activated);
    }

    /// ตั้งค่าข้อความบนปุ่ม Item (เหมือน SetCreatureAttacks)
    public void SetBackpackItems(List<BattleManager.ItemSlot> playerInventory)
    {
        for (int i = 0; i < itemTexts.Count; i++)
        {
            if (i < playerInventory.Count)
            {
                var slot = playerInventory[i];
                itemTexts[i].text = $"{slot.item.itemName} x{slot.quantity}";
                itemButtons[i].SetActive(true);
            }
            else
            {
                itemTexts[i].text = "---";
                itemButtons[i].SetActive(false);
            }
        }
    }
}