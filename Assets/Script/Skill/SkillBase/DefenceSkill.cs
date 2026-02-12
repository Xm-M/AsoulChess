using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceSkill : SkillBase<SkillConfig_Defence>
{
    int t;//受击次数

    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        t = config.startDefenceTime;
        user.propertyController.onGetDamage.AddListener(CaculateDefenceTime);
    }

    public override bool IfSkillReady(Chess user)
    {
        // 1. 自身触发条件（次数 / CD / 受击数）
        if (t < config.defenceTime)
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
        user.propertyController.onGetDamage.RemoveListener(CaculateDefenceTime);
    }
    public override void SkillOver(Chess user)
    {
        if (returnCD)
        {
            returnCD = false;
            return;
        }
        t = 0;
    }
    public void CaculateDefenceTime(DamageMessege chess) => t += 1;
    bool returnCD;
    public override void ReturnCD()
    {
        t=config.defenceTime;
        returnCD = true;
    }
}
