using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class SkillController:Controller
{
    [HideInInspector]public Chess user;
    [SerializeField]
    public ISkill passiveSkill;//被动技能同理
    [SerializeField]
    public ISkill activeSkill;//如果有复合技能 应该在Iskill类中制作一个复合技能而不是在这用list保存
    [HideInInspector]
    public DamageMessege DM;

    public void InitController(Chess c){
        this.user=c;
        passiveSkill?.InitSkill(user);
        activeSkill?.InitSkill(user);
        DM = new DamageMessege();
    }
    public void WhenControllerEnterWar()
    {
        passiveSkill?.UseSkill(user);
    }
    public void WhenControllerLeaveWar()
    {
        DM.damageType=DamageType.Magic;
        passiveSkill?.LeaveSkill(user);
        activeSkill?.LeaveSkill(user);
    }

    /// <summary>
    /// 这个其实是技能动画播放到释放技能的时候调用的
    /// 主要是播放Skill动画和实际技能使用是两回事 所以要分开讨论
    /// </summary>
    public void UseSkill()
    {
        activeSkill?.UseSkill(user);
    }
    /// <summary>
    /// 这个也是用在Transition用来判断是否转换的
    /// </summary>
    /// <returns></returns>
    public bool IfSkillReady()
    {
        if(activeSkill != null)
        {
            return activeSkill.IfSkillReady(user);
        }
        return false;
    }
}
