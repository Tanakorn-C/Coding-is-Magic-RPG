using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 
using System.Linq;

public class Game_Manager : MonoBehaviour
{
    private QuizQuestionPython currentQuestion;
    private List<QuizQuestionPython> remainingQuestions; 
    private EncounterData currentEncounter;

    public Unit PlayerUnit;
    private Unit EnemyUnit;
    
    [Header("Dynamic Setup")]
    public Transform enemySpawnPoint; 
    
    [Header("Enemy UI References")]
    public Slider EnemyHpSlider;
    public TextMeshProUGUI EnemyHpText;
    public TextMeshProUGUI EnemyNameText; 
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI timer;

    [Header("Timer Settings")]
    public float timePerQuestion = 10f;
    private float currentTimer;
    private Coroutine timerCoroutine;

    public Button[] answerButtons;

    [Header("Sound")]
    private AudioSource sfxAudioSource;
    public AudioClip correctSound, wrongSound, winSound, loseSound, clickSound;

    private enum GameState { PlayerTurn, EnemyTurn, Busy, Won, Lost }
    private GameState state;
    private bool isWaitingForSpace = false;
    private bool spaceBarWasPressed = false;

    void Awake()
    {
        sfxAudioSource = GetComponent<AudioSource>();
        if (sfxAudioSource == null) { Debug.LogError("!!! ERROR: Game_Manager ไม่มี AudioSource!"); }
    }

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        string encounterIDToLoad = "Enemy_Easy"; 
        if (GameDataPersistence.Instance != null && !string.IsNullOrEmpty(GameDataPersistence.Instance.currentEncounterID))
        {
            encounterIDToLoad = GameDataPersistence.Instance.currentEncounterID;
        } 
        else
        {
            Debug.LogWarning($"ไม่พบ GameData! กำลังโหลด Encounter เริ่มต้น: {encounterIDToLoad} (Test Mode)");
        }

        currentEncounter = Resources.Load<EncounterData>($"Encounters/{encounterIDToLoad}");

        if (currentEncounter == null) 
        {
            Debug.LogError($"!!! FATAL ERROR: หา Encounter Data ไม่เจอที่ 'Resources/Encounters/{encounterIDToLoad}'");
            return;
        }

        if (currentEncounter.enemyPrefab != null && enemySpawnPoint != null)
        {
            GameObject enemyObj = Instantiate(currentEncounter.enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
            EnemyUnit = enemyObj.GetComponent<Unit>();
            if (EnemyUnit != null)
            {
                EnemyUnit.unitName = currentEncounter.enemyName;
                EnemyUnit.hpSlider = this.EnemyHpSlider;
                EnemyUnit.hpText = this.EnemyHpText;
                EnemyUnit.nameText = this.EnemyNameText;
            }
        }
        else { /* ... (Error Log) ... */ }

        if (PlayerUnit == null || EnemyUnit == null) { /* ... (Error Log) ... */ }
        
        PlayerUnit.Setup();
        EnemyUnit.Setup();

        List<QuizQuestionPython> allQuestions = QuizDataHandler.LoadCustomQuestions();

        // 1. กรองขั้นต้น (Category & Difficulty)
        List<QuizQuestionPython> encounterPool = allQuestions
            .Where(q => q.category == currentEncounter.categoryToAsk &&
                        q.difficulty == currentEncounter.difficultyToAsk)
            .ToList();

        if (encounterPool.Count == 0)
        {
            Debug.LogError($"!!! ERROR: 'QuizDataHandler' did not find any questions for Category: {currentEncounter.categoryToAsk} AND Difficulty: {currentEncounter.difficultyToAsk}!");
            infoText.text = "Error: No matching questions found!";
            return;
        }

        // 2. ดึง List คำถามที่เคยตอบถูกแล้ว (จาก GameData)
        List<string> answeredQuestions = new List<string>();
        if (GameDataPersistence.Instance != null)
        {
            answeredQuestions = GameDataPersistence.Instance.correctlyAnsweredQuestions;
        }

        // 3. กรองครั้งที่สอง (เอาที่ตอบถูกแล้วออก)
        remainingQuestions = encounterPool
            .Where(q => !answeredQuestions.Contains(q.questionText)) // <-- (ใช้ questionText เป็น ID)
            .ToList();
        
        Debug.Log($"โหลดคำถาม: พบ {encounterPool.Count} ข้อ, กรองที่ตอบถูกแล้ว {answeredQuestions.Count} ข้อ, เหลือ {remainingQuestions.Count} ข้อ");

        StartCoroutine(SetupFirstTurn());
    }

    void ShowNewQuestion()
    {
        if (remainingQuestions.Count == 0)
        {

            List<QuizQuestionPython> allQuestions = QuizDataHandler.LoadCustomQuestions();

            List<QuizQuestionPython> encounterPool = allQuestions
                .Where(q => q.category == currentEncounter.categoryToAsk &&
                            q.difficulty == currentEncounter.difficultyToAsk)
                .ToList();

            // ดึง List ที่ตอบถูกแล้ว
            List<string> answeredQuestions = new List<string>();
            if (GameDataPersistence.Instance != null)
            {
                answeredQuestions = GameDataPersistence.Instance.correctlyAnsweredQuestions;
            }

            // กรองที่ตอบถูกแล้วออก
            remainingQuestions = encounterPool
                .Where(q => !answeredQuestions.Contains(q.questionText))
                .ToList();
            
        }
        
        int randomIndex = Random.Range(0, remainingQuestions.Count);
        currentQuestion = remainingQuestions[randomIndex];
        remainingQuestions.RemoveAt(randomIndex); 
        infoText.text = currentQuestion.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[i];
            answerButtons[i].interactable = true; 
        }
    }

    IEnumerator SetupFirstTurn()
    {
        float countdownDuration = 3f; 
        float timer = countdownDuration;
        string originalMessage = "การต่อสู้เริ่มขึ้น!";
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            int timeRemaining = Mathf.CeilToInt(timer);
            if (timeRemaining < 0) { timeRemaining = 0; }
            infoText.text = $"{originalMessage}... ( {timeRemaining} )";
            yield return null; 
        }
        StartPlayerTurn();
        yield break; 
    }

    void StartPlayerTurn()
    {
        state = GameState.PlayerTurn;
        ShowNewQuestion();

        currentTimer = timePerQuestion;
        timer.text = Mathf.CeilToInt(currentTimer).ToString();
        timer.gameObject.SetActive(true);

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        timerCoroutine = StartCoroutine(RunTimer());
    }

    public void OnAnswerSelected(int selectedIndex)
    {
        if (state != GameState.PlayerTurn) return;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        timer.gameObject.SetActive(false);

        if (sfxAudioSource != null && clickSound != null) 
            sfxAudioSource.PlayOneShot(clickSound);

        DisableButtons();
        state = GameState.Busy;

        if (selectedIndex == currentQuestion.correctAnswerIndex)
        {
            StartCoroutine(PlayerAttack());
        }
        else
        {
            StartCoroutine(PlayerGotHit());
        }
    }

    IEnumerator RunTimer()
    {
        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            timer.text = Mathf.CeilToInt(currentTimer).ToString();
            yield return null;
        }

        timer.text = "0";
        timerCoroutine = null;
        state = GameState.Busy;

        infoText.text = "เวลาหมด!"; 

        DisableButtons();
        
        StartCoroutine(PlayerGotHit());
    }

    IEnumerator PlayerAttack()
    {
        if (sfxAudioSource != null && correctSound != null)
            sfxAudioSource.PlayOneShot(correctSound);

        if (GameDataPersistence.Instance != null && !GameDataPersistence.Instance.correctlyAnsweredQuestions.Contains(currentQuestion.questionText))
        {
            GameDataPersistence.Instance.correctlyAnsweredQuestions.Add(currentQuestion.questionText);
            Debug.Log($"บันทึกคำถามที่ตอบถูก: {currentQuestion.questionText}");
        }

        infoText.text = "ยอดเยี่ยม! ตอบถูก!";
        yield return new WaitForSeconds(1.5f);

        PlayerUnit.PlayAttackAnimation();
        infoText.text = "โจมตี Slingshot Paw Paw!";
        yield return new WaitForSeconds(1f);

        bool isEnemyDead = EnemyUnit.TakeDamage(PlayerUnit.damageAmount);
        yield return new WaitForSeconds(1);
        if (isEnemyDead)
        {
            state = GameState.Won;
            StartCoroutine(EndGame(true));
        }
        else
        {
            StartCoroutine(WaitAndNextQuestion());
        }
    }

    void Update()
    {
        if (isWaitingForSpace && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            spaceBarWasPressed = true;
        }
    }

    private IEnumerator WaitForSpaceBar()
    {
        isWaitingForSpace = true;
        spaceBarWasPressed = false;
        yield return new WaitUntil(() => spaceBarWasPressed == true);
        isWaitingForSpace = false;
    }

    IEnumerator PlayerGotHit()
    {
        if (!infoText.text.Contains("เวลาหมด"))
        {
            if (sfxAudioSource != null && wrongSound != null)
                sfxAudioSource.PlayOneShot(wrongSound);
                
            infoText.text = "ตอบผิด!";
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            infoText.text = $"ช้าไป คาวบอย!";
            yield return new WaitForSeconds(1.5f);
        }
        EnemyUnit.PlayAttackAnimation(); 
        infoText.text = "โดนโจมตี";
        yield return new WaitForSeconds(1f);

        bool isPlayerDead = PlayerUnit.TakeDamage(EnemyUnit.damageAmount);
        yield return new WaitForSeconds(1f); 
        if (isPlayerDead)
        {
            state = GameState.Lost;
            StartCoroutine(EndGame(false));
        }
        else
        {
            StartCoroutine(WaitAndNextQuestion());
        }
    }

    IEnumerator WaitAndNextQuestion()
    {
        timer.gameObject.SetActive(false);

        infoText.text = "คำถามข้อต่อไป...(Press spacebar)";
        yield return StartCoroutine(WaitForSpaceBar()); 
        StartPlayerTurn();
    }

    IEnumerator EndGame(bool didWin)
    {
        float fadeDuration = 2f;
        if (didWin)
        {
            infoText.text = "คุณชนะแล้ว!";
            if (sfxAudioSource != null && winSound != null)
                sfxAudioSource.PlayOneShot(winSound);
            
            if (GameDataPersistence.Instance != null)
            {
                GameDataPersistence.Instance.justWonBattle = true;
                if (!string.IsNullOrEmpty(GameDataPersistence.Instance.currentEncounterID))
                {
                    GameDataPersistence.Instance.defeatedEnemies.Add(GameDataPersistence.Instance.currentEncounterID);
                    GameDataPersistence.Instance.currentEncounterID = null;
                }
            }
            
            yield return StartCoroutine(EnemyUnit.FadeOut(fadeDuration)); 
            yield return new WaitForSeconds(1f);

            if (GameDataPersistence.Instance != null)
            {
            
                string specialScene = GameDataPersistence.Instance.sceneToLoadOnWin;
                string returnScene = GameDataPersistence.Instance.sceneToReturnTo;

                if (!string.IsNullOrEmpty(specialScene))
                {
                    // Check boss 
                    Debug.Log($"Final Encounter Won! Loading scene: {specialScene}");
                    GameDataPersistence.Instance.sceneToLoadOnWin = null;
                    SceneManager.LoadScene(specialScene);
                }
                else if (!string.IsNullOrEmpty(returnScene))
                {
                    // Check enemy
                    SceneManager.LoadScene(returnScene);
                }
                else
                {
                    //
                    Debug.LogWarning("ไม่พบ GameData! กำลังโหลดฉากต่อสู้ใหม่ (Test Mode)");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                //
                Debug.LogWarning("ไม่พบ GameData! กำลังโหลดฉากต่อสู้ใหม่ (Test Mode)");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else
        {
            infoText.text = "คุณแพ้แล้ว...";
            if (sfxAudioSource != null && loseSound != null)
                sfxAudioSource.PlayOneShot(loseSound);

            if (GameDataPersistence.Instance != null)
            {
                GameDataPersistence.Instance.justWonBattle = false; 
                GameDataPersistence.Instance.currentEncounterID = null; 
            }

            yield return StartCoroutine(PlayerUnit.FadeOut(fadeDuration));
            yield return new WaitForSeconds(1f);

            if (GameDataPersistence.Instance != null && !string.IsNullOrEmpty(GameDataPersistence.Instance.sceneToReturnTo))
            {
                SceneManager.LoadScene(GameDataPersistence.Instance.sceneToReturnTo);
            }
            else
            {
                Debug.LogWarning("ไม่พบ GameData! กำลังโหลดฉากต่อสู้ใหม่ (Test Mode)");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    void DisableButtons()
    {
        foreach (var button in answerButtons)
        {
            button.interactable = false;
        }
    }
}