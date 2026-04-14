using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_Soyo : AnimatorController
{
    bool friend;
    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        friend = false;
    }
 
    protected override void Update()
    {
        LayerMask enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(chess.gameObject);
        RaycastHit2D hit = Physics2D.Raycast(chess.transform.position, chess.transform.right,
            chess.propertyController.GetAttackRange()*2, enemyLayer);
        if (hit.collider != null)
        {
            animator.SetBool("enemy", true);
        }
        else
        {
            animator.SetBool("enemy", false);
        }
    }
    //public override void PlayIdle()
    //{
    //    float mygo = animator.GetFloat("Mygo");
    //    if (mygo < 1)
    //    {
    //        animator.Play("idle1");
    //    }
    //    else if (mygo > 0.9)
    //    {
    //        animator.Play("idle2");
    //    }
    //}
}

