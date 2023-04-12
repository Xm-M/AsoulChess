using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{

    public AttackState(){
        stateName=StateName.AttackState;
    }

    public override void Enter(Chess chess)
    {
        chess.animator.Play("idle");
        base.Enter(chess);
        chess.equipWeapon.Attack();
    }
    public override void Excute(Chess chess)
    {
  
        chess.equipWeapon.Attack();
        base.Excute(chess);
    }
    public override void Exit(Chess chess)
    {
        //chess.equipWeapon.Stand();
        base.Exit(chess);
    }
    public override State Clone()
    {
        AttackState ans=new AttackState();
        ans.stateName=stateName;
        return ans;
    }
}
