using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class SkillController:Controller
{
    [HideInInspector]public Chess user;
    public List<Skill> skillList;
    [HideInInspector]public Skill currentSkill;
    List<Skill> prepareSkill;
    public void InitController(Chess c){
        this.user=c;
        prepareSkill=new List<Skill>();
        for(int i = 0; i < skillList.Count; i++)
        {
            skillList[i].user=user;
        }
    }
    public void WhenControllerEnterWar()
    {
        for(int i = 0; i < skillList.Count; i++)
        {
            skillList[i].InitSkill();
            if (skillList[i].loop)
            {
                int n = i;
                skillList[i].timer = GameManage.instance.timerManage.AddTimer(
                    ()=> {AddPrepareSkill(skillList[n]); }, skillList[i].coldDown);
                skillList[i].timer.ResetTime(skillList[i].startTime);
            }
            else
            {
                skillList[i].UseSkill();
            }
        }
    }
    public void AddPrepareSkill(Skill skill)
    {
        prepareSkill.Add(skill);
    }

    public void WhenControllerLeaveWar()
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].WhenSkillLeave();
            if (skillList[i].loop)
            {
                skillList[i].timer.Stop();
            }
        }
    }

    public void UseSkill()
    {
        currentSkill.UseSkill();
    }
    public void LoopSkill()
    {
        prepareSkill.Remove(currentSkill);
        int n = skillList.IndexOf(currentSkill);
        currentSkill.timer= GameManage.instance.timerManage.AddTimer(
                    () => { AddPrepareSkill(skillList[n]); }, currentSkill.coldDown);
        currentSkill = null;
    }

    public bool IfSkillReady()
    {
        for(int i=0;i<prepareSkill.Count; i++)
        {
            if (prepareSkill[i].IfSkillReady())
            {
                currentSkill = prepareSkill[i];
                return true;
            }
        }
        return false;
    }
}
