using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 配置：ScriptableObject
[CreateAssetMenu(menuName = "Skill/SkillConfig")]
public class SkillConfig : ScriptableObject
{
    public SkillType skillType;//技能类型
    public SkillDurationMode skillDurationMode;//技能的释放类型
    public string skillName;//技能名
    public int maxTargetNum;//技能的目标对象最大值(Aoe技能默认100)
    public int minTargetNum;//技能的目标对象最小值(Aoe技能默认为0)
    public bool auto;//技能是否是自动释放
    public List<float> baseDamage;//技能伤害 
}

//技能类型
public enum SkillType
{
    ColdSkill,//正常的冷却型技能
    AttackSkill,//攻回技能(攻击多少次释放技能)
    DefenceSkill,//受击技能(受到多少伤害释放技能)
    DeathSkill,//亡语技能(死亡时候释放的技能)
    PassiveSkill,//被动技能(一般是角色入场时候就会触发的技能 可能是加个buff就好了)
}
public enum SkillDurationMode
{
    ByAnimation,    // 完全由动画控制（瞬发 or 动画播完即结束）
    ByTime,         // 用时间控制（持续技能）
    ByBulletNum,    // 弹药类技能
    ByBuffOver,     // Buff类技能
}

