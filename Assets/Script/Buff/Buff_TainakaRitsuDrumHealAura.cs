using Sirenix.OdinInspector;
using UnityEngine;
using System;
/// <summary>
/// 律鼓点：按固定 buffName 刷新，不叠加；仅修改 healRate。
/// </summary>
[Serializable]
public class Buff_TainakaRitsuDrumHealAura : Buff
{
    public const string DefaultBuffName = "田井中律鼓点治疗";

    [LabelText("额外治疗倍率"), Tooltip("叠加到 Property.healRate，与 GetExtraDamage 换算结果一致")]
    [Range(0f, 5f)]
    public float extraHealRate;

    float _applied;

    public Buff_TainakaRitsuDrumHealAura()
    {
        buffName = DefaultBuffName;
    }

    public override Buff Clone()
    {
        return (Buff_TainakaRitsuDrumHealAura)base.Clone();
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        _applied = Mathf.Max(0f, extraHealRate);
        if (_applied > 0f)
            target.propertyController.ChangeHealRate(_applied);
    }

    public override void BuffReset(Buff resetBuff)
    {
        if (target != null && _applied > 0f)
            target.propertyController.ChangeHealRate(-_applied);

        if (resetBuff is Buff_TainakaRitsuDrumHealAura o)
            extraHealRate = o.extraHealRate;

        _applied = Mathf.Max(0f, extraHealRate);
        if (target != null && _applied > 0f)
            target.propertyController.ChangeHealRate(_applied);
    }

    public override void BuffOver()
    {
        if (target != null && _applied > 0f)
            target.propertyController.ChangeHealRate(-_applied);
        _applied = 0f;
        base.BuffOver();
    }
}
