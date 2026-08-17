using System;

namespace AsoulChess.Game.Entity.Combat
{
    [Flags]
    public enum ElementType
    {
        None = 0,
        CloseAttack = 1 << 0,
        Bullet = 1 << 1,
        AOE = 1 << 2,
        Puncture = 1 << 3,
        Explode = 1 << 4,
        Fire = 1 << 5,
        Cutting = 1 << 6,
        Grind = 1 << 7,
    }

    public enum DamageType
    {
        Physical,
        Magic,
        Real,
        Miss,
        Heal,
    }
}
