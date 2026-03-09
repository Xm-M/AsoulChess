using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICircleSearch : IFindTarget
{
    public float checkRange;
    public Vector3 dx;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        Collider2D[] cols = CheckObjectPoolManage.GetColArray((int)(1000*checkRange*checkRange));
        LayerMask layer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int i = Physics2D.OverlapCircleNonAlloc(user.transform.position+dx, checkRange, cols, layer);
        //Debug.Log("找到了" + i);
        for (int j = 0; j < i; j++)
        {
            Chess enemy = cols[j].GetComponent<Chess>();
            targets.Add(enemy);
        }
        CheckObjectPoolManage.ReleaseColArray(1000, cols);
    }
}
public class ICircleSerach_EnemyListSearch : IFindTarget
{
    public float checkRange;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        foreach(var chess in ChessTeamManage.Instance.GetEnemyTeam(user.tag))
        {
            
            if (Vector2.Distance(chess.transform.position, user.transform.position) <= checkRange)
            {
                //Debug.Log(chess);
                targets.Add(chess);
            }
        }
    }
}