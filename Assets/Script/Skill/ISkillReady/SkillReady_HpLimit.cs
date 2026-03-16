using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SkillReady_HpLimit : ISkillReady
{
    [LabelText("生命值百分比阈值")]
    [Range(0f, 1f)]
    [Tooltip("当生命值百分比低于此值时返回 true，如 0.3 表示 30% 以下")]
    public float hpPercentThreshold = 0.3f;

    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        return user?.propertyController != null && user.propertyController.GetHpPerCent() <= hpPercentThreshold;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }
}
