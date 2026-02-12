using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 也就是说凉的技能持续时间和CD是相同的 也就是说技能好了 CD也就好了
/// </summary>
public class SkillEffect_Liang : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        foreach(var target in targets)
        {
            if (target.propertyController.GetSize() < user.propertyController.GetSize())
            {
                target.Death();
            }
        }
    }
}
