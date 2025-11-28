using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "Creature", menuName = "Creature/New Creature")]
public class CreatureBase : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string creatureName;
    public string CreatureName => creatureName;

    [TextArea]
    [SerializeField] private string description;
    public string Description => description;

    [SerializeField] private Sprite frontSprite;
    public Sprite FrontSprite => frontSprite;

    [SerializeField] private Sprite backSprite;
    public Sprite BackSprite => backSprite;

    [SerializeField] private CreatureType type;
    public CreatureType Type => type;
    [Header("Quiz Settings")]
    [SerializeField] private QuestionCategory associatedCategory;
    public QuestionCategory AssociatedCategory => associatedCategory;

    [SerializeField] private QuestionDifficulty associatedDifficulty;
    public QuestionDifficulty AssociatedDifficulty => associatedDifficulty;

    // Stats
    [SerializeField] private int maxHP;
    public int MaxHP => maxHP;

    [SerializeField] private int attack;
    public int Attack => attack;

    [SerializeField] private int defense;
    public int Defense => defense;

    [SerializeField] private int spAttack;
    public int SpAttack => spAttack;

    [SerializeField] private int spDefense;
    public int SpDefense => spDefense;

    [SerializeField] private int speed;
    public int Speed => speed;

    [SerializeField] private List<LearnableAttack> learnableAttacks;
    public List<LearnableAttack> LearnableAttacks => learnableAttacks;
}

public enum CreatureType
{
    Wind,
    Fire,
    Shock,
    Aqua,
    Earth,
}

public class TypeMatrix
{
    private static float[][] matrix =
    {
        //                   WIND  FIRE  SHOC  AQUA  EART 
        /*WIND*/ new float[]{0.5f, 1.0f, 1.0f, 1.5f, 1.0f},
        /*FIRE*/ new float[]{1.0f, 0.5f, 1.0f, 0.5f, 1.5f},
        /*SHOC*/ new float[]{2.0f, 2.0f, 2.0f, 2.0f, 2.0f},
        /*AQUA*/ new float[]{1.0f, 1.5f, 1.0f, 0.5f, 1.0f},
        /*EART*/ new float[]{1.5f, 1.0f, 1.0f, 1.0f, 0.5f}
    };

    public static float GetMultEffectiveness(CreatureType attackType, CreatureType defenderType)
    {
        int row = (int)attackType;
        int col = (int)defenderType;

        return matrix[row][col];
    }
}

[Serializable]
public class LearnableAttack
{
    [SerializeField] private AttackBase attack;
    [SerializeField] private int level;
    public int Level => level;

    // FIX: The public 'Attack' property must return the private '[SerializeField] attack' field.
    // The old property '{ get; internal set; }' was not connected and would always be null.
    public AttackBase Attack => attack;
}