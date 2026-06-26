using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 所以说replaceSkill应该是有一个List<Skill>
/// 然后要有一个切换方案
/// 那么这个切换方案我应该放在哪呢 update里吗 但是有的切换方案是按技能的释放 有的是随机 还有的是按m3那种
/// 
/// </summary>
public class ReplaceSkill : ISkill
{
    [SerializeReference]
    public ICheckReplace checkReplace;
    [SerializeReference]
    public List<ISkill> replaces;
    
    protected ISkill currentSkill;//当前技能
    /// <summary>当前子技能（用于 UI 冷却条等）。</summary>
    public ISkill CurrentSubSkill => currentSkill;

    protected Chess user;
    int n;
    /// <summary>本次施法锁定的子技能索引；<see cref="UseSkill"/> 多帧触发时勿因 <see cref="n"/> 变化切技能。</summary>
    int _castSkillIndex = -1;

    /// <summary>当前 Replace 子技能索引（与 <see cref="ChangeSkill"/> 同步）。</summary>
    public int CurrentSkillIndex => n;
    [HideInInspector]
    public UnityEvent<Chess> CheckReady, OnUseSKill, OnSkillOver;
    public SkillConfig GetSkillConfig()
    {
        return currentSkill.GetSkillConfig();
    }

    public bool IfSkillReady(Chess user)
    {
        CheckReady?.Invoke(user);
        return currentSkill.IfSkillReady(user);
    }

    public void InitSkill(Chess user)
    {
        this.user = user;   
        foreach(var replace in replaces)
        {
            replace.InitSkill(user);
        }
    }

    public bool IsSkillFinished(Chess user)
    {
         return currentSkill.IsSkillFinished(user);
    }
    /// <summary>
    /// leave一般是结束某些效果 因为多个技能可能都产生了效果 所以都要调用一次leave
    /// </summary>
    /// <param name="user"></param>
    public void LeaveSkill(Chess user)
    {
        _castSkillIndex = -1;
        checkReplace.WhenLeave(user, this);
        foreach(var replace in replaces)
        {
            replace.LeaveSkill(user);
        }
        CheckReady.RemoveAllListeners();
        OnUseSKill.RemoveAllListeners();
        OnSkillOver.RemoveAllListeners();
    }

    public void SkillOver(Chess user)
    {
        OnSkillOver?.Invoke(user);
        currentSkill.SkillOver(user);
        _castSkillIndex = -1;
        currentSkill = replaces[n];
    }

    public void UseSkill(Chess user)
    {
        if (_castSkillIndex < 0)
            _castSkillIndex = n;
        currentSkill = replaces[_castSkillIndex];
        OnUseSKill?.Invoke(user);
        currentSkill.UseSkill(user);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    public void WhenEnter(Chess user)
    {
        checkReplace.WhenEnter(user, this);
        foreach(var replec in replaces)
        {
            replec.WhenEnter(user);
        }
        currentSkill = replaces[0];
        n = 0;
    }
    public void ReturnCD()
    {
        currentSkill.ReturnCD();
    }

    public void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(ReplaceSkill);
        data.Set("n", n);
    }
    public void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        n = data.GetInt("n", 0);
        if (n >= 0 && n < replaces?.Count) currentSkill = replaces[n];
    }

    /// <summary>
    /// 如果是技能中就等待技能结束的时候切换 如果不是技能中就直接切换
    /// </summary>
    /// <param name="n"></param>
    public void ChangeSkill(int n)
    {
        this.n = n;
        if (_castSkillIndex < 0
            && user.stateController.currentState.state.stateName != StateName.SkillState)
        {
            currentSkill = replaces[n];
        }
    }
}
public  interface ICheckReplace
{
    public void WhenEnter(Chess user,ReplaceSkill replaceSkill);

    public void WhenLeave(Chess user,ReplaceSkill replaceSkill);

}