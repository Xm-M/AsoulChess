using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill  :ScriptableObject
{
    public int skillid;//��֪����ʲô��
    public string skillName;//�����е��ã���������ʾ���
    [Multiline]
    public string skillDescription;//����������ʾ��� 
    //public List<string> attackTargetTags;//�������ö���
    public string AnimationName;//���ܶ�Ӧ�Ķ���
    [SerializeReference]
    public ISkillEffect skillEffect;
    [SerializeReference]
    public ISkillOver skillOver;
}

