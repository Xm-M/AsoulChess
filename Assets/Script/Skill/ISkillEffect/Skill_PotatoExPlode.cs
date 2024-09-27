using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_PotatoExPlode : ISkill
{
    public float coldDown;//CD
    public string prepareAnim;
    public float explodeRate=10;
    public IFindTarget findTarget;
    List<Chess> targets;
   
    //DamageType
    float t;
    bool explodeOver;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        if(t< user.propertyController.GetColdDown(coldDown)&&explodeOver==false)
            t += Time.deltaTime;
        else if (t > user.propertyController.GetColdDown(coldDown))
        {
            explodeOver = true;
            user.animator.Play(prepareAnim);
            t = 0;
        }
        if (explodeOver)
        {
            findTarget.FindTarget(user, targets);
        }
        return explodeOver&&(targets.Count>0);
    }

    public void InitSkill(Chess user)
    {
        t = 0;
        targets = new List<Chess>();
        //DM = new DamageMessege();
        //DM.damageType = DamageType.Magic;
    }

    public void LeaveSkill(Chess user)
    {
        t = 0;
        targets.Clear();
    }


    /// <summary>
    /// 一般来说 所有的爆炸类植物攻击力为180，技能增幅为10
    /// </summary>
    /// <param name="user"></param>
    public void UseSkill(Chess user)
    {
        findTarget.FindTarget(user, targets);
        for(int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
            user.skillController.DM.damage =
                user.propertyController.GetAttack()*explodeRate;
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    }
}
