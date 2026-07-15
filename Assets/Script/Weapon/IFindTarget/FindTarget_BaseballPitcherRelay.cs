using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 投手合并索敌：敌人门控（B1）→ 有挥棒手则喂球，否则走原敌人索敌。
/// </summary>
[Serializable]
public class FindTarget_BaseballPitcherRelay : IFindTarget
{
    [SerializeReference]
    public IFindTarget enemyFindTarget;

    static readonly List<Chess> EnemyScratch = new List<Chess>();

    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user == null || user.IfDeath || enemyFindTarget == null)
            return;

        EnemyScratch.Clear();
        enemyFindTarget.FindTarget(user, EnemyScratch);

        bool hasEnemy = false;
        for (int i = 0; i < EnemyScratch.Count; i++)
        {
            if (EnemyScratch[i] != null && !EnemyScratch[i].IfDeath)
            {
                hasEnemy = true;
                break;
            }
        }

        if (!hasEnemy)
            return;

        Chess batter = BaseballRelayHelper.TryFindBatterInRelayRange(user);
        if (batter != null)
        {
            targets.Add(batter);
            return;
        }

        for (int i = 0; i < EnemyScratch.Count; i++)
        {
            Chess c = EnemyScratch[i];
            if (c != null && !c.IfDeath && !targets.Contains(c))
                targets.Add(c);
        }
    }
}
