using System.Collections.Generic;

/// <summary>
/// 秋山澪常规产阳：仅在非溢出模式下与 <see cref="ColdSkill"/> CD 一起判定为可释放。
/// </summary>
public class SkillReady_MioNormalSunMode : ISkillReady
{
    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.skillController?.context == null)
            return false;

        bool overflowMode = false;
        user.skillController.context.TryGet(SkillEffect_MioToggleMode.ContextKeyOverflowMode, out overflowMode);
        return !overflowMode;
    }
}
