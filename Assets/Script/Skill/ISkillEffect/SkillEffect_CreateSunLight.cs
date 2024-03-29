using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_CreateSunLight : ISkillEffect
{
    //public GameObject sunLight;
    public int sunLightNum = 25;
    public void SkillEffect(Skill skill)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        if (lignt != null)
        {
            lignt.InitSunLight(skill.user.moveController.standTile,sunLightNum);
        }
    }
}
