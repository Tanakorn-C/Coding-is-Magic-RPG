using UnityEngine;

public class Attack
{
    private AttackBase _base;
    private int _pp;

    public AttackBase Base
    {
        get => _base;
        set => _base = value;
    }
    public int Pp
    {
        get => _pp;
        set => _pp = value;
    }

    public Attack(AttackBase aBase)
    {
        Base = aBase;
        Pp = aBase.PP;
    }
}
