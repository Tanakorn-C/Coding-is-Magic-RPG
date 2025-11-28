using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox battleDialogBox;
    [Header("Inventory")]
    [SerializeField] private List<ItemSlot> playerInventory;
    public Button primaryAttackbutton;

    [Header("Quiz Timer System")]
    [SerializeField] private Text quizTimerText;
    [SerializeField] private float quizTimeLimit = 60f;
    [Header("Quiz System")]
    [SerializeField] private List<QuizQuestionPython> pythonQuizDatabase;

    [Header("Audio Settings")] // 🔥 เพิ่มส่วนนี้
    [SerializeField] private AudioSource audioSource;       // ตัวเล่นเสียง Effect (SFX)
    [SerializeField] private AudioSource battleMusicSource; // ตัวเล่นเพลง BGM (ลากตัวที่เปิดเพลง Battle มาใส่)
    [SerializeField] private AudioClip winSound;

    [Range(0f, 1f)][SerializeField] private float winVolume = 1.0f; // ความดังเสียงชนะ (0-1)
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.5f; // ความดังเพลง (ถ้าอยากคุม)

    // คลังคำถามที่จะใช้ในฉากนี้ (จะถูกกรองเอาข้อที่ตอบแล้วออก)
    private List<QuizQuestionPython> _availableQuestions;
    public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }
    private float quizTimeElapsed;
    private BattleState state;
    private bool isQuizActive = false;
    private Coroutine activeQuizTimer;

    private Attack selectedAttack;
    private QuizQuestionPython currentQuestion;
    [SerializeField] private CreatureBase _base;

    public void SetCreatureBase(CreatureBase newBase)
    {
        _base = newBase;
    }

    private void Start()
    {
        state = BattleState.Start; //

        if (quizTimerText != null)
            quizTimerText.gameObject.SetActive(false);

        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        Creature persistentPlayer = GameDataPersistenceMain.Instance.PlayerCreature;
        playerUnit.LoadPersistentCreature(persistentPlayer);

        playerHUD.SetCreatureData(playerUnit.Creature);
        battleDialogBox.SetCreatureAtacks(playerUnit.Creature.Attacks);

        // --------------------------------------------------------
        // 🔥 1. ปรับปรุงระบบโหลดคำถาม (กรองข้อที่ตอบแล้วออก)
        // --------------------------------------------------------

        // A. รวมคำถามทั้งหมด (Custom + Built-in) เป็น Master List ก่อน
        List<QuizQuestionPython> allPotentialQuestions = new List<QuizQuestionPython>(pythonQuizDatabase);
        List<QuizQuestionPython> customQuestions = QuizDataHandler.LoadCustomQuestions();
        allPotentialQuestions.AddRange(customQuestions);

        // B. เตรียม List สำหรับใช้งาน
        _availableQuestions = new List<QuizQuestionPython>();

        // C. กรองเอาเฉพาะข้อที่ "ยังไม่อยู่ใน List ที่ตอบถูก"
        if (GameDataPersistenceMain.Instance != null)
        {
            foreach (var q in allPotentialQuestions)
            {
                // ใช้ questionText เป็น ID ในการเช็ค
                if (!GameDataPersistenceMain.Instance.solvedQuestions.Contains(q.questionText))
                {
                    _availableQuestions.Add(q);
                }
            }
            Debug.Log($"โหลดคำถามทั้งหมด: {allPotentialQuestions.Count}, เหลือที่ยังไม่ตอบ: {_availableQuestions.Count}");
        }
        else
        {
            _availableQuestions = new List<QuizQuestionPython>(allPotentialQuestions);
        }

        // --------------------------------------------------------

        if (GameDataPersistenceMain.Instance != null && GameDataPersistenceMain.Instance.creatureToLoad != null)
        {
            enemyUnit.SetCreatureBase(GameDataPersistenceMain.Instance.creatureToLoad);
            enemyUnit._level = GameDataPersistenceMain.Instance.enemyLevelToLoad;
        }

        enemyUnit.SetupCreature();
        enemyHUD.SetCreatureData(enemyUnit.Creature);

        yield return StartCoroutine(battleDialogBox.SetDialog($"{enemyUnit.Creature.Base.name} โผล่มาแล้ว!"));
        yield return new WaitForSeconds(1.0f);
        if (playerUnit.Creature.Speed < enemyUnit.Creature.Speed)
        {
            StartCoroutine(EnemyAction());
        }
        else
        {
            PlayerAction();
        }
    }

    // ... (EnemyAction, PlayerAction, UseItem... คงเดิม ไม่ต้องแก้) ...

    IEnumerator EnemyAction()
    {
        state = BattleState.EnemyMove; //

        Attack attack = enemyUnit.Creature.RandomAttack();
        yield return ShowDialogAndWait($"{enemyUnit.Creature.Base.name} ใช้ท่า {attack.Base.Name}");
        yield return new WaitForSeconds(1.0f);

        var oldHPValue = playerUnit.Creature.HP;

        enemyUnit.playAttackAnimation();
        playerUnit.playReceiveAttackAnimation();

        var damageDesc = playerUnit.Creature.ReceiveDamage(enemyUnit.Creature, attack);

        playerHUD.UpdateCreatureData(oldHPValue);
        yield return ShowDamageDescription(damageDesc);

        if (damageDesc.Dead)
        {
            yield return ShowDialogAndWait($"คุณได้เสียท่าให้กับ {enemyUnit.Creature.Base.name}");
            playerUnit.playDeathAnimation();
            yield return new WaitForSeconds(1.5f);
            GameDataPersistenceMain.Instance.PlayerLostOrRan();
        }
        else
        {
            PlayerAction();
        }
        yield return new WaitForSeconds(1.0f);
    }

    public void PlayerAction()
    {
        state = BattleState.Start;
        StartCoroutine(PlayerActionCoroutine());
    }

    IEnumerator PlayerActionCoroutine()
    {
        // 1. ✅ เพิ่มบรรทัดนี้: สั่งซ่อนปุ่มก่อนเสมอ เพื่อความชัวร์
        battleDialogBox.ToggleDialogText(true);

        // 2. รอจนกว่าข้อความ "กรุณาเลือกท่า" จะพิมพ์เสร็จ
        yield return StartCoroutine(ShowDialogAndWait("กรุณาเลือกท่า"));

        // 3. เมื่อพิมพ์เสร็จแล้ว ค่อยเปิดปุ่มให้กด
        battleDialogBox.ToggleActions(true);

        state = BattleState.PlayerAction;
    }

    public void PlayerAttack()
    {
        if (state != BattleState.PlayerAction) return;

        StartCoroutine(PlayerAttackCoroutine());
    }

    IEnumerator PlayerAttackCoroutine()
    {
        state = BattleState.Busy; // ✅ ล็อค

        battleDialogBox.ToggleDialogText(false);
        battleDialogBox.ToggleActions(false);
        yield return new WaitForSeconds(0.2f);
        battleDialogBox.ToggleAttacks(true);
    }

    public void attackButtonPressed(int selectedAttackIndex)
    {
        StartCoroutine(AttackQuizSequence(selectedAttackIndex));
    }


    public void OnRunButtonSelected()
    {
        // ไม่ให้หนีระหว่าง Quiz
        if (state != BattleState.PlayerAction) return;
        if (isQuizActive) return;

        // ซ่อนเมนู
        state = BattleState.Busy; // ✅ ล็อค
        battleDialogBox.ToggleActions(false);

        // เริ่ม Coroutine การหนี
        StartCoroutine(PlayerRun());
    }


    IEnumerator PlayerRun()
    {
        yield return StartCoroutine(ShowDialogAndWait("คุณวิ่งหนีอย่างรวดเร็ว!"));
        yield return new WaitForSeconds(1.5f);



        GameDataPersistenceMain.Instance.PlayerLostOrRan();

        // โหลด MainScene
        //SceneManager.LoadScene("MainScene");
    }

    public void OnBackpackButtonSelected()
    {
        if (state != BattleState.PlayerAction) return;

        state = BattleState.Busy; // ✅ ล็อค
        battleDialogBox.ToggleActions(false);
        battleDialogBox.SetBackpackItems(playerInventory);
        battleDialogBox.ToggleBackpack(true);
    }

    public void OnBackpackItemSelected(int itemIndex)
    {
        if (battleDialogBox.isWriting) return;
        StartCoroutine(UseItem(itemIndex));
    }

    IEnumerator UseItem(int itemIndex)
    {
        state = BattleState.Busy; // ✅ ล็อค
        battleDialogBox.ToggleBackpack(false);

        battleDialogBox.ToggleDialogText(true);
        var slot = playerInventory[itemIndex];

        if (playerUnit.Creature.HP == playerUnit.Creature.Base.MaxHP)
        {
            yield return StartCoroutine(ShowDialogAndWait("HP เต็มอยู่แล้ว!"));
            PlayerAction();
            yield break;
        }

        if (slot.item.effectType == ItemEffect.Heal)
        {
            int oldHP = playerUnit.Creature.HP;
            playerUnit.Creature.Heal(slot.item.effectAmount);
            slot.quantity--;
            yield return StartCoroutine(ShowDialogAndWait($"คุณใช้ {slot.item.itemName}!"));
            yield return new WaitForSeconds(1f);
            playerHUD.UpdateCreatureData(oldHP);
            yield return StartCoroutine(ShowDialogAndWait($"ฟื้นฟู HP {slot.item.effectAmount} หน่วย"));
        }
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(EnemyAction());
    }

    public IEnumerator AttackQuizSequence(int selectedAttackIndex)
    {
        if (isQuizActive) yield break;

        selectedAttack = playerUnit.Creature.Attacks[selectedAttackIndex];

        if (selectedAttack.Pp <= 0)
        {
            StartCoroutine(ShowDialogAndWait("PP ของท่านี้หมดแล้ว!"));
            yield break;
        }

        QuestionCategory categoryToAsk = enemyUnit.AssociatedCategory;
        currentQuestion = GetRandomQuestion(categoryToAsk);

        if (currentQuestion == null)
        {
            Debug.LogError("เกิดข้อผิดพลาด: หาคำถามไม่ได้เลย (แม้จะรีเซ็ตแล้ว)");
            // กรณีฉุกเฉินจริงๆ ให้ข้าม Quiz ไปโจมตีเลย
            StartCoroutine(PerformPlayerAttack(0f));
            yield break;
        }

        quizTimeElapsed = 0f;
        battleDialogBox.ToggleAttacks(false);
        battleDialogBox.ToggleDialogText(true);

        yield return StartCoroutine(ShowDialogAndWait("จงกล่าวคำร่าย..."));
        yield return new WaitForSeconds(1.0f);

        battleDialogBox.ShowQuizPanel(currentQuestion);

        isQuizActive = true;
        activeQuizTimer = StartCoroutine(StartQuizTimer());
    }

    IEnumerator StartQuizTimer()
    {
        quizTimerText.gameObject.SetActive(true);
        quizTimeElapsed = 0f;

        while (quizTimeElapsed < quizTimeLimit)
        {
            quizTimeElapsed += Time.deltaTime;
            float timeLeft = quizTimeLimit - quizTimeElapsed;
            quizTimerText.text = $"Time: {timeLeft:F0}";

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                quizTimerText.text = $"Time: 0";
                break;
            }
            yield return null;
        }

        if (isQuizActive)
        {
            isQuizActive = false;
            quizTimerText.gameObject.SetActive(false);
            battleDialogBox.HideQuizPanel();

            yield return StartCoroutine(ShowDialogAndWait("เวลาหมด! ช้าเกินไป..."));
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PerformPlayerAttack(0f));
        }
    }

    public void OnQuizAnswerSelected(int selectedAnswerIndex)
    {
        if (!isQuizActive) return;

        isQuizActive = false;
        StopCoroutine(activeQuizTimer);
        quizTimerText.gameObject.SetActive(false);
        battleDialogBox.HideQuizPanel();

        bool isCorrect = (selectedAnswerIndex == currentQuestion.correctAnswerIndex);
        float bonus = 0f;

        if (isCorrect)
        {
            // 🔥 2. ตอบถูก! บันทึกลง GameData เพื่อไม่ให้ถามซ้ำ
            if (GameDataPersistenceMain.Instance != null)
            {
                // เช็คกันเหนียวว่ายังไม่มีใน List
                if (!GameDataPersistenceMain.Instance.solvedQuestions.Contains(currentQuestion.questionText))
                {
                    GameDataPersistenceMain.Instance.solvedQuestions.Add(currentQuestion.questionText);
                }
            }

            if (quizTimeElapsed <= 20f)
            {
                bonus = 0.20f;
                StartCoroutine(HandleQuizResult("สุดยอด! Critical Hit!", bonus));
            }
            else if (quizTimeElapsed <= 40f)
            {
                bonus = 0.10f;
                StartCoroutine(HandleQuizResult("ยอดเยี่ยม! Power Up!", bonus));
            }
            else
            {
                bonus = 0f;
                StartCoroutine(HandleQuizResult("ถูกต้อง... แต่ช้าไป (ไม่ได้รับโบนัส)", bonus));
            }
        }
        else
        {
            // ตอบผิด: ไม่บันทึก (ให้โอกาสเจอข้อเดิมใหม่)
            bonus = -0.60f;
            StartCoroutine(HandleQuizResult("โดนแค่เฉี่ยวๆ Glancing Blow!", bonus));
        }
    }

    IEnumerator HandleQuizResult(string message, float bonus)
    {
        yield return StartCoroutine(ShowDialogAndWait(message));
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(PerformPlayerAttack(bonus));
    }

    IEnumerator PerformPlayerAttack(float damageBonus)
    {
        state = BattleState.PlayerMove; //

        selectedAttack.Pp--;
        yield return StartCoroutine(ShowDialogAndWait($"คุณใช้ท่า {selectedAttack.Base.Name}"));

        playerUnit.playAttackAnimation();
        enemyUnit.playReceiveAttackAnimation();

        if (selectedAttack == playerUnit.Creature.Attacks[0])
        {
            playerUnit.PlayFireballAnimation();
        }

        var oldHPValue = enemyUnit.Creature.HP;
        var damageDesc = enemyUnit.Creature.ReceiveDamage(playerUnit.Creature, selectedAttack, damageBonus);

        enemyHUD.UpdateCreatureData(oldHPValue);
        yield return ShowDamageDescription(damageDesc);

        if (damageDesc.Dead)
        {
            // 1. หยุดเพลง Battle (ถ้ามี)
            if (battleMusicSource != null)
                battleMusicSource.Stop();

            // 2. เล่นเสียงชนะ!
            if (audioSource != null && winSound != null)
                audioSource.PlayOneShot(winSound);

            yield return ShowDialogAndWait($"คุณได้จัดการ {enemyUnit.Creature.Base.name}");
            enemyUnit.playDeathAnimation();

            // 3. รอสักพัก (ให้เสียงชนะเล่นไปสักหน่อย หรือรออนิเมชั่นจบ)
            yield return new WaitForSeconds(2.0f);

            GameDataPersistenceMain.Instance.PlayerWonBattle();
        }
        {
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(EnemyAction());
        }
    }

    // 🔥 3. ปรับปรุง Logic การสุ่ม (รองรับกรณีคำถามหมด)
    private QuizQuestionPython GetRandomQuestion(QuestionCategory category)
    {
        if (_availableQuestions == null || _availableQuestions.Count == 0)
        {
            // ถ้าคำถามใน "คลังที่โหลดมา" หมดเกลี้ยง (คือตอบถูกหมดทุกข้อในเกมแล้ว)
            // เราต้องยอม "รีไซเคิล" เอาคำถามทั้งหมดกลับมาใหม่ ไม่งั้นเกมค้าง
            Debug.LogWarning("คำถามหมดเกลี้ยง! ทำการ Reset คำถามชั่วคราวสำหรับ Battle นี้");
            RefillQuestionsFromMaster(category, true); // true = force refill even solved ones
        }

        // 1. กรองหาหมวดหมู่ที่ต้องการ
        List<QuizQuestionPython> matchingQuestions = _availableQuestions
            .Where(q => q.category == category)
            .ToList();

        // 2. ถ้าหมวดหมู่นี้ไม่มีเหลือแล้ว
        if (matchingQuestions.Count == 0)
        {
            Debug.LogWarning($"หมวดหมู่ {category} ถูกใช้หมดแล้ว (หรือตอบถูกหมดแล้ว)");

            // พยายามเติมเฉพาะหมวดหมู่นี้ จาก Master DB
            // แต่ต้องเช็คด้วยว่า ใน Master มีข้อที่ยังไม่ตอบเหลือไหม

            // (แบบง่าย) ดึงจาก Master มาเติมเลย ยอมให้ซ้ำได้ถ้าจำเป็น
            RefillQuestionsFromMaster(category, true);

            // กรองใหม่
            matchingQuestions = _availableQuestions
                .Where(q => q.category == category)
                .ToList();

            // ถ้ายังไม่มีอีก แสดงว่า Database หลักไม่มีหมวดหมู่นี้เลย
            if (matchingQuestions.Count == 0)
            {
                // สุ่มมั่วๆ มา 1 ข้อเพื่อกันเกมค้าง
                if (_availableQuestions.Count > 0)
                    return _availableQuestions[Random.Range(0, _availableQuestions.Count)];
                else
                    return null; // หมดหนทาง
            }
        }

        // 3. สุ่มและส่งกลับ
        int index = Random.Range(0, matchingQuestions.Count);
        QuizQuestionPython chosenQuestion = matchingQuestions[index];

        // ลบออกจาก available (เพื่อไม่ให้ซ้ำใน Battle รอบนี้)
        _availableQuestions.Remove(chosenQuestion);

        return chosenQuestion;
    }

    // ฟังก์ชันช่วยเติมคำถาม
    private void RefillQuestionsFromMaster(QuestionCategory category, bool forceIncludeSolved)
    {
        var masterList = new List<QuizQuestionPython>(pythonQuizDatabase);
        masterList.AddRange(QuizDataHandler.LoadCustomQuestions());

        var categoryQuestions = masterList.Where(q => q.category == category);

        foreach (var q in categoryQuestions)
        {
            // ถ้าบังคับเติม (force) หรือ ยังไม่ได้ตอบ -> ให้เพิ่มเข้า list
            if (forceIncludeSolved || (GameDataPersistenceMain.Instance != null && !GameDataPersistenceMain.Instance.solvedQuestions.Contains(q.questionText)))
            {
                // เช็คว่ามีอยู่แล้วหรือยังก่อนเพิ่ม
                if (!_availableQuestions.Any(existing => existing.questionText == q.questionText))
                {
                    _availableQuestions.Add(q);
                }
            }
        }
    }

    // ... (ส่วนอื่นๆ ShowDamageDescription, ItemSlot, ShowDialogAndWait คงเดิม) ...
    IEnumerator ShowDamageDescription(DamageDescription desc)
    {
        if (desc.Critical > 1)
        {
            yield return ShowDialogAndWait("การโจมตีนี้ Critical");
            yield return new WaitForSeconds(1.5f);
        }
        if (desc.Type > 1)
        {
            yield return ShowDialogAndWait("การโจมตีรุนแรงถึงใจ");
            yield return new WaitForSeconds(1.5f);
        }
        if (desc.Type < 1)
        {
            yield return ShowDialogAndWait("การโจมตีไม่ได้ผลเท่าไหร่");
            yield return new WaitForSeconds(1.5f);
        }
    }

    [System.Serializable]
    public class ItemSlot
    {
        public ItemBase item;
        public int quantity;
    }

    public void OnReturnToActionSelect()
    {
        battleDialogBox.ToggleAttacks(false);
        battleDialogBox.ToggleBackpack(false);
        StartCoroutine(PlayerActionCoroutine());
    }

    public IEnumerator ShowDialogAndWait(string message)
    {
        while (battleDialogBox.isWriting)
        {
            yield return null;
        }
        yield return StartCoroutine(battleDialogBox.SetDialog(message));
    }
}