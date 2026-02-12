using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消耗品技能 虽然说这个技能效果应该绑定在末尾啊 但是实际上在这调用也是可以的
/// </summary>
public class ConsumablesSkill : SkillBase<SkillConfig>
{
    public override bool IfSkillReady(Chess user)
    {
        return true;
    }
    public override void WhenEnter(Chess user)
    {
        user.UnSelectable();
        base.WhenEnter(user);
    }
    /// <summary>
    /// 消耗品用完就回收啦~
    /// </summary>
    /// <param name="user"></param>
    public override void SkillOver(Chess user)
    {
        user.Death();
    }

    public override void ReturnCD()
    {
        throw new System.NotImplementedException();
    }
}
