using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
    public DeathState()
    {
        stateName = StateName.DeathState;
    }
    public override void Enter(Chess chess)
    {
        //chess.animator.Play("death");
        //亡语是在死亡状态触发的，所以没有进入死亡状态就不会触发
        chess.animatorController.PlayDeath();
        chess.IfDeath = true;
        chess.DeathEvent?.Invoke(chess);
        chess.SetCol(false);
        base.Enter(chess);
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
    }
    public override void Exit(Chess chess)
    {
        chess.SetCol(true);
        base.Exit(chess);
    }
    public override State Clone()
    {
        DeathState deathState = new DeathState();
        deathState.stateName = StateName.DeathState;
        return deathState;
    }
}
