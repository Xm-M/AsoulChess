using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkill : ISkill
{
    [SerializeReference]
    public ISkillEffect effect;
    public SkillConfig GetSkillConfig()
    {
        return null;
    }

    public List<Chess> GetTargets()
    {
        throw new System.NotImplementedException();
    }

    public bool IfSkillReady(Chess user)
    {
        return true;
    }

    public virtual void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public bool IsSkillFinished(Chess user)
    {
       return false;
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void ReturnCD()
    {
         
    }

    public void SkillOver(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {
         effect?.SkillEffect(user,null,null);
    }

    public void WhenEnter(Chess user)
    {
    }

    public void WriteToSaveData(SkillStateSaveData data) { }
    public void RestoreFromSaveData(SkillStateSaveData data, Chess user) { }
}
