using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Pingun : ISkill
{
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return false;
    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {
         user.stateController.ChangeState(StateName.MoveState);
        user.moveController.standTile.ChessLeave(user);
    }

    public void WhenEnter(Chess user)
    {
         
    }
}
