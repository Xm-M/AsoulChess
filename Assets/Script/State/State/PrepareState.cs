using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PrepareState : State
{
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        chess.animatorController.PlayIdle();
    }
    public PrepareState(){
        stateName=StateName.PrepareState;
    }
    public override State Clone()
    {
        PrepareState ans= new PrepareState();
        ans.stateName=stateName;
        return ans;
    }
}
