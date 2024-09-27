 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  interface ISkill  
{
    public void InitSkill(Chess user);//初始化，一般是添加一些东西
    public void UseSkill(Chess user);//使用skill,可能被引用多次
    public void LeaveSkill(Chess user);//结束初始化，主要是清除一些东西
    public bool IfSkillReady(Chess user);//这个是判断函数
}
