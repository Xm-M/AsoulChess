using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Timer : ISkill
{
    public float startTime;
    public float coldDown;//CD
    float t;
    public virtual bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            t = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void InitSkill(Chess user)
    {
        t = startTime;

    }

    public virtual void LeaveSkill(Chess user)
    {
        t = 0;
    }



    public virtual void UseSkill(Chess user)
    {
         
    }

    public virtual void WhenEnter(Chess user)
    {
        t = startTime;
    }
}
