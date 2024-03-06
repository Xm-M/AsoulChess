using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillState : State
{
    float t;
    public SkillState(){
        stateName=StateName.SkillState;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
        t = 0;
        if (chess.skillController.currentSkill.skillName != "")
            chess.animator.Play(chess.skillController.currentSkill.skillName);
        //chess.skillController.UseSkll();       
    }
    public override void Execute(Chess chess)
    {
        base.Execute(chess);
        
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
    }

    public override State Clone()
    {
        SkillState ans= new SkillState();
        ans.stateName=stateName;
        return ans;
    }
}
