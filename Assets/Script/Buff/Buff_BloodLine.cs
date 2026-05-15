using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 血线连结：挂在红线目标身上。多条红线共用同一 Buff（同名叠层 <see cref="BuffReset"/> 合并 <see cref="sourceArmors"/>）。
/// 若本次结算伤害带 <see cref="ElementType.Cutting"/>，则<strong>所有</strong>牵线 <see cref="BloodLineArmor.BreakLineOnly"/>（默认触发防具破碎事件）仅断线，
/// 再对自身造成一次「当前生命值 × <see cref="cuttingSelfHpFraction"/>」的真实伤害，最后 <see cref="BuffOver"/>（内部对牵线再做一次静默断线以防遗漏）。
/// </summary>
public class Buff_BloodLine : Buff
{
    public const string BuffKey = "血线连结";

    /// <summary>任一条牵线的防具携带者（首条写入，合并时保留已有）。</summary>
    [HideInInspector] public Chess lineOwner;

    /// <summary>所有仍生效的 <see cref="BloodLineArmor"/>；切割时全部 <see cref="BloodLineArmor.BreakLineOnly"/>。</summary>
    [HideInInspector] public List<BloodLineArmor> sourceArmors;

    [Tooltip("受切割时，按当前生命比例追加的真实伤害（整次切割只结算一次）")]
    [Range(0f, 1f)]
    public float cuttingSelfHpFraction = 0.5f;

    UnityAction<DamageMessege> _cutListener;

    public Buff_BloodLine()
    {
        buffName = BuffKey;
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        if (resetBuff is not Buff_BloodLine other) return;
        if (sourceArmors == null) sourceArmors = new List<BloodLineArmor>();
        if (other.sourceArmors != null)
        {
            foreach (var a in other.sourceArmors)
            {
                if (a != null && !sourceArmors.Contains(a))
                    sourceArmors.Add(a);
            }
        }
        if (lineOwner == null && other.lineOwner != null)
            lineOwner = other.lineOwner;
    }

    public override void BuffEffect(Chess target)
    {
        if (string.IsNullOrEmpty(buffName))
            buffName = BuffKey;
        if (sourceArmors == null)
            sourceArmors = new List<BloodLineArmor>();
        base.BuffEffect(target);
        _cutListener = OnTargetSetDamage;
        target.propertyController.onSetDamage.AddListener(_cutListener);
    }

    void OnTargetSetDamage(DamageMessege mes)
    {
        if (target == null || target.IfDeath || mes.damageTo != target) return;
        if ((mes.damageElementType & ElementType.Cutting) == 0) return;

        if (_cutListener != null)
        {
            target.propertyController.onSetDamage.RemoveListener(_cutListener);
            _cutListener = null;
        }

        if (sourceArmors != null && sourceArmors.Count > 0)
        {
            var copy = new List<BloodLineArmor>(sourceArmors);
            foreach (var a in copy)
            {
                if (a != null)
                    a.BreakLineOnly();
            }
        }

        float extra = target.propertyController.GetHp() * cuttingSelfHpFraction;
        if (extra > 0f)
        {
            var selfHit = new DamageMessege(target, target, extra, DamageType.Real);
            target.propertyController.GetDamage(selfHit);
        }

        BuffOver();
    }

    public override void BuffOver()
    {
        if (target != null && _cutListener != null)
            target.propertyController.onSetDamage.RemoveListener(_cutListener);
        _cutListener = null;

        if (sourceArmors != null)
        {
            foreach (var a in sourceArmors)
            {
                if (a != null)
                    a.BreakLineOnly(false);
            }
            sourceArmors.Clear();
        }
        sourceArmors = null;
        lineOwner = null;
        base.BuffOver();
    }
}
