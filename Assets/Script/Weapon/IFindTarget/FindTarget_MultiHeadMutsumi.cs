using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多首索敌：始终检测本行；有分身时额外检测地图上任意行（攻击距离内射线），
/// 以便邻行/更远行有怪时也能进入 AttackState，由分身弹覆盖。
/// </summary>
public class FindTarget_MultiHeadMutsumi : IFindTarget
{
    static readonly List<int> RowScratch = new List<int>(16);

    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user == null || ChessTeamManage.Instance == null)
            return;
        // 终局锁：不再索敌，无法进入 AttackState
        if (MultiHeadMutsumiKeys.IsFinaleLocked(user))
            return;

        if (!MultiHeadMutsumiKeys.TryGetHomeRow(user, out int homeY))
        {
            LayerMask layer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
            RaycastHit2D hit = Physics2D.Raycast(
                user.transform.position,
                user.transform.right,
                user.propertyController != null ? user.propertyController.GetAttackRange() : 10f,
                layer);
            if (hit.collider != null)
            {
                Chess c = hit.collider.GetComponentInParent<Chess>();
                if (c != null && !c.IfDeath)
                    targets.Add(c);
            }
            return;
        }

        Vector2 origin = user.transform.position;
        Chess home = MultiHeadRowFind.FindEnemyOnRow(user, homeY, origin);
        if (home != null)
        {
            targets.Add(home);
            return;
        }

        if (MultiHeadMutsumiKeys.GetCloneCount(user) <= 0)
            return;

        MapManage map = MapManage.instance;
        int mapH = map != null ? map.mapSize.y : 1;
        MultiHeadMutsumiKeys.BuildRowOrder(homeY, mapH, RowScratch);
        for (int i = 0; i < RowScratch.Count; i++)
        {
            int rowY = RowScratch[i];
            if (rowY == homeY)
                continue;
            Chess enemy = MultiHeadRowFind.FindEnemyOnRow(user, rowY, origin);
            if (enemy == null)
                continue;
            targets.Add(enemy);
            return;
        }
    }
}
