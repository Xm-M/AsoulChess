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
    protected Chess user;
    int n;
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
        //checkReplace.WhenLeave(user, this);
        //Debug.Log(n);
        OnSkillOver?.Invoke(user);
        currentSkill.SkillOver(user);
        //Debug.Log(n);
        //技能转换这件事 必须要在结束的时候调用
        currentSkill = replaces[n];
    }

    public void UseSkill(Chess user)
    {
        currentSkill = replaces[n];
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

    /// <summary>
    /// 如果是技能中就等待技能结束的时候切换 如果不是技能中就直接切换
    /// </summary>
    /// <param name="n"></param>
    public void ChangeSkill(int n)
    {
        this.n = n;
        if (user.stateController.currentState.state.stateName != StateName.SkillState)
        {
            Debug.Log("替换");
            currentSkill = replaces[n];
        }
    }
}
public  interface ICheckReplace
{
    public void WhenEnter(Chess user,ReplaceSkill replaceSkill);

    public void WhenLeave(Chess user,ReplaceSkill replaceSkill);

}