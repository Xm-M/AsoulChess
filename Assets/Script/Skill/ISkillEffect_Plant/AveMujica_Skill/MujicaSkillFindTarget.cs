using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// AveMujica 技能效果共用：从 <see cref="SkillReady_IfTargetInRange.search"/> 或武器索敌收集敌人。
/// </summary>
public static class MujicaSkillFindTarget
{
    public static void CollectEnemies(
        Chess user,
        ISkill skill,
        List<Chess> outTargets,
        IFindTarget searchOverride = null)
    {
        outTargets.Clear();
        if (user == null)
            return;

        IFindTarget search = searchOverride ?? TryGetSearchFromSkillReady(skill);
        if (search != null)
            search.FindTarget(user, outTargets);
        else if (user.equipWeapon?.weapon is Weapon_Sample ws && ws.findTarget != null)
            ws.findTarget.FindTarget(user, outTargets);
    }

    static IFindTarget TryGetSearchFromSkillReady(ISkill skill)
    {
        if (skill == null)
            return null;

        for (var type = skill.GetType(); type != null; type = type.BaseType)
        {
            var field = type.GetField("readyChecker", BindingFlags.Instance | BindingFlags.Public);
            if (field?.GetValue(skill) is SkillReady_IfTargetInRange range)
                return range.search;
        }
        return null;
    }
}
