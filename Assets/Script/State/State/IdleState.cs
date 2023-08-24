using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public IdleState()
    {
        stateName=StateName.IdleState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        chess.animator.Play("idle");
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
    }
    public override State Clone()
    {
        IdleState ans = new IdleState();
        ans.stateName = stateName;
        return ans;
    }

    

}
