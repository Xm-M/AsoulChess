using System.Collections.Generic;

/// <summary>
/// 老仓育主动：按当前行列友方数刷新 A/B，按当前压力 stress:30 刷新 a:b；不修改压力。
/// </summary>
public class SkillEffect_OkuwakiRefresh : ISkillEffect
{
    public int maxAmplitude = OkuwakiCurveKeys.DefaultMaxAmplitude;
    public int ratioMax = OkuwakiCurveKeys.DefaultRatioMax;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;
        OkuwakiCurveState.RefreshFromSkill(user, maxAmplitude, ratioMax);
    }
}
