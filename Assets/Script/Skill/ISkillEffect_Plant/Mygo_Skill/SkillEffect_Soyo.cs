using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 或者说soyo这里只需要判断CD是否结束 那不是一样的啊
/// </summary>
public class SkillEffect_Soyo : ISkill
{
    //public float startTime;
    public float coldDown;//CD
    public MouseDownSkill ifMouseDown;
    [SerializeReference]
    [LabelText("寻敌方式")]
    public IFindTarget findTarget;
    [LabelText("爆炸伤害倍率")]
    public float explodeRate;
    List<Chess> targets;
    float t;
    Timer timer;
    Chess user;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;

        if (t > user.propertyController.GetColdDown(coldDown)&&user.animatorController.animator.GetFloat("Mygo")>0)
        {
            //user.animatorController.ChangeFlash(-1);
            if (ifMouseDown.IfDown)
            {
                t = 0;
                //user.animatorController.ChangeFlash(1);
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
        this.user = user;
    }

    public void LeaveSkill(Chess user)
    {
        if (timer != null)
        {
            timer.Stop();
            timer = null;
        }
    }

    public void UseSkill(Chess user)
    {
        findTarget.FindTarget(user, targets);
        for (int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
            user.skillController.DM.damageElementType = ElementType.Explode;
            user.skillController.DM.damage =
                user.propertyController.GetAttack() * explodeRate;
            user.propertyController.TakeDamage(user.skillController.DM);
        }

    }
    public void ChangeState()
    {
        //ObjectPool.instance.ReycleObject(black);
        user.stateController.ChangeState(StateName.IdleState);
        user.animatorController.ChangeFlash(0);
    }

    public void WhenEnter(Chess user)
    {
        t = 0;
        targets = new List<Chess>();
        //user.animatorController.ChangeFlash(0);
        //SkillEffect_Anon
    }
}
