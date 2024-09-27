using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_CreateSweet : ISkill
{
    float CreateNum;
    public float coldDown;//CD
    float t;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        t += Time.deltaTime;
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
        t = 0;
    }

    public void LeaveSkill(Chess user)
    {
        t = 0;
    }



    public void UseSkill(Chess user)
    {
        PowerBarPanel.GetView<SweetBar>().UpdateBar(CreateNum);
    }
     
}
