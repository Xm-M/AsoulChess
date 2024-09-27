using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Summon : ISkill
{
    public PropertyCreator creator;

    public bool IfSkillReady(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void InitSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void LeaveSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }

    public void SkillEffect(Chess user)
    {
        ChessTeamManage.Instance.CreateChess(creator,  user.moveController.standTile,
             user.tag);
    }

    public void UseSkill(Chess user)
    {
        throw new System.NotImplementedException();
    }
}
