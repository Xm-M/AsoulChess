 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IHasRuntimeInfo { SkillRuntimeInfo Runtime { get; } }
public interface ISkill
{
    void InitSkill(Chess user);
    void UseSkill(Chess user);
    void LeaveSkill(Chess user);
    bool IfSkillReady(Chess user);
    void WhenEnter(Chess user);
    bool IsSkillFinished(Chess user);
    void SkillOver(Chess user);
    SkillConfig GetSkillConfig();
    void ReturnCD();
    void WriteToSaveData(SkillStateSaveData data);
    void RestoreFromSaveData(SkillStateSaveData data, Chess user);
}
public interface ISkillEffect
{
    public void SkillEffect(Chess user,SkillConfig config,List<Chess> targets);
}

/// <summary>
/// 主动技能「自身条件」进度（不含 <see cref="ISkillReady"/> 如点击/索敌），用于世界空间冷却图标 <see cref="SkillColdFXPresenter"/>。
/// </summary>
public interface ISkillCooldownProgress
{
    /// <summary>0~1：1 表示已达到与 <see cref="ISkill.IfSkillReady"/> 中第一段判断等价的进度（仍可能因 readyChecker 未通过而无法释放）。</summary>
    float GetCooldownProgress01();
}