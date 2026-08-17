using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 司霆惊蛰被动：进场常态不可选中（无飞行系统时的「起飞」简化）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_Leizi : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.IfDeath)
            return;
        user.UnSelectable();
    }
}
