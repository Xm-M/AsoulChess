using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill
{
    [HideInInspector]public Chess user;
    [LabelText("���ܶ���")]
    public string AnimationName;//���ܶ�Ӧ�Ķ���
    public float coldDown;//ʵ��cd
    public float startTime;//��ʼcd
    public Timer timer;
    public bool loop = true;//����skillΪfalse
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

