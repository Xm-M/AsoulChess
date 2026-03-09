 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public  interface ISkill
{
    public void InitSkill(Chess user);//初始化，一般是添加一些东西
    public void UseSkill(Chess user);//使用skill,可能被引用多次
    public void LeaveSkill(Chess user);//结束初始化，主要是清除一些东西
    public bool IfSkillReady(Chess user);//这个是判断函数

    public void WhenEnter(Chess user);//进入战斗初始化
    public bool IsSkillFinished(Chess user);
    public void SkillOver(Chess user);
    public SkillConfig GetSkillConfig();
    public void ReturnCD();
}
public interface ISkillEffect
{
    public void SkillEffect(Chess user,SkillConfig config,List<Chess> targets);
}