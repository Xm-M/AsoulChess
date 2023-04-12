using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
[CreateAssetMenu(fileName = "SkillCreator", menuName = "Skill/SkillCreator")]
public class SkillCreator : ScriptableObject
{
    public Skill skill;
    public SkillName skillName;
    SkillName currentName;
    public void CreateSKill()
    {
        if (skillName != currentName)
        {
            skill = Activator.CreateInstance(Type.GetType(skillName.ToString())) as Skill;
            currentName = skillName;
        }
    }
}
public enum SkillName
{
    Skill=0,
    Skill_ElfHeal=1,
}
