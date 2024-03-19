using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDownJudge : ISkillJudge
{
    public MouseDownSkill mouse;
    public bool IfSkillOver(Skill skill)
    {
        return mouse.IfDown;
    }
}
