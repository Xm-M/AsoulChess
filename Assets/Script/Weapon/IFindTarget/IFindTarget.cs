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
/// 
/// </summary>
public class StraightLaser : IFindTarget
{
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        RaycastHit2D[] hits= CheckObjectPoolManage.GetHitArray(100*(int)user.propertyController.GetAttackRange());
        int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
            hits,user.propertyController.GetAttackRange(), enemyLayer);
        for(int i = 0; i < num; i++) 
        {
            targets.Add(hits[i].collider.GetComponent<Chess>());
        }
        CheckObjectPoolManage.ReleaseArray(100, hits);
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
        for(int i = 0; i<findDates.Count; i++)
        {
            findDates[i].Find(user);
            for(int j = 0; j < findDates[i].chesses.Count; j++)
            {
                targets.Add(findDates[i].chesses[j]);
            }
        }
    }
}
