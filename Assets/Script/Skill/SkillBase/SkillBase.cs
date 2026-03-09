using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SkillBase<TConfig> : ISkill
    where TConfig : SkillConfig
{
    public TConfig config;
    [SerializeReference]
    public ISkillReady readyChecker;
    [SerializeReference]
    public ISkillEffect effect;
    [SerializeReference]
    public ISkillFinish skillFinish;
    [SerializeReference]
    public SkillRuntimeInfo runtime;
    protected List<Chess> targets;

    public abstract bool IfSkillReady(Chess user);
    public virtual void UseSkill(Chess user)
    {
        effect.SkillEffect(user, config, targets);
    }
    public virtual void InitSkill(Chess user) { 
        targets = new List<Chess>();

    }
    public virtual void LeaveSkill(Chess user) {
         
       
    }
    public virtual void WhenEnter(Chess user) {
        runtime?.Clear(user);
        readyChecker?.InitSkillReady(user,config,targets);
    }


    public virtual SkillConfig GetSkillConfig()
    {
        return config;
    }

    public virtual bool IsSkillFinished(Chess user)
    {
        if (skillFinish == null) return true;
        if (runtime == null) return true; // 或者直接 false，看你需求
        return skillFinish.IsFinished(user, config, runtime);
    }

    public abstract void SkillOver(Chess user);
    public abstract void ReturnCD();//返还cd
}
