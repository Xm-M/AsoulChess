using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : State
{
    
    public override void Enter(Chess chess)
    {
        chess.animator.Play("death");
        chess.GetComponent<Collider2D>().enabled = false;
        base.Enter(chess);
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
    }
    public override void Exit(Chess chess)
    {
        chess.GetComponent<Collider2D>().enabled = true;
        base.Exit(chess);
    }
    public override State Clone()
    {
        DeathState deathState = new DeathState();
        deathState.stateName = StateName.DeathState;
        return deathState;
    }
}
