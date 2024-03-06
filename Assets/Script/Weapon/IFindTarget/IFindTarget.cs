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
        RaycastHit2D[] hits = GameManage.instance.checObjectPoolManage.GetHitArray(100);
        int i = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
            hits,100,user.gameObject.layer);
        for(int n = 0; n < i; n++)
        {
            Chess t = hits[n].collider.GetComponent<Chess>();
            if ( t!= null && !t.CompareTag(user.tag))
            {
                targets.Add(t);
                break;
            }
        }
        GameManage.instance.checObjectPoolManage.ReleaseArray(100, hits);
    }
}
