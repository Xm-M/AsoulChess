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