using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>爆裂黎明：攻加成 + 标记必爆（通过 Context ExplodeProcRate）。</summary>
[Serializable]
public class Buff_WisadelBurst : Buff
{
    [SerializeField] float extraAttack = 0.8f;

    [SerializeField, LabelText("眩晕（爆炸用）")]
    DizznessBuff stunBuff;

    Buff_BaseValueBuff_Attack _attackBuff;

    public Buff_WisadelBurst()
    {
        buffName = "WisadelBurst";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target?.skillController?.context != null)
            target.skillController.context.Set(WisadelKeys.ExplodeProcRate, 1f);

        _attackBuff = new Buff_BaseValueBuff_Attack { extraAttack = extraAttack };
        _attackBuff.target = target;
        _attackBuff.BuffEffect(target);
    }

    public override void BuffOver()
    {
        if (target?.skillController?.context != null)
            target.skillController.context.Set(WisadelKeys.ExplodeProcRate, 0.15f);

        _attackBuff?.BuffOver();
        _attackBuff = null;
        base.BuffOver();
    }

    public DizznessBuff GetStunTemplate() => stunBuff;

    public override Buff Clone()
    {
        var copy = new Buff_WisadelBurst
        {
            extraAttack = extraAttack,
            stunBuff = stunBuff
        };
        return copy;
    }
}
