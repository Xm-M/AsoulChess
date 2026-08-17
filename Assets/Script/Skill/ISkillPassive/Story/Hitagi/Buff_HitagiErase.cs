using System;
using UnityEngine;

/// <summary>橡皮擦命中叠层；满层直接死亡。</summary>
[Serializable]
public class Buff_HitagiErase : Buff
{
    [Min(1)]
    public int maxStacks = 5;

    int _stacks = 1;

    public Buff_HitagiErase()
    {
        buffName = "擦除";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        _stacks = 1;
        TryExecute();
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        _stacks++;
        if (resetBuff is Buff_HitagiErase other && other.maxStacks > 0)
            maxStacks = other.maxStacks;
        TryExecute();
    }

    void TryExecute()
    {
        if (target == null || target.IfDeath) return;
        if (_stacks < maxStacks) return;
        target.Death();
    }

    public override int GetStackCount() => _stacks;

    public override void SetStackCount(int v)
    {
        _stacks = Mathf.Max(1, v);
    }

    public override Buff Clone()
    {
        var c = (Buff_HitagiErase)base.Clone();
        c._stacks = 1;
        return c;
    }
}
