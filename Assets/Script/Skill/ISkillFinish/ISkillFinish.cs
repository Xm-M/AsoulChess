using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime);
}

public class DurationFinish : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        SkillRuntimeInfo_Duration  info=runtime as SkillRuntimeInfo_Duration;
        if (info.currentTime < info.maxTime)
        {
            info.currentTime += Time.deltaTime;
            return false;
        }
        else  
        {
            info.currentTime = 0;
            return true;
        }
    }
}
public class AttackTimeFinish : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        SkillRuntimeInfo_AttackTime info=runtime as SkillRuntimeInfo_AttackTime;
        if (info.currentAttackTime < info.maxAttackTime)
        {
            return false;
        }
        else
        {
            info.currentAttackTime = 0;
            return true;
        }
    }
}
public class AnimFinish : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        //string animName = user.animatorController.GetCurrentAnimName();
        //if(!animName.Contains("skill"))return false;
        return user.animatorController.IfAnimPlayOver();
    }
}
public class MultiFinish : ISkillFinish
{
    public List<ISkillFinish> finishers;

    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        //if (runtime is not SkillRuntimeInfo_Multy multiRuntime) return true;
        SkillRuntimeInfo_Multy multiRuntime=runtime as SkillRuntimeInfo_Multy;
        // 举例：所有子条件都结束才算结束（AND）
        for (int i = 0; i < finishers.Count && i < multiRuntime.infos.Count; i++)
        {
            if (!finishers[i].IsFinished(user, config, multiRuntime.infos[i]))
                return false;
        }
        return true;
    }
}
public class BuffOverFinish : ISkillFinish
{
    public bool IsFinished(Chess user, SkillConfig config, SkillRuntimeInfo runtime)
    {
        SkillRuntimeInfo_WaitBuffOver buffover=runtime as SkillRuntimeInfo_WaitBuffOver;
        return !user.buffController.buffDic.ContainsKey(buffover.buffName);
         
    }
}


public abstract class SkillRuntimeInfo {
    public abstract void Clear(Chess user);
}

public class SkillRuntimeInfo_Duration:SkillRuntimeInfo
{
    public float currentTime;
    public float maxTime;
    public override void Clear(Chess user)
    {
        currentTime = 0;
    }
    //public Timer timer;
}
public class SkillRuntimeInfo_AttackTime : SkillRuntimeInfo
{
    public int currentAttackTime;
    public int maxAttackTime;
    public override void Clear(Chess user)
    {
        currentAttackTime = 0;
        user.skillController.onUseSkill.AddListener(Count);
    }
    public void Count(Chess user)
    {
        currentAttackTime++;
    }
}
public class SkillRuntimeInfo_Anim : SkillRuntimeInfo
{
    public float currentTime;
    public override void Clear(Chess user)
    {
        currentTime = 0;
    }
}
public class SkillRuntimeInfo_Multy:SkillRuntimeInfo
{
    public List<SkillRuntimeInfo> infos;
    public override void Clear(Chess user)
    {
        foreach (var info in infos) info.Clear(user);
    }
}

public class SkillRuntimeInfo_WaitBuffOver : SkillRuntimeInfo
{
    public string buffName;
    //public Buff targetBuff;
    public override void Clear(Chess user)
    {
       
    }
}