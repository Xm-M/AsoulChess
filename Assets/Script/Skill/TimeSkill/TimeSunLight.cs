using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class TimeSunLight : ISkillEffect
{
    public GameObject sunLight;
    public void UseSkill(Chess user)
    {
        GameObject a = ObjectPool.instance.Create(sunLight);
        a.transform.position = user.transform.position;
        SunLight lignt = a.GetComponent<SunLight>();
        if (lignt != null)
        {
            lignt.InitSunLight(user.moveController.standTile);
        }
    }
}
