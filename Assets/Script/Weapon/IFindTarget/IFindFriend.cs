using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>友方治疗索敌共用：满血友军不作为治疗目标。</summary>
public static class HealFindTargetUtil
{
    const float FullHpEpsilon = 0.0001f;

    public static bool IsAllyAtFullHp(Chess ally)
    {
        if (ally == null || ally.propertyController == null)
            return true;
        var pc = ally.propertyController;
        float maxHp = pc.GetMaxHp();
        if (maxHp <= 0f)
            return true;
        return pc.GetHp() >= maxHp - FullHpEpsilon;
    }
}

public class StraightFindFirend_Heal_HpMin : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        Chess healTarget = null;
        float minHp=1;
        foreach(var friend in GameManage.instance.chessTeamManage.GetTeam(user.tag))
        {
            if (Vector2.Distance(friend.transform.position, user.transform.position) < user.propertyController.GetAttackRange())
            {
                if (HealFindTargetUtil.IsAllyAtFullHp(friend))
                    continue;
                if (friend.propertyController.GetHpPerCent() < minHp)
                {
                    minHp = friend.propertyController.GetHpPerCent();
                    healTarget = friend;
                }
            }
        }
        if (healTarget != null) targets.Add(healTarget);
    }
}
public class FindFriend_CircleRange_HealAll : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        foreach (var friend in GameManage.instance.chessTeamManage.GetTeam(user.tag))
        {
            if (Vector2.Distance(friend.transform.position, user.transform.position) < user.propertyController.GetAttackRange())
            {
                if (HealFindTargetUtil.IsAllyAtFullHp(friend))
                    continue;
                targets.Add(friend);
            }
        }
    }
}
