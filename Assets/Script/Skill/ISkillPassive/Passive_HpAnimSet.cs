using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_HpAnimSet : ISkill
{
    public string animName = "HP";
    Animator anim;
    public void InitSkill(Chess user)
    {
        anim = user.animator;
        
    }
    public void WhenTakeDamage(DamageMessege dm)
    {
        float p= dm.damageTo.propertyController.GetHpPerCent();
        anim.SetFloat(animName, p);
        //Debug.Log(" ‹µΩ…À∫¶" + p);
    }
 

    public void UseSkill(Chess user)
    {
        anim.SetFloat(animName, 1);
        user.propertyController.onGetDamage.AddListener(WhenTakeDamage);
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public bool IfSkillReady(Chess user)
    {
        return false;
    }
}
