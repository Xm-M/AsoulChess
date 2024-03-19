using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_CreateSunLight : ISkillEffect
{
    public GameObject sunLight;
    public int sunLightNum = 25;
    public void SkillEffect(Skill skill)
    {
        GameObject a = ObjectPool.instance.Create(sunLight);
        a.transform.position = skill.user.transform.position;
        SunLight lignt = a.GetComponent<SunLight>();
        if (lignt != null)
        {
            lignt.InitSunLight(skill.user.moveController.standTile,sunLightNum);
        }
    }
}
