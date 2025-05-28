using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeState : State
{
    public ResumeState()
    {
        this.stateName = StateName.ResumeState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        chess.animatorController.animator.Play("resume");
        chess.UnSelectable();
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.ResumeSelectable();
    }
    public override State Clone()
    {
        return new ResumeState();
    }
}
public class HealOverTransition : Transition
{
    public override bool ifReach(Chess chess)
    {
        //Debug.Log(chess.propertyController.GetMaxHp());
        return chess.propertyController.GetHpPerCent()>0.999;
    }
    public override Transition Clone()
    {
        return new HealOverTransition();
    }
}
