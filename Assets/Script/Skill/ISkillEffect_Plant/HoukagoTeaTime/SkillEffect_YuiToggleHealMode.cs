using System.Collections.Generic;

/// <summary>平泽唯主动：切换「发呆回血」模式（写入 skill context）。</summary>
public class SkillEffect_YuiToggleHealMode : ISkillEffect
{
    public const string ContextKeyHealMode = "yuiHealMode";

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        bool heal = false;
        user.skillController.context.TryGet(ContextKeyHealMode, out heal);
        user.skillController.context.Set(ContextKeyHealMode, !heal);
    }
}
