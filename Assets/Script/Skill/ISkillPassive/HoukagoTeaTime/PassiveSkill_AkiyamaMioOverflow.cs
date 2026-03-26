using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 秋山澪被动：在 <see cref="PropertyController.onSetDamage"/> 中检测治疗，按与 <see cref="PropertyController.Heal"/> 相同规则计算溢出量，
/// 累加到 <see cref="SkillContext"/>（键 <see cref="SkillEffect_AkiyamaMioSun.ContextKeyOverflowHealBuffer"/>）。
/// 阳光在主动技能效果里与基础产额一起结算。
/// 注意：溢出用当前帧的 mes.damage×healRate 与当前 Hp 计算；若其他监听者在 onSetDamage 里改写了治疗量，需保证与 Heal 最终用量一致（或调整监听顺序）。
/// </summary>
public class PassiveSkill_AkiyamaMioOverflow : ISkillEffect
{
    Chess user;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        user.propertyController.onSetDamage.AddListener(OnSetDamage);
        user.OnRemove.AddListener(OnChessRemove);
        user.skillController.context.Set(SkillEffect_AkiyamaMioSun.ContextKeyOverflowHealBuffer, 0f);
    }

    void OnSetDamage(DamageMessege mes)
    {
        if (user == null || mes.damageType != DamageType.Heal) return;
        float heal = mes.damage * user.propertyController.GetHealRate();
        float room = Mathf.Max(0f, user.propertyController.GetMaxHp() - user.propertyController.GetHp());
        float overflow = Mathf.Max(0f, heal - room);
        Debug.Log("溢出量为:" + overflow);
        if (overflow <= 0f) return;
        float cur = 0f;
        user.skillController.context.TryGet(SkillEffect_AkiyamaMioSun.ContextKeyOverflowHealBuffer, out cur);
        user.skillController.context.Set(SkillEffect_AkiyamaMioSun.ContextKeyOverflowHealBuffer, cur + overflow);
    }

    void OnChessRemove(Chess c)
    {
        if (user != null && user.propertyController != null)
        {
            user.propertyController.onSetDamage.RemoveListener(OnSetDamage);
            user.OnRemove.RemoveListener(OnChessRemove);
        }
    }
}
