using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 秋山澪溢出产阳：仅在溢出模式下，且溢出缓存 ≥ 阈值时，与 <see cref="ColdSkill"/> CD 一起判定为可释放。
/// </summary>
public class SkillReady_MioOverflowHeal : ISkillReady
{
    [Tooltip("与 SkillEffect_OverflowHeal.overflowThreshold 保持一致")]
    [MinValue(0.01f)]
    public float overflowThreshold = 50f;

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.skillController?.context == null)
            return false;

        bool overflowMode = false;
        user.skillController.context.TryGet(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out overflowMode);
        if (!overflowMode)
            return false;

        float buffer = 0f;
        user.skillController.context.TryGet(SkillEffect_OverflowHeal.ContextKeyOverflowHealBuffer, out buffer);
        return buffer >= overflowThreshold;
    }
}
