using System;
using UnityEngine;

/// <summary>
/// 僵王站立期间 100% 额外减伤 + 免疫控制（韧性拉满）；俯身时由 <see cref="Skill_ZombieKingBoss"/> 移除。
/// </summary>
[Serializable]
public class Buff_ZombieKingStandImmunity : Buff
{
    public const string BuffKey = "Buff_ZombieKingStandImmunity";

    [SerializeReference]
    public Buff_BaseValueBuff_ExtraDefence extraDefenceBuff;

    public float extraDefence = 1f;

    float _savedTenacity;

    public Buff_ZombieKingStandImmunity()
    {
        buffName = BuffKey;
    }

    public static bool IsActiveOn(Chess chess) =>
        chess?.buffController?.buffDic != null
        && chess.buffController.buffDic.ContainsKey(BuffKey);

    public static bool BlocksControlBuff(Buff buff) =>
        buff is DizznessBuff || buff is DizzyBuff;

    void EnsureBuff()
    {
        if (extraDefenceBuff == null)
            extraDefenceBuff = new Buff_BaseValueBuff_ExtraDefence { extraDefence = extraDefence };
    }

    public override Buff Clone()
    {
        var c = (Buff_ZombieKingStandImmunity)base.Clone();
        c.extraDefenceBuff = extraDefenceBuff != null
            ? (Buff_BaseValueBuff_ExtraDefence)extraDefenceBuff.Clone()
            : null;
        return c;
    }

    protected override void PrepareForRestore() => EnsureBuff();

    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        extraDefenceBuff.target = target;
        extraDefenceBuff.BuffEffect(target);
        _savedTenacity = target.propertyController.GetTenacity();
        target.propertyController.SetTenacity(1f);
    }

    public override void BuffOver()
    {
        if (extraDefenceBuff != null)
            extraDefenceBuff.BuffOver();
        if (target?.propertyController != null)
            target.propertyController.SetTenacity(_savedTenacity);
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is Buff_ZombieKingStandImmunity other && other.extraDefenceBuff != null && extraDefenceBuff != null)
            extraDefenceBuff.BuffReset(other.extraDefenceBuff);
    }
}
