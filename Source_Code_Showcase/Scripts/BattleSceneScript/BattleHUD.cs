using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class BattleHUD : MonoBehaviour
{
    public Text creatureName;
    public Text creatureLevel;
    public HealthBar healthBar;
    public Text creatureHealth;
    private Creature _creature;

    // --- (START) MODIFICATION ---
    [Header("Type Icon")]
    [SerializeField] private Image typeIcon; // ลาก Image UI ที่จะใช้แสดงไอคอนธาตุมาใส่
    // --- (END) MODIFICATION ---
    public void SetCreatureData(Creature creature)
    {
        _creature = creature;
        creatureName.text = creature.Base.CreatureName;
        creatureLevel.text = $"Lv: {creature.Level}";
        healthBar.SetHP((float)creature.HP / creature.MaxHP);

        // --- (START) MODIFICATION ---
        // ตั้งค่าไอคอนธาตุ
        if (typeIcon != null && TypeIconManager.Instance != null)
        {
            typeIcon.sprite = TypeIconManager.Instance.GetIconForType(creature.Base.Type);
            
            // ซ่อน/แสดง ไอคอน ถ้ามี Sprite
            typeIcon.gameObject.SetActive(typeIcon.sprite != null); 
        }
        // --- (END) MODIFICATION ---
        
        UpdateCreatureData(creature.HP);
    }

    public void UpdateCreatureData(int oldHPValue)
    {
        StartCoroutine(healthBar.SetSmoothHP((float) _creature.HP/ _creature.MaxHP));
        StartCoroutine(DecreaseHealthPoints(oldHPValue));
    }

    IEnumerator DecreaseHealthPoints(int oldHPValue)
    {
        while (oldHPValue > _creature.HP)
        {
            oldHPValue--;
            creatureHealth.text = $"{oldHPValue}/{_creature.MaxHP}";
            yield return new WaitForSeconds(0.04f);
        }
        creatureHealth.text = $"{_creature.HP}/{_creature.MaxHP}";
    }
}