using System.Collections.Generic;

/// <summary>秋山澪：点击切换「常规 / 溢出转阳」模式（写入 skill context）。</summary>
public class SkillEffect_MioToggleMode : ISkillEffect
{
    /// <summary>true = 溢出转阳模式（被动累计溢出、主动可结算产阳）。</summary>
    public const string ContextKeyOverflowMode = "mioOverflowMode";

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        bool overflow = false;
        user.skillController.context.TryGet(ContextKeyOverflowMode, out overflow);
        user.skillController.context.Set(ContextKeyOverflowMode, !overflow);
    }
}
