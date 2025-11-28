using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Input")]
    [Tooltip("หน่วงเวลาก่อนรับคลิก/Space หลังเปิดกล่อง (วินาที)")]
    public float advanceCooldown = 0.2f;

    [Header("Typing Effect")]
    [Tooltip("ความเร็วในการพิมพ์ (วินาทีต่อตัวอักษร)")]
    public float typingSpeed = 0.01f;

    private string[] lines;
    private int index;
    private bool isTalking;
    public bool IsTalking => isTalking; 
    private float nextAcceptTime;       
    private Coroutine typingCoroutine;

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (!dialoguePanel || !dialogueText)
        {
            Debug.LogError("[DialogueManager] UI references not assigned.");
            enabled = false; return;
        }
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        
        if (!isTalking) return;
        if (Time.time < nextAcceptTime) return; 

        bool isAdvancePressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                              (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        bool isSkipPressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);

        if (isAdvancePressed)
        {
            NextLine();
        }
        else if (isSkipPressed)
        {
            EndDialogue();
        }
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (Time.timeScale == 0f) return; 
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        lines = dialogueLines;
        index = 0;
        isTalking = true;

        dialoguePanel.SetActive(true);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(lines[index]));

        nextAcceptTime = Time.time + advanceCooldown;
    }

    void NextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            dialogueText.text = lines[index];
        }
        else
        {
            index++;
            if (index < lines.Length)
            {
                typingCoroutine = StartCoroutine(TypeLine(lines[index]));
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTalking = false;
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }
}