using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>全屏最近敌人；仅当进入攻击范围时写入 targets（供近战 AttackState）。</summary>
[Serializable]
public class FindTarget_NearestEnemyInRange : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user == null || ChessTeamManage.Instance == null)
            return;

        Chess nearest = FindNearestEnemy(user, out float dist);
        if (nearest == null)
            return;

        float range = user.propertyController != null
            ? user.propertyController.GetAttackRange()
            : 1f;
        if (dist <= range)
            targets.Add(nearest);
    }

    public static Chess FindNearestEnemy(Chess user, out float distance)
    {
        distance = float.MaxValue;
        Chess best = null;
        if (user == null || ChessTeamManage.Instance == null)
            return null;

        var enemies = ChessTeamManage.Instance.GetEnemyTeam(user.tag);
        if (enemies == null)
            return null;

        Vector2 origin = user.transform.position;
        for (int i = 0; i < enemies.Count; i++)
        {
            Chess e = enemies[i];
            if (e == null || e.IfDeath)
                continue;
            float d = Vector2.Distance(origin, e.transform.position);
            if (d < distance)
            {
                distance = d;
                best = e;
            }
        }
        return best;
    }
}
