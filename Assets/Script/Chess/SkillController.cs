using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class SkillController:Controller
{
    public Chess user;
    [SerializeReference]
    public PassiveSkill passive;
    [SerializeReference]
    public AttackSkill attackSkill;
    [SerializeReference]
    public HurtSkill hurtSkill;
    [SerializeReference]
    public TimeSkill timeSkill;
    public UnityEvent<Chess> OnUseSkill;
    public bool IfUseSkill{get;private set;}
    float damagePool;
    float damageTimes;
    Timer timer;
    public Skill currentSkill;

    public void InitController(Chess c){
        this.user=c;
    }
    public void WhenControllerEnterWar()
    {
        if (timeSkill != null)
        {
            timer= GameManage.instance.timerManage.AddTimer(UseTimeSkill, timeSkill.coldDown ,true);
        }
        if (hurtSkill != null)
        {
            user.propertyController.onGetDamage.AddListener(WhenGetDamage);
        }
        if (attackSkill != null)
        {
            user.equipWeapon.OnWeaponAttack.AddListener(WhenTakeDamage);
        }
        passive?.skillEffect.UseSkill(user);
    }

    public void WhenControllerLeaveWar()
    {
        if (timeSkill != null)
        {    
            timer.Stop();
        }
    }

    public void UseSkll()
    {
        currentSkill.skillEffect.UseSkill(user);
        OnUseSkill?.Invoke(user);
    }
    public void SkillOver()
    {
        IfUseSkill = false;
    }
    public void UseTimeSkill()
    {
        IfUseSkill = true;
        currentSkill = timeSkill;
    }
     
    public void WhenGetDamage(DamageMessege dm){
        damagePool+=dm.damage;
        if(damagePool>hurtSkill.hurtDamage){
            IfUseSkill=true;
            currentSkill=hurtSkill;
            damagePool = 0;
        }
    }
    public void WhenTakeDamage(Weapon weapon){
        damageTimes += 1;
         
        if (damageTimes > attackSkill.maxAttackNum)
        {
             
            damageTimes = 0;
            currentSkill = attackSkill;
            IfUseSkill = true;
        }
    }

    
}
