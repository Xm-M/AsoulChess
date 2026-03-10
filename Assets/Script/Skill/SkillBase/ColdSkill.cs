using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColdSkill : SkillBase<SkillConfig_Cold>
{
    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    float t;
    public override void InitSkill(Chess user)
    {
        base.InitSkill(user);
        t = 0;
    }
    public override void WhenEnter(Chess user)
    {
        base.WhenEnter(user);
        t = config.startCd;
    }
    public override bool IfSkillReady(Chess user)
    {
        t+=Time.deltaTime;
        // 1. 自身触发条件（次数 / CD / 受击数）
        if (t < config.baseCd)
            return false;

        // 2. 交给策略去判断：点击 / 范围内有敌人 / 复合条件 等等
        if (readyChecker == null)
            return true;

        targets.Clear();
        return readyChecker.IfSkillReady(user, config, targets);

    }
    public override bool IsSkillFinished(Chess user)
    {
        return base.IsSkillFinished(user);
    }
    public override void SkillOver(Chess user)
    {
        if (returnCD)
        {
            returnCD = false;
            return;
        }
        t = 0;
        // 
    }
    bool returnCD=false;    
    public override void ReturnCD()
    {
        t=config.baseCd;
        returnCD=true;
    }

    public override void WriteToSaveData(SkillStateSaveData data)
    {
        if (data == null) return;
        data.skillType = nameof(ColdSkill);
        data.Set("t", t);
    }
    public override void RestoreFromSaveData(SkillStateSaveData data, Chess user)
    {
        if (data == null) return;
        t = data.GetFloat("t", config.startCd);
    }
}
