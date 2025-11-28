using System.Collections.Generic;
using UnityEngine;

// middle man class
public class Creature
{
    private CreatureBase _base;

    public CreatureBase Base
    {
        get => _base;
    }

    private int _level;

    public int Level
    {
        get => _level;
        set => _level = value;
    }
    private List<Attack> _attacks;

    public List<Attack> Attacks
    {
        get => _attacks;
        set => _attacks = value;
    }

    private int _hp;

    public int HP
    {
        get => _hp;
        set => _hp = value;
    }

    public Creature(CreatureBase creatureBase, int creaturelevel)
    {
        _base = creatureBase;
        _level = creaturelevel;

        _hp = MaxHP;

        // FIX 2: Use shorter list initialization syntax
        _attacks = new List<Attack>();

        foreach (var lAttack in _base.LearnableAttacks)
        {
            if (lAttack.Level <= _level)
            {

                // Only add the attack if the AttackBase asset is actually assigned.
                if (lAttack.Attack != null)
                {
                    _attacks.Add(new Attack(lAttack.Attack));
                }
            }
            // Logic to cap attacks at 4
            if (_attacks.Count >= 4)
            {
                break;
            }
        }
    }

    public int MaxHP => Mathf.FloorToInt((_base.MaxHP * _level) / 20.0f) + 10;
    public int Attack => Mathf.FloorToInt((_base.Attack * _level) / 100.0f) + 1;
    public int Defense => Mathf.FloorToInt((_base.Defense * _level) / 100.0f) + 1;
    public int SpAttack => Mathf.FloorToInt((_base.SpAttack * _level) / 100.0f) + 1;
    public int SpDefense => Mathf.FloorToInt((_base.SpDefense * _level) / 100.0f) + 1;
    public int Speed => Mathf.FloorToInt((_base.Speed * _level) / 100.0f) + 1;

    /// <param name="amount">จำนวน HP ที่จะฟื้นฟู</param>
    public void Heal(int amount)
    {
        HP += amount;

        if (HP > MaxHP) //check 
        {
            HP = MaxHP; // 
        }
    }

    public DamageDescription ReceiveDamage(Creature attacker, Attack attack, float damageBonus = 0f)
    {
        float critical = 1f;
        if (Random.Range(0, 100f) < 5f)
        {
            critical = 2f;
        }

        float type = TypeMatrix.GetMultEffectiveness(attack.Base.Type, this.Base.Type);

        var damageDesc = new DamageDescription()
        {
            Critical = critical,
            Type = type,
            Dead = false
        };

        float modifiers = Random.Range(0.84f, 1.0f) * type * critical;

        // --- ส่วนที่แก้ไข (เริ่ม) ---

        float attackStat;
        float defenseStat;

        // เช็ค Category ของท่าที่โจมตีเข้ามา
        if (attack.Base.Category == MoveCategory.Special)
        {
            // ถ้าเป็นท่า Special ให้ใช้ SpAttack และ SpDefense
            attackStat = attacker.SpAttack;
            defenseStat = this.SpDefense;
        }
        else
        {
            // ถ้าเป็น Physical (หรืออื่นๆ) ให้ใช้ Attack และ Defense ปกติ
            attackStat = attacker.Attack;
            defenseStat = this.Defense;
        }

        // ใช้ตัวแปร attackStat และ defenseStat แทนการระบุค่าตรงๆ
        float baseDamage = ((2 * attacker.Level / 5f + 2) * attack.Base.Power * (attackStat / defenseStat)) / 50f + 2;

        // --- ส่วนที่แก้ไข (จบ) ---

        int totalDamage = Mathf.FloorToInt(baseDamage * modifiers);

        if (damageBonus != 0f)
        {
            totalDamage = Mathf.RoundToInt(totalDamage + (totalDamage * damageBonus));
        }

        if (totalDamage < 0) totalDamage = 0;

        HP -= totalDamage;
        if (HP <= 0)
        {
            HP = 0;
            damageDesc.Dead = true;
        }

        return damageDesc;
    }
    public Attack RandomAttack()
    {
        int randId = Random.Range(0, Attacks.Count);
        return Attacks[randId];
    }
}



public class DamageDescription
{
    public float Critical { get; set; }
    public float Type { get; set; }
    public bool Dead { get; set; }
}