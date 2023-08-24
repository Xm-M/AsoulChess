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
        base.Enter(chess);
        chess.equipWeapon.Attack();
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
        chess.equipWeapon.WeaponUpdate();
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
    }
    public override State Clone()
    {
        AttackState ans=new AttackState();
        ans.stateName=stateName;
        return ans;
    }
}
