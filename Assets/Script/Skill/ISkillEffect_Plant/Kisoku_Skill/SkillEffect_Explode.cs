using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 9уееф  
/// </summary>
public class SkillEffect_KitaExplode : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.UnSelectable();
        for (int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
 
            user.skillController.DM.damage =
                user.propertyController.GetAttack() * config.baseDamage[0];
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    }
}
