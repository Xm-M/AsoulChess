using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillState : State
{
    public SkillState(){
        stateName=StateName.SkillState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        chess.skillController.skillEffectFiredThisCast = false;
        chess.animatorController.PlaySkill();    
    }
    public override void Execute(Chess chess)
    {
        base .Execute(chess);
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
        chess.skillController.SkillOver(chess);
    }
    public override State Clone()
    {
        SkillState ans= new SkillState();
        ans.stateName=stateName;
        return ans;
    }
}
