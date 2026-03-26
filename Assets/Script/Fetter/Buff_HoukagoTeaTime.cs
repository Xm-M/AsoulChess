using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 放学后茶会：额外治疗效果（healRate）；伤害在减血前按「先受击者双抗等结算后的 D」均摊为真伤分给全员。
/// 真伤、治疗、Miss 不参与均摊。
/// </summary>
public class Buff_HoukagoTeaTime : Buff
{
    public const string HttBuffName = "放学后茶会";

    [LabelText("额外治疗倍率")] [Tooltip("叠加到 Property.healRate，如 0.15 = +15% 治疗效果")]
    [Range(0f, 2f)]
    public float extraHealRate = 0.15f;

    public Buff_HoukagoTeaTime()
    {
        buffName = HttBuffName;
    }

    public override Buff Clone()
    {
        var c = (Buff_HoukagoTeaTime)base.Clone();
        return c;
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (extraHealRate > 0)
            target.propertyController.ChangeHealRate(extraHealRate);
    }

    public override void BuffOver()
    {
        if (target != null && extraHealRate > 0)
            target.propertyController.ChangeHealRate(-extraHealRate);
        base.BuffOver();
    }

    /// <summary>
    /// 在 GetDamage 中已完成双抗、额外减伤、闪避判定之后、扣血之前调用。
    /// 将 mes.damage 改为 D/n 且类型改为 Real；对其余茶会成员各造成 D/n 真伤。
    /// </summary>
    public static void TryApplyShareAfterMitigation(Chess victim, DamageMessege mes)
    {
        if (mes == null || victim == null || mes.damage <= 0f) return;
        if (mes.damageType == DamageType.Real || mes.damageType == DamageType.Heal || mes.damageType == DamageType.Miss)
            return;

        if (!victim.buffController.buffDic.TryGetValue(HttBuffName, out var b) || !(b is Buff_HoukagoTeaTime))
            return;

        var members = GetActiveHttMembersOnField();
        int n = members.Count;
        if (n <= 1) return;

        float d = mes.damage;
        float per = d / n;

        foreach (var other in members)
        {
            if (other == null || other == victim || other.IfDeath) continue;
            var dm = new DamageMessege
            {
                damageFrom = mes.damageFrom,
                damageTo = other,
                damage = per,
                damageType = DamageType.Real,
                damageElementType = ElementType.None,
                ifCrit = false,
                takeBuff = null
            };
            other.propertyController.GetDamage(dm);
        }

        mes.damage = per;
        mes.damageType = DamageType.Real;
    }

    static List<Chess> _memberBuffer;

    static List<Chess> GetActiveHttMembersOnField()
    {
        if (_memberBuffer == null) _memberBuffer = new List<Chess>(8);
        else _memberBuffer.Clear();

        foreach (var c in GameManage.instance.chessTeamManage.GetTeam("Player"))
        {
            if (c == null || c.IfDeath) continue;
            if (c.propertyController?.creator?.plantTags != null &&
                c.propertyController.creator.plantTags.Contains(HoukagoTeaTime.PlantTag))
                _memberBuffer.Add(c);
        }
        return _memberBuffer;
    }
}
