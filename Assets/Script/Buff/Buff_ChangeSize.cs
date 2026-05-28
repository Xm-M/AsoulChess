using System;
using UnityEngine;

/// <summary>
/// 将单位 <see cref="Transform.localScale"/> 设为指定值；<see cref="BuffOver"/> 时恢复施加前的缩放。
/// </summary>
[Serializable]
public class Buff_ChangeSize : Buff
{
    public Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

    Vector3 _originalScale;
    bool _hasOriginal;

    public Buff_ChangeSize()
    {
        buffName = "ChangeSize";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target == null)
            return;

        if (!_hasOriginal)
        {
            _originalScale = target.transform.localScale;
            _hasOriginal = true;
        }

        target.transform.localScale = scale;
    }

    public override void BuffOver()
    {
        if (target != null && _hasOriginal)
            target.transform.localScale = _originalScale;

        _hasOriginal = false;
        base.BuffOver();
    }

    public override void BuffReset(Buff resetBuff)
    {
        if (resetBuff is not Buff_ChangeSize other || target == null)
            return;

        if (_hasOriginal)
            target.transform.localScale = _originalScale;

        scale = other.scale;
        target.transform.localScale = scale;
    }

    public override Buff Clone()
    {
        var c = (Buff_ChangeSize)base.Clone();
        c._hasOriginal = false;
        return c;
    }
}
