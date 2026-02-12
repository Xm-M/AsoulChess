using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// 被动技能和主动技能
/// </summary>

[Serializable]
public class SkillController:Controller
{
    [HideInInspector]public Chess user;
    [SerializeReference]
    public ISkill passiveSkill;//被动技能同理
    [SerializeReference]
    public ISkill activeSkill;//如果有复合技能 应该在Iskill类中制作一个复合技能而不是在这用list保存
    //[HideInInspector]
    public DamageMessege DM;
    public SkillContext context;
    [HideInInspector]public UnityEvent<Chess> onUseSkill;
    [HideInInspector]public UnityEvent<Chess> onSkillOver;
    public void InitController(Chess c){
        this.user=c;
        passiveSkill?.InitSkill(user);
        activeSkill?.InitSkill(user);
        //DM = new DamageMessege();
        context = new SkillContext();
    }
    public void WhenControllerEnterWar()
    {
        passiveSkill?.UseSkill(user);
        activeSkill?.WhenEnter(user);
        
    }
    public void WhenControllerLeaveWar()
    {
        //DM.damageType=DamageType.Magic;
        passiveSkill?.LeaveSkill(user);
        activeSkill?.LeaveSkill(user);
        onUseSkill.RemoveAllListeners();
        onSkillOver.RemoveAllListeners();
        context.Clear();
    }

    /// <summary>
    /// 这个其实是技能动画播放到释放技能的时候调用的
    /// 主要是播放Skill动画和实际技能使用是两回事 所以要分开讨论
    /// </summary>
    public void UseSkill()
    {
        onUseSkill?.Invoke(user);
        activeSkill?.UseSkill(user);
    }
    /// <summary>
    /// 这个也是用在Transition用来判断是否转换的
    /// </summary>
    /// <returns></returns>
    public bool IfSkillReady()
    {
        if(activeSkill != null)
        {
            return activeSkill.IfSkillReady(user);
        }
        return false;
    }
    public void SkillOver(Chess user)
    {
        activeSkill.SkillOver(user);
        onSkillOver?.Invoke(user);
    }
}
public class SkillContext
{
    // 扩展用的字典（可选）
    Dictionary<string, object> extra;
    public UnityEvent OnValueChange;
    public SkillContext()
    {
        extra = new Dictionary<string, object>();
        OnValueChange=new UnityEvent();
    }
    public void Clear()
    {
        extra.Clear();
        OnValueChange.RemoveAllListeners();
    }
    public void AddEvent(UnityAction unityAction)
    {
        Debug.Log("添加事件");
        OnValueChange.AddListener(unityAction);
    }
    public  void RemoveEvent(UnityAction unityAction)
    {
        OnValueChange.RemoveListener(unityAction);
    }
    public void Set<T>(string key, T value)
    {
        extra ??= new Dictionary<string, object>();
        extra[key] = value;
        OnValueChange?.Invoke();
        //Debug.Log("?");
    }
    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (extra != null && extra.TryGetValue(key, out var obj) && obj is T cast)
        {
            value = cast;
            return true;
        }
        return false;
    }
    public void Remove(string key)
    {
        extra.Remove(key);
        OnValueChange?.Invoke();
    }
}
