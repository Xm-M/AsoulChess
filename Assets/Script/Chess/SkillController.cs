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
    public ISkill passiveSkill;//��������ͬ��
    [SerializeField]
    public ISkill activeSkill;//����и��ϼ��� Ӧ����Iskill��������һ�����ϼ��ܶ�����������list����
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
    /// �����ʵ�Ǽ��ܶ������ŵ��ͷż��ܵ�ʱ����õ�
    /// ��Ҫ�ǲ���Skill������ʵ�ʼ���ʹ���������� ����Ҫ�ֿ�����
    /// </summary>
    public void UseSkill()
    {
        activeSkill?.UseSkill(user);
    }
    /// <summary>
    /// ���Ҳ������Transition�����ж��Ƿ�ת����
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
