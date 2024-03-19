 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  interface ISkill  
{
    public void InitSkill(Skill chess);//初始化，一般是添加一些东西
    public void UseSkill(Skill user);//使用skill,可能被引用多次
    public void LeaveSkill(Skill user);//结束初始化，主要是清楚一些东西
    public bool ifSkillReady(Skill user);//这个是判断函数
}
