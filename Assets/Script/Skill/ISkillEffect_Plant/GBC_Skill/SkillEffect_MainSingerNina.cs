using System.Collections.Generic;

/// <summary>主唱Nina主动：收集无刺有刺成员压力并清空，累加到刺雨伤害，立刻触发刺雨。需配合 SkillReady_RainNotActive 使用。</summary>
public class SkillEffect_TriggerRain : ISkillEffect
{
    const string FetterName = "无刺有刺";
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        var fetter = GameManage.instance.fetterManage.GetFetter(FetterName) as TogenashiTogeari;
        fetter?.TriggerRainFromMainSingerNina();
    }
}
