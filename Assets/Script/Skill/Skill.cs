using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill  :ScriptableObject
{
    public int skillid;//不知道有什么用
    public string skillName;//可能有点用，可以在显示面板
    [Multiline]
    public string skillDescription;//可以用在显示面板 
    //public List<string> attackTargetTags;//技能作用对象
    public string AnimationName;//技能对应的动画
    [SerializeReference]
    public ISkillEffect skillEffect;
    [SerializeReference]
    public ISkillOver skillOver;
}

