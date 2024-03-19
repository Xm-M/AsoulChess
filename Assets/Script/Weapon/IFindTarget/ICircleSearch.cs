using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICircleSearch : IFindTarget
{
    public float checkRange;
    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        LayerMask layer = GameManage.instance.GetEnemyLayer(user.gameObject);
        int i = Physics2D.OverlapCircleNonAlloc(user.transform.position, checkRange, cols, layer);
        for (int j = 0; j < i; j++)
        {
            Chess enemy = cols[j].GetComponent<Chess>();
            targets.Add(enemy);
        }
        CheckObjectPoolManage.ReleaseColArray(100, cols);
    }
}
