using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 秋山澪被动：在 <see cref="PropertyController.onSetDamage"/> 中检测治疗，按与 <see cref="PropertyController.Heal"/> 相同规则计算溢出量，
/// 累加到 <see cref="SkillContext"/>（键 <see cref="SkillEffect_OverflowHeal.ContextKeyOverflowHealBuffer"/>）。
/// 仅在溢出模式下累计；阳光由 <see cref="SkillEffect_OverflowHeal"/> 主动结算。
/// 监听 <see cref="SkillEffect_MioToggleMode"/> 切换，同步 Animator <c>Blend</c>（<see cref="AnimatorController.ChangeFloat"/> 0/1）。
/// 注意：溢出用当前帧的 mes.damage×healRate 与当前 Hp 计算；若其他监听者在 onSetDamage 里改写了治疗量，需保证与 Heal 最终用量一致（或调整监听顺序）。
/// </summary>
public class PassiveSkill_AkiyamaMioOverflow : ISkillEffect
{
    Chess user;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        user.propertyController.onSetDamage.AddListener(OnSetDamage);
        user.skillController.context.OnValueChange.AddListener(SyncFromContext);
        user.OnRemove.AddListener(OnChessRemove);

        if (!user.skillController.context.TryGet<bool>(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out _))
            user.skillController.context.Set(SkillEffect_MioToggleMode.ContextKeyOverflowMode, false);
        user.skillController.context.Set(SkillEffect_OverflowHeal.ContextKeyOverflowHealBuffer, 0f);
        SyncFromContext();
    }

    void SyncFromContext()
    {
        if (user?.skillController?.context == null || user.animatorController == null)
            return;

        bool overflowMode = false;
        user.skillController.context.TryGet(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out overflowMode);
        user.animatorController.ChangeFloat(overflowMode ? 1f : 0f);
    }

    void OnSetDamage(DamageMessege mes)
    {
        if (user == null || mes.damageType != DamageType.Heal) return;
        bool overflowMode = false;
        user.skillController.context.TryGet(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out overflowMode);
        if (!overflowMode) return;
        float heal = mes.damage * user.propertyController.GetHealRate();
        float room = Mathf.Max(0f, user.propertyController.GetMaxHp() - user.propertyController.GetHp());
        float overflow = Mathf.Max(0f, heal - room);
        if (overflow <= 0f) return;
        float cur = 0f;
        user.skillController.context.TryGet(SkillEffect_OverflowHeal.ContextKeyOverflowHealBuffer, out cur);
        user.skillController.context.Set(SkillEffect_OverflowHeal.ContextKeyOverflowHealBuffer, cur + overflow);
    }

    void OnChessRemove(Chess c)
    {
        if (user == null)
            return;
        if (user.propertyController != null)
            user.propertyController.onSetDamage.RemoveListener(OnSetDamage);
        if (user.skillController?.context != null)
            user.skillController.context.OnValueChange.RemoveListener(SyncFromContext);
        user.OnRemove.RemoveListener(OnChessRemove);
    }
}
