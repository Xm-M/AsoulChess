using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFindAll_Circle : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        Collider2D[] hits = CheckObjectPoolManage.GetColArray(100);
        int enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int friendLayer = 1 << user.gameObject.layer; // 将 Layer Index 转换成 LayerMask
        int combinedLayerMask = enemyLayer | friendLayer; // 同时检测敌我
        int n= Physics2D.OverlapCircleNonAlloc(user.transform.position, user.propertyController.GetAttackRange(),hits, combinedLayerMask);
        for(int i=0;i<n;i++)
        {
            Chess chess = hits[i].GetComponent<Chess>();
            targets.Add(chess);   
        }
        CheckObjectPoolManage.ReleaseColArray(100, hits);
    }
}

public class StraightLaser_AllChess : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100);
        int enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int friendLayer = 1 << user.gameObject.layer; // 将 Layer Index 转换成 LayerMask

        int combinedLayerMask = enemyLayer | friendLayer; // 同时检测敌我

        int n = Physics2D.RaycastNonAlloc(
            user.transform.position,
            user.transform.right,
            hits,
            user.propertyController.GetAttackRange(),
            combinedLayerMask
        );
        for (int i = 0; i < n; i++)
        {
            Chess chess = hits[i].collider.GetComponent<Chess>();
            if (user != chess)
            {
                targets.Add(chess);
            }
        }
        CheckObjectPoolManage.ReleaseArray(100, hits);
    }
}
