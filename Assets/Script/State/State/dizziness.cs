using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 眩晕状态 具体的话应该有个眩晕标志
/// </summary>
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
        //speed = chess.animator.speed;
        //speed = chess.animatorController.animator.speed;
        //chess.animatorController.ChangeSpeed(0);
        //chess.animator.speed = 0;
        chess.propertyController.Freezy();
    }
    public override void Execute(Chess chess)
    {
        
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        //chess.animatorController.ChangeSpeed(speed);
        chess.propertyController.UnFreezy();
    }
    public override State Clone()
    {
        return new DizzinessState();
    }
}
