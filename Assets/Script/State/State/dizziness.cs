using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzinessState : State
{
    float speed;
    public DizzinessState()
    {
        stateName=StateName.DizzyState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        speed = chess.animator.speed;
        chess.animator.speed = 0;
    }
    public override void Execute(Chess chess)
    {
        
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.animator.speed = speed;
    }
    public override State Clone()
    {
        return new DizzinessState();
    }
}
