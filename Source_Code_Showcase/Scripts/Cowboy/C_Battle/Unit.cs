using System.Collections;
using UnityEngine;
using UnityEngine.UI; 
using TMPro; 

public class Unit : MonoBehaviour
{
    public string unitName;
    public int maxHp;
    public int currentHp;
    public int damageAmount = 10; 

    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI nameText;

    [Header("Smooth Settings")]
    public float smoothDuration = 0.3f;
    public float textTickSpeed = 0.01f;

    // Audio
    private AudioSource sfxAudioSource; 
    public AudioClip attackSound;
    public AudioClip hitSound;

    private Animator animator;
    private SpriteRenderer spriteRenderer; 

    void Awake() 
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        sfxAudioSource = GetComponent<AudioSource>(); 

        if (animator == null) { Debug.LogError($"Animator not found on {gameObject.name}."); }
        if (spriteRenderer == null) { Debug.LogError($"SpriteRenderer not found on {gameObject.name}."); }
        if (sfxAudioSource == null) { Debug.LogError($"AudioSource not found on {gameObject.name}."); }
    }

    public void Setup()
    {
        currentHp = maxHp;
        
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }

        if (nameText != null)
        {
            nameText.text = unitName;
        }
    }

    public bool TakeDamage(int damage)
    {
        int oldHPValue = currentHp; 
        currentHp -= damage;
        if (currentHp < 0) { currentHp = 0; }

        StopAllCoroutines(); 

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; 
            StartCoroutine(FlashWhenHit(Color.red, 0.2f)); 
        }
        
        if (sfxAudioSource != null && hitSound != null)
        {
            sfxAudioSource.PlayOneShot(hitSound);
        }

        StartCoroutine(SetSmoothHP(currentHp)); 
        StartCoroutine(DecreaseHealthPoints(oldHPValue, currentHp)); 

        return currentHp <= 0;
    }

    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (sfxAudioSource != null && attackSound != null)
        {
            sfxAudioSource.PlayOneShot(attackSound);
        }
    }

    private IEnumerator FlashWhenHit(Color flashColor, float durationPerBlink)
    {
        int blinkCount = 3; 
        float halfBlink = durationPerBlink / 2f; 
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(halfBlink);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(halfBlink);
        }
        spriteRenderer.color = Color.white;
    }

    private IEnumerator SetSmoothHP(int targetHP)
    {
        float startHP = hpSlider.value; 
        float timer = 0f;
        while (timer < smoothDuration)
        {
            timer += Time.deltaTime;
            float newSliderValue = Mathf.Lerp(startHP, targetHP, timer / smoothDuration);
            hpSlider.value = newSliderValue;
            yield return null;
        }
        hpSlider.value = targetHP;
    }

    private IEnumerator DecreaseHealthPoints(int oldHPValue, int newHPValue)
    {
        int currentTextHP = oldHPValue;
        while (currentTextHP > newHPValue)
        {
            currentTextHP--;
            if (hpText != null)
            {
                hpText.text = $"{currentTextHP} / {maxHp}";
            }
            yield return new WaitForSeconds(textTickSpeed);
        }
        if (hpText != null)
        {
            hpText.text = $"{newHPValue} / {maxHp}";
        }
    }
    
    public IEnumerator FadeOut(float duration)
    {
        StopAllCoroutines();
        float timer = 0f;
        Color startColor = spriteRenderer.color; 
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); 
        while (timer < duration)
        {
            timer += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null; 
        }
        spriteRenderer.color = endColor;
    }
}