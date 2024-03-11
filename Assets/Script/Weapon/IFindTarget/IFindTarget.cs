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
        LayerMask enemyLayer=GameManage.instance.GetEnemyLayer(user.gameObject);
        RaycastHit2D hit=Physics2D.Raycast(user.transform.position,user.transform.right,
            user.propertyController.GetAttackRange(),enemyLayer);
        //if (user.CompareTag("Enemy"))
        //{
        //    //Debug.Log(user.gameObject.name+" "+hit.collider+" "+enemyLayer.value);
        //}
        //if (hit.collider == null)
        //    Debug.DrawLine(user.transform.position, user.transform.position + user.transform.right, Color.red);
        //else
        //    Debug.DrawLine(user.transform.position, hit.point, Color.green);
        if (hit.collider != null)
        {
            targets.Add(hit.collider.GetComponent<Chess>());
        }
    }
}
