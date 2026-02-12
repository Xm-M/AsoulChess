using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Rupa : ISkillEffect
{
    //public int limit = 50;
    [SerializeReference]
    public DizznessBuff buff;
    public float speed=3;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        DamageMessege DM = user.skillController.DM;
        if(DM.takeBuff==null)
            DM.takeBuff = buff.Clone();
        foreach(var target in targets)
        {
            float dis =Mathf.Max(1, config.baseDamage[1] - target.propertyController.GetSize());
            DM.damage = (user.propertyController.GetAttack() * config.baseDamage[0]) * (dis);
            DM.damageFrom = user;
            DM.damageTo = target;
            user.propertyController.TakeDamage(DM);
            Vector2 targetPos=target.transform.position+user.transform.right*dis;
            target.moveController.MoveToTarget(targetPos,speed);
        }
    }
}
