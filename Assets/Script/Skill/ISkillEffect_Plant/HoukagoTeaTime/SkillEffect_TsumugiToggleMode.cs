using System.Collections.Generic;

/// <summary>琴吹䌷主动：切换「坚毅 / 回复」形态（写入 skill context）。</summary>
public class SkillEffect_TsumugiToggleMode : ISkillEffect
{
    public const string ContextKeySturdyMode = "tsumugiSturdyMode";

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        bool sturdy = false;
        user.skillController.context.TryGet(ContextKeySturdyMode, out sturdy);
        user.skillController.context.Set(ContextKeySturdyMode, !sturdy);
    }
}
