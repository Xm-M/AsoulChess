using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill
{
    [HideInInspector]public Chess user;
    [LabelText("技能动画")]
    public string AnimationName;//技能对应的动画
    public float coldDown;//实际cd
    public float startTime;//起始cd
    public Timer timer;
    public bool loop = true;//被动skill为false
    public List<Chess> targets;
    [SerializeReference]
    public ISkill skillEffect;
    public void InitSkill()
    {
        skillEffect.InitSkill(this);
    }
    public bool IfSkillReady()
    {
        return skillEffect.ifSkillReady(this);
    }
    public void WhenSkillLeave()
    {
        skillEffect.LeaveSkill(this);
    }
    public void UseSkill()
    {
        skillEffect.UseSkill(this);
    }
}

