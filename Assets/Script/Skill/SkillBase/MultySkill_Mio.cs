using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 秋山澪复合主动：在 <see cref="SkillEffect_MioToggleMode"/> 切换的形态下，分发三条子技能。
/// 点击优先切换；常规模式 CD 产阳（复用 <see cref="SkillEffect_CreateSunLight"/>）；溢出模式达阈值产阳。
/// 动画 <c>skill</c>：0=切换，1=常规产阳，2=溢出转阳。
/// </summary>
public class MultySkill_Mio : ISkill, ISkillFireUseSkillOnEnter, ISkillCooldownProgress
{
    public const int AnimIndexToggle = 0;
    public const int AnimIndexSunNormal = 1;
    public const int AnimIndexOverflowHeal = 2;

    [SerializeReference]
    public ISkill toggleSkill;

    [Tooltip("常规模式：CD 到后播 skill1，effect 建议 SkillEffect_CreateSunLight")]
    [SerializeReference]
    public ISkill sunNormalSkill;

    [Tooltip("溢出模式：缓存达阈值后播 skill2，effect 建议 SkillEffect_OverflowHeal")]
    [SerializeReference]
    public ISkill overflowHealSkill;

    public string skillTag = "skill";

    ISkill currentSkill;

    public bool IfSkillReady(Chess user)
    {
        if (toggleSkill != null && toggleSkill.IfSkillReady(user))
        {
            currentSkill = toggleSkill;
            SetSkillAnimIndex(user, AnimIndexToggle);
            return true;
        }

        if (!IsOverflowMode(user) && sunNormalSkill != null && sunNormalSkill.IfSkillReady(user))
        {
            currentSkill = sunNormalSkill;
            SetSkillAnimIndex(user, AnimIndexSunNormal);
            return true;
        }

        if (IsOverflowMode(user) && overflowHealSkill != null && overflowHealSkill.IfSkillReady(user))
        {
            currentSkill = overflowHealSkill;
            SetSkillAnimIndex(user, AnimIndexOverflowHeal);
            return true;
        }

        return false;
    }

    public void UseSkill(Chess user)
    {
        if (currentSkill != null)
            currentSkill.UseSkill(user);
    }

    public void SkillOver(Chess user)
    {
        currentSkill?.SkillOver(user);
    }

    public void InitSkill(Chess user)
    {
        toggleSkill?.InitSkill(user);
        sunNormalSkill?.InitSkill(user);
        overflowHealSkill?.InitSkill(user);
    }

    public void LeaveSkill(Chess user)
    {
        toggleSkill?.LeaveSkill(user);
        sunNormalSkill?.LeaveSkill(user);
        overflowHealSkill?.LeaveSkill(user);
    }

    public void WhenEnter(Chess user)
    {
        toggleSkill?.WhenEnter(user);
        sunNormalSkill?.WhenEnter(user);
        overflowHealSkill?.WhenEnter(user);
        if (user?.skillController?.context != null
            && !user.skillController.context.TryGet<bool>(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out _))
            user.skillController.context.Set(SkillEffect_MioToggleMode.ContextKeyOverflowMode, false);
    }

    public bool IsSkillFinished(Chess user)
    {
        return currentSkill == null || currentSkill.IsSkillFinished(user);
    }

    public SkillConfig GetSkillConfig()
    {
        return currentSkill?.GetSkillConfig();
    }

    public void ReturnCD()
    {
        currentSkill?.ReturnCD();
    }

    /// <summary>切换：进场即结算；常规/溢出产阳：动画事件 <see cref="Chess.UseSkill"/>。</summary>
    public void FireUseSkillOnEnter(Chess chess)
    {
        if (currentSkill is ColdSkill_YuiToggle)
            chess.UseSkill();
    }

    public float GetCooldownProgress01()
    {
        if (toggleSkill is ISkillCooldownProgress p)
            return Mathf.Clamp01(p.GetCooldownProgress01());
        return 1f;
    }

    public void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(MultySkill_Mio);
        data.Set("currentIndex", ResolveAnimIndex(currentSkill));
    }

    public void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        currentSkill = SkillAtAnimIndex(data.GetInt("currentIndex", 0));
    }

    ISkill SkillAtAnimIndex(int idx)
    {
        switch (idx)
        {
            case AnimIndexSunNormal: return sunNormalSkill ?? toggleSkill;
            case AnimIndexOverflowHeal: return overflowHealSkill ?? toggleSkill;
            default: return toggleSkill;
        }
    }

    int ResolveAnimIndex(ISkill skill)
    {
        if (skill == sunNormalSkill) return AnimIndexSunNormal;
        if (skill == overflowHealSkill) return AnimIndexOverflowHeal;
        return AnimIndexToggle;
    }

    static bool IsOverflowMode(Chess user)
    {
        if (user?.skillController?.context == null)
            return false;
        bool mode = false;
        user.skillController.context.TryGet(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out mode);
        return mode;
    }

    static void SetSkillAnimIndex(Chess user, int index)
    {
        if (user?.animatorController?.animator == null)
            return;
        if (AnimatorController.HasParameter(user.animatorController.animator, "skill"))
            user.animatorController.animator.SetInteger("skill", index);
    }
}
