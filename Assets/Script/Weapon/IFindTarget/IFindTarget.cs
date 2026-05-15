using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IFindTarget 
{
    public void FindTarget(Chess user, List<Chess> targets);
}
public class StraightFindTarget:IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        LayerMask enemyLayer=ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        RaycastHit2D hit=Physics2D.Raycast(user.transform.position,user.transform.right,
            user.propertyController.GetAttackRange(),enemyLayer);
        if (hit.collider != null)
        {
            targets.Add(hit.collider.GetComponent<Chess>());
        }
    }
}
public class StraightFindTargetByDir : IFindTarget
{
    public Transform shooter;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        RaycastHit2D hit = Physics2D.Raycast(user.transform.position, shooter.transform.right,
            user.propertyController.GetAttackRange(), enemyLayer);
        if (hit.collider != null)
        {
            targets.Add(hit.collider.GetComponent<Chess>());
        }
    }
}

/// <summary>
/// 龙然和大喷菇都是用这个逻辑
/// 这个是Aoe找目标
/// </summary>
public class StraightLaser : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user.propertyController.GetAttackRange() <= 0) return;
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int size = 1000 * (int)user.propertyController.GetAttackRange();
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(size);
        int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
            hits, user.propertyController.GetAttackRange(), enemyLayer);
        for (int i = 0; i < num; i++)
        {
            targets.Add(hits[i].collider.GetComponent<Chess>());
        }
        CheckObjectPoolManage.ReleaseArray(size, hits);
    }
}


/// <summary>
/// 这个是找最后一个
/// </summary>
public class StraightFindLastTarget : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        float attackRange = user.propertyController.GetAttackRange();
        if (attackRange <= 0) return;

        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int size = 1000 * (int)attackRange;
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(size);

        int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
            hits, attackRange, enemyLayer);

        // 对hits进行排序：从远到近
        Array.Sort(hits, 0, num, Comparer<RaycastHit2D>.Create((a, b) =>
            Vector2.Distance(user.transform.position, b.point).CompareTo(
            Vector2.Distance(user.transform.position, a.point))));

        // 按排序后的结果添加到targets中
        for (int i = 0; i < num; i++)
        {
            targets.Add(hits[i].collider.GetComponent<Chess>());
        }

        CheckObjectPoolManage.ReleaseArray(size, hits);
    }
}

/// <summary>
/// 直线前方：<see cref="StraightFindLastTarget"/> 的「最后一个敌人」（射线方向上最远的那名敌人）；
/// 若射线路径上有任意单位携带 <see cref="Buff_Zombie_BullyBuff"/>（霸凌标记），则<strong>优先</strong>选择其中距离射击者<strong>最近</strong>的一名。
/// </summary>
[Serializable]
public class StraightFindLastTarget_FindAiming : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        float attackRange = user.propertyController.GetAttackRange();
        if (attackRange <= 0) return;

        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int size = 1000 * (int)attackRange;
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(size);

        int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
            hits, attackRange, enemyLayer);

        Array.Sort(hits, 0, num, Comparer<RaycastHit2D>.Create((a, b) =>
            Vector2.Distance(user.transform.position, b.point).CompareTo(
                Vector2.Distance(user.transform.position, a.point))));

        var seen = new HashSet<Chess>();
        var orderedFarToNear = new List<Chess>();
        for (int i = 0; i < num; i++)
        {
            var c = hits[i].collider != null ? hits[i].collider.GetComponent<Chess>() : null;
            if (c == null || c.IfDeath || !seen.Add(c)) continue;
            orderedFarToNear.Add(c);
        }

        CheckObjectPoolManage.ReleaseArray(size, hits);

        if (orderedFarToNear.Count == 0) return;

        Chess nearestBully = null;
        float bestSq = float.MaxValue;
        Vector2 origin = user.transform.position;
        foreach (var c in orderedFarToNear)
        {
            if (!HasBullyBuff(c)) continue;
            float sq = ((Vector2)c.transform.position - origin).sqrMagnitude;
            if (sq < bestSq)
            {
                bestSq = sq;
                nearestBully = c;
            }
        }

        if (nearestBully != null)
            targets.Add(nearestBully);
        else
            targets.Add(orderedFarToNear[0]);
    }

    static bool HasBullyBuff(Chess chess)
    {
        if (chess?.buffController?.buffDic == null) return false;
        foreach (var kv in chess.buffController.buffDic)
        {
            if (kv.Value is Buff_Zombie_BullyBuff)
                return true;
        }
        return false;
    }
}

public class MultiFindTarget : IFindTarget
{
    [Serializable]
    public class MFindDate
    {
        public List<Chess> chesses;
        [SerializeReference]
        public IFindTarget findTarget;
        public MFindDate()
        {
            chesses = new List<Chess>();
        }
        public void Find(Chess user)
        {
            findTarget.FindTarget(user, chesses);
        }
    }
    [SerializeReference]
    public List<MFindDate> findDates;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        for (int i = 0; i < findDates.Count; i++)
        {
            findDates[i].Find(user);
            for (int j = 0; j < findDates[i].chesses.Count; j++)
            {
                targets.Add(findDates[i].chesses[j]);
            }
        }
    }
}

/// <summary>不锁定任何敌人（<see cref="Weapon_Sample.FindEnemy"/> 返回 0，通常不触发普攻）。</summary>
[Serializable]
public class FindTarget_Empty : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
    }
}

