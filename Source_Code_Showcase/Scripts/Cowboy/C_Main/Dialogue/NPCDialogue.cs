using UnityEngine;
using UnityEngine.UI;

public class NPCDialogue : MonoBehaviour
{
    [TextArea(3, 5)] public string[] dialogueLines;
    private bool isPlayerNearby;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Instance.StartDialogue(dialogueLines);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("กด E เพื่อคุย");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
