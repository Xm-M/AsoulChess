 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  interface ISkill  
{
    public void InitSkill(Skill chess);//��ʼ����һ�������һЩ����
    public void UseSkill(Skill user);//ʹ��skill,���ܱ����ö��
    public void LeaveSkill(Skill user);//������ʼ������Ҫ�����һЩ����
    public bool ifSkillReady(Skill user);//������жϺ���
}
