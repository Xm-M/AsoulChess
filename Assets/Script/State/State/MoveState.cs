using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : State
{
    public MoveState(){
        stateName=StateName.MoveState;
    }

    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        chess.moveController.StartMoving();
    }

    public override void Execute(Chess chess)
    {
        base.Execute(chess);
        chess.moveController.WhenMoving();
    }

    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.moveController.EndMoving();
    }
 
    
    public override State Clone()
    {
        MoveState ans= new MoveState();
        ans.stateName=stateName;
        return ans;
    }
}
 
