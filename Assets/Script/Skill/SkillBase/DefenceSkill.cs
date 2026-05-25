using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceSkill : SkillBase<SkillConfig_Defence>, ISkillCooldownProgress
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

    public float GetCooldownProgress01()
    {
        if (config == null || config.defenceTime <= 0)
            return 1f;
        return Mathf.Clamp01((float)t / config.defenceTime);
    }

    bool returnCD;
    public override void ReturnCD()
    {
        t=config.defenceTime;
        returnCD = true;
    }

    public override void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(DefenceSkill);
        data.Set("t", t);
    }
    public override void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        t = data.GetInt("t", config.startDefenceTime);
    }
}
