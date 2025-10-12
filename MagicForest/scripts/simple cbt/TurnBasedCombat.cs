using System.Collections;
using System.Collections.Generic; // You'll need this for Lists
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// This class defines what a single quiz question will look like.
// [System.Serializable] makes it visible in the Unity Inspector.
[System.Serializable]
public class QuizQuestion
{
    public string question;
    public string[] answers = new string[4];
    public int correctAnswerIndex;
}

public class TurnBasedCombat : MonoBehaviour
{
    // --- Existing Variables ---
    [SerializeField] GameObject player;
    [SerializeField] GameObject enemy;
    [SerializeField] int playerHealth = 10;
    [SerializeField] int maxPlayerHealth = 10;
    [SerializeField] int enemyHealth = 10;
    [SerializeField] int maxEnemyHealth = 10;
    [SerializeField] TextMeshProUGUI playerHealthText;
    [SerializeField] TextMeshProUGUI enemyHealthText;

    // --- NEW: References for the Quiz UI ---
    [Header("Quiz UI Elements")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons = new Button[4]; // Drag your 4 answer buttons here

    // --- NEW: Quiz Logic Variables ---
    private List<QuizQuestion> questions;
    private QuizQuestion currentQuestion;
    private bool playerTurn = true;
    private Vector3 playerStartPosition;
    private Vector3 enemyStartPosition;

    void Start()
    {
        playerStartPosition = player.transform.position;
        enemyStartPosition = enemy.transform.position;

        // NEW: We call a function to create our questions right at the start.
        CreateQuestions();

        // NEW: Set up the click event for each of our four answer buttons.
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int buttonIndex = i; // A temporary variable to capture the index for the click event
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }

        UpdateUI();
        StartPlayerTurn(); // Start the first turn
    }
    
    // NEW: This function creates our 5 Python questions and adds them to the list.
    void CreateQuestions()
    {
        questions = new List<QuizQuestion>();

        questions.Add(new QuizQuestion {
            question = "Which command displays text on the screen in Python?",
            answers = new string[] { "print()", "display()", "show()", "output()" },
            correctAnswerIndex = 0
        });

        questions.Add(new QuizQuestion {
            question = "How do you write a single-line comment in Python?",
            answers = new string[] { "// comment", "# comment", "/* comment */", "" },
            correctAnswerIndex = 1
        });

        questions.Add(new QuizQuestion {
            question = "What will print(5 + 3) output?",
            answers = new string[] { "\"5 + 3\"", "53", "8", "Error" },
            correctAnswerIndex = 2
        });

        questions.Add(new QuizQuestion {
            question = "Which is the correct way to assign 10 to a variable 'score'?",
            answers = new string[] { "score = 10", "10 = score", "let score = 10", "var score = 10" },
            correctAnswerIndex = 0
        });

        questions.Add(new QuizQuestion {
            question = "What is text like \"Hello, World!\" called in programming?",
            answers = new string[] { "Text", "Integer", "String", "Boolean" },
            correctAnswerIndex = 2
        });
    }

    // NEW: Starts the player's turn by showing a new question.
    void StartPlayerTurn()
    {
        playerTurn = true;
        ShowNewQuestion();
    }

    // NEW: Chooses a random question and displays it on the UI.
    void ShowNewQuestion()
    {
        quizPanel.SetActive(true); // Show the quiz UI

        int randomIndex = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[randomIndex];

        questionText.text = currentQuestion.question;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[i];
        }
    }

    // NEW: This function is called when any of the four answer buttons is clicked.
    void OnAnswerSelected(int selectedIndex)
    {
        if (!playerTurn) return; // Ignore clicks if it's not the player's turn

        quizPanel.SetActive(false); // Hide the quiz UI

        // Check if the selected answer was correct
        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct! Attacking...");
            PlayerAttack(); // If correct, perform the attack
        }
        else
        {
            Debug.Log("Incorrect! Skipping turn.");
            EndPlayerTurn(); // If incorrect, skip the attack and end the turn
        }
    }

    // MODIFIED: This function now only contains the attack animation and damage logic.
    void PlayerAttack()
    {
        StartCoroutine(DoAttack(player, enemy, () =>
        {
            enemyHealth -= 2; // Damage the enemy
            if (enemyHealth <= 0)
            {
                enemyHealth = 0;
                Debug.Log("Enemy defeated!");
                // Here you could add logic to end the entire battle
            }
            EndPlayerTurn(); // End the player's turn after the attack
        }));
    }

    // NEW: A helper function to neatly end the player's turn.
    void EndPlayerTurn()
    {
        playerTurn = false;
        UpdateUI();
        Invoke(nameof(EnemyAttack), 1f); // Start the enemy's attack after a short delay
    }

    // MODIFIED: At the end of the enemy's turn, it starts the player's next turn.
    void EnemyAttack()
    {
        StartCoroutine(DoAttack(enemy, player, () =>
        {
            playerHealth -= 1;
            if (playerHealth <= 0)
            {
                playerHealth = 0;
                Debug.Log("Player defeated!");
                // Here you could add logic for a game over
            }
            StartPlayerTurn(); // It's the player's turn again
        }));
    }

    void UpdateUI()
    {
        playerHealthText.text = playerHealth + " / " + maxPlayerHealth;
        enemyHealthText.text = enemyHealth + " / " + maxEnemyHealth;
    }

    // --- The rest of your code (DoAttack, MoveOverTime) remains exactly the same ---
    IEnumerator DoAttack(GameObject attacker, GameObject target, Action onComplete)
    {
        Vector3 attackerStart = (attacker == player) ? playerStartPosition : enemyStartPosition;
        Vector3 targetStart = (target == player) ? playerStartPosition : enemyStartPosition;
        Vector3 attackPos = attackerStart + (targetStart - attackerStart).normalized * 0.5f;
        Vector3 hitPushPos = targetStart + (targetStart - attackerStart).normalized * 0.3f;
        yield return MoveOverTime(attacker, attackerStart, attackPos, 0.1f);
        yield return MoveOverTime(target, targetStart, hitPushPos, 0.05f);
        yield return MoveOverTime(target, hitPushPos, targetStart, 0.1f);
        yield return MoveOverTime(attacker, attackPos, attackerStart, 0.1f);
        onComplete?.Invoke();
    }

    IEnumerator MoveOverTime(GameObject obj, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = endPosition;
    }

    void Update() {}
}