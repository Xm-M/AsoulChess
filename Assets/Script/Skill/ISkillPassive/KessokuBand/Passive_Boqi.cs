using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被动技能该怎么设计呢 思考....
/// 其实是可以做成buff的对吧....
/// </summary>
public class Passive_Boqi : ISkillEffect
{
    public void OnSetDamage(DamageMessege dm)
    {
        if ((dm.damageType & DamageType.Real) == 0)
        {
            float n = Random.Range(0, 1);
            if (n < dm.damageTo.propertyController.GetCrit())
            {
                //Debug.Log("会心防御！");
                float critDamege = dm.damageTo.propertyController.GetCritDamage();
                dm.damage *= ((critDamege - 1) / (critDamege));
            }
        }
    }
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.propertyController.onSetDamage.AddListener(OnSetDamage);
    }
}
