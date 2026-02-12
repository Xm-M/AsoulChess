using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 问题就在于返还cd的问题
/// </summary>
public class AttackSkill : SkillBase<SkillConfig_Attack>
{
    int t;//攻击次数
    
    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        t=config.startAttackTime;
        user.equipWeapon.OnAttack.AddListener(CaculateAttackTime);
    }

    public override bool IfSkillReady(Chess user)
    {
        // 1. 自身触发条件（次数 / CD / 受击数）
        if (t < config.attackTimeCold)
            return false;

        // 2. 交给策略去判断：点击 / 范围内有敌人 / 复合条件 等等
        if (readyChecker == null)
            return true;

        targets.Clear();
        return readyChecker.IfSkillReady(user, config, targets);
    }

    public override void LeaveSkill(Chess user)
    {
        base.LeaveSkill(user);
        user.equipWeapon.OnAttack.RemoveListener(CaculateAttackTime);
    }
    bool returnCd;
    public override void SkillOver(Chess user)
    {
        if (returnCd)
        {
            returnCd = false;
            return;
        }
        t = 0;
    }
    public override void ReturnCD()
    {
        returnCd=true;
        t=config.attackTimeCold;
    }
    public void CaculateAttackTime(Chess chess) => t += 1;
    
}
