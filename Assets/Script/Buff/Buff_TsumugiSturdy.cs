using System;
using UnityEngine;

/// <summary>
/// 琴吹䌷坚毅形态：50% 额外减伤 + 体型 +10（仅 <see cref="PropertyController.ChangeSize"/>）。
/// </summary>
[Serializable]
public class Buff_TsumugiSturdy : Buff
{
    [SerializeReference]
    public Buff_BaseValueBuff_ExtraDefence extraDefenceBuff;
    [SerializeReference]
    public Buff_BaseValueBuff_Size sizeBuff;

    public float extraDefence = 0.5f;
    public int sizeAdd = 10;

    public Buff_TsumugiSturdy()
    {
        buffName = "Buff_TsumugiSturdy";
    }

    void EnsureBuffs()
    {
        if (extraDefenceBuff == null)
            extraDefenceBuff = new Buff_BaseValueBuff_ExtraDefence { extraDefence = extraDefence };
        if (sizeBuff == null)
            sizeBuff = new Buff_BaseValueBuff_Size { size = sizeAdd };
    }

    public override Buff Clone()
    {
        var c = (Buff_TsumugiSturdy)base.Clone();
        c.extraDefenceBuff = extraDefenceBuff != null
            ? (Buff_BaseValueBuff_ExtraDefence)extraDefenceBuff.Clone()
            : null;
        c.sizeBuff = sizeBuff != null ? (Buff_BaseValueBuff_Size)sizeBuff.Clone() : null;
        return c;
    }

    protected override void PrepareForRestore() => EnsureBuffs();

    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        extraDefenceBuff.target = target;
        extraDefenceBuff.BuffEffect(target);
        sizeBuff.target = target;
        sizeBuff.BuffEffect(target);
    }

    public override void BuffOver()
    {
        if (extraDefenceBuff != null)
            extraDefenceBuff.BuffOver();
        if (sizeBuff != null)
            sizeBuff.BuffOver();
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is Buff_TsumugiSturdy other)
        {
            extraDefence = other.extraDefence;
            sizeAdd = other.sizeAdd;
        }
        EnsureBuffs();
        if (extraDefenceBuff != null && resetBuff is Buff_TsumugiSturdy o2 && o2.extraDefenceBuff != null)
            extraDefenceBuff.BuffReset(o2.extraDefenceBuff);
        if (sizeBuff != null && resetBuff is Buff_TsumugiSturdy o3 && o3.sizeBuff != null)
            sizeBuff.BuffReset(o3.sizeBuff);
    }
}
