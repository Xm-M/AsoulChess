using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SkillEffect_CreateSunLight : ISkill
{
    public float coldDown;//CD
    public int sunLightNum = 25;
    float t;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        t+=Time.deltaTime;
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
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        Debug.Log(lignt.gameObject.name);
        lignt.InitSunLight( user.moveController.standTile, sunLightNum);
    }
}
