using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这里放的都是一些生成阳光的技能
/// </summary>

/// <summary>
/// 最基本的生成阳光的技能 就是向日葵(虹夏
/// </summary>
public class SkillEffect_CreateSunLight : ISkillEffect
{
    public Transform sunLightPos;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        lignt.InitSunLight(user.moveController.standTile, (int)config.baseDamage[0], sunLightPos.transform.position);
    }
}

