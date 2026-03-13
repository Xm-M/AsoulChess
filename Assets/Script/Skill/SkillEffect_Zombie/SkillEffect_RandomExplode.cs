using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///// <summary>
///// 重点是有没有这个盒子的问题
///// 爆炸的伤害类型都是在skillcontroller里使用的
///// </summary>
public class SkillEffect_RandomExplode : ISkillEffect
{
    [SerializeReference]
    [LabelText("寻敌方式")]
    public IFindTarget findTarget;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        findTarget.FindTarget(user, targets);
        for (int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
            user.skillController.DM.damageElementType = ElementType.Explode;
            user.skillController.DM.damage =
                user.propertyController.GetAttack() * config.baseDamage[0];
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    }
 
}
