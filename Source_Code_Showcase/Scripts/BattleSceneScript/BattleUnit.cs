using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class BattleUnit : MonoBehaviour
{
    public CreatureBase _base;
    public int _level;
    public bool isPlayer;
    public Creature Creature { get; set; }

    public QuestionCategory AssociatedCategory => Creature.Base.AssociatedCategory;
    public QuestionDifficulty QuestionDifficulty => Creature.Base.AssociatedDifficulty;

    private Vector3 initialPosition;
    private Color initialColor;
    [SerializeField] float startTimeAnimation, Attack, attackTimeAnimation, dieTimeAnimation, hitTimeAnimation;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;

    // 🔥 เพิ่มตัวนี้: สไลด์ปรับความดัง (ค่าเริ่มต้น 0.5 หรือ 50%)
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.5f;

    private Image creatureImage;
    private Animator animator;
    private AudioSource audioSource;
    private Tween idleTween;

    public void SetCreatureBase(CreatureBase newBase)
    {
        _base = newBase;
    }

    private void Awake()
    {
        creatureImage = GetComponent<Image>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        initialPosition = creatureImage.transform.localPosition;
        initialColor = creatureImage.color;
    }

    public void SetupCreature()
    {
        Creature = new Creature(_base, _level);

        if (isPlayer)
        {
            creatureImage.sprite = Creature.Base.BackSprite;
        }
        else
        {
            creatureImage.sprite = Creature.Base.FrontSprite;
        }

        playerStartAnimation();
    }

    public void LoadPersistentCreature(Creature creatureToLoad)
    {
        Creature = creatureToLoad;

        if (isPlayer)
        {
            creatureImage.sprite = Creature.Base.BackSprite;
        }
        else
        {
            creatureImage.sprite = Creature.Base.FrontSprite;
        }

        playerStartAnimation();
    }

    public void PlayFireballAnimation()
    {
        if (animator != null)
            animator.SetTrigger("DoFireBall");
    }

    public void playerStartAnimation()
    {
        creatureImage.transform.localScale = Vector3.one;
        creatureImage.transform.DOKill();

        CanvasGroup cg = creatureImage.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
        }

        float startX;
        if (isPlayer)
        {
            startX = initialPosition.x - 400;
        }
        else
        {
            startX = initialPosition.x + 400;
        }

        creatureImage.transform.localPosition = new Vector3(startX, initialPosition.y - 100);

        Sequence mySequence = DOTween.Sequence().SetLink(gameObject);

        mySequence.Append(
            creatureImage.transform.DOLocalMove(initialPosition, startTimeAnimation)
                .SetEase(Ease.OutCubic)
        );

        if (cg != null)
        {
            mySequence.Insert(0,
                cg.DOFade(1, startTimeAnimation * 0.75f)
            );
        }

        mySequence.Append(
            creatureImage.transform.DOPunchScale(
                new Vector3(0.1f, 0.1f, 0.1f),
                0.3f,
                10,
                1
            )
        );

        mySequence.OnComplete(() => PlayIdleAnimation());
        mySequence.Play();
    }

    public void PlayIdleAnimation()
    {
        if (idleTween != null) idleTween.Kill();
        creatureImage.transform.localScale = Vector3.one;

        idleTween = creatureImage.transform.DOScale(new Vector3(1.03f, 1.03f, 1f), 1.0f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    public void playAttackAnimation()
    {
        // 🔥 แก้ไข: ใส่ sfxVolume เข้าไปเป็นตัวคูณความดัง
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, sfxVolume);
        }

        var seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(creatureImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? 1 : -1) * 60, 0.2f));
        seq.Append(creatureImage.transform.DOLocalMoveX(initialPosition.x, 0.3f));
    }

    public void playReceiveAttackAnimation()
    {
        // 🔥 แก้ไข: ใส่ sfxVolume เข้าไปเป็นตัวคูณความดัง
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, sfxVolume);
        }

        creatureImage.transform.DOShakePosition(1, 20, 10, 90, false, true).SetLink(gameObject);

        var seq = DOTween.Sequence().SetLink(gameObject);

        for (int i = 0; i < 3; i++)
        {
            seq.Append(creatureImage.DOColor(Color.grey, hitTimeAnimation));
            seq.Append(creatureImage.DOColor(initialColor, hitTimeAnimation));
        }
    }

    public void playDeathAnimation()
    {
        if (idleTween != null) idleTween.Kill();
        creatureImage.transform.DOKill();

        var seq = DOTween.Sequence().SetLink(gameObject);

        seq.Join(creatureImage.DOFade(0, dieTimeAnimation));
        seq.Join(creatureImage.transform.DOShakePosition(1, 20, 10, 90, false, true));
    }
}