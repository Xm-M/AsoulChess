using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        foreach (var friend in GameManage.instance.chessTeamManage.GetTeam(user.tag))
        {
            if (Vector2.Distance(friend.transform.position, user.transform.position) < user.propertyController.GetAttackRange())
            {
                targets.Add(friend);
            }
        }
    }
}
