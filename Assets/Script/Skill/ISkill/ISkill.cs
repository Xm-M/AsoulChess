 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  interface ISkill  
{
    public void InitSkill(Chess user);//��ʼ����һ�������һЩ����
    public void UseSkill(Chess user);//ʹ��skill,���ܱ����ö��
    public void LeaveSkill(Chess user);//������ʼ������Ҫ�����һЩ����
    public bool IfSkillReady(Chess user);//������жϺ���
}
