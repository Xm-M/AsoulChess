using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 冰爆辣椒：与火爆辣椒相同的整行 Real 伤害与行判定；额外对同行敌方施加 <see cref="FreezyBuff"/>。
/// 不融冰、不铺冰、不清除僵王冰球；清除同行 <see cref="ZombieKingFireBall"/>。
/// </summary>
[Serializable]
public class SkillEffect_IceJalapeno : ISkillEffect
{
    [Tooltip("整行冰爆表现；生成在该行地图中心。需在对象池注册")]
    public GameObject rowEffect;

    [SerializeReference]
    [LabelText("冻结 Buff")]
    public FreezyBuff freezeBuff;

    [LabelText("冻结时长（秒）")]
    [Tooltip("≤0 时若 config.baseDamage[1] 存在则用其作为秒数，否则 3")]
    public float freezeDuration = 3f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (!JalapenoRowSkill.TryGetUserRow(user, out int rowY, out MapManage map))
            return;

        JalapenoRowSkill.SpawnRowVisual(rowEffect, map, rowY, user);

        float damage = JalapenoRowSkill.ResolveDamage(user, config);
        float freezeSec = ResolveFreezeDuration(config);
        var hit = JalapenoRowSkill.CollectEnemiesOnRow(user, rowY, map);

        foreach (var target in hit)
        {
            JalapenoRowSkill.ApplyRealDamage(user, target, damage);
            if (target == null || target.IfDeath || freezeBuff == null)
                continue;

            freezeBuff.continueTime = freezeSec;
            target.buffController.AddBuff(freezeBuff);
        }

        JalapenoRowSkill.ClearZombieKingFireBallsOnRow(rowY);
    }

    float ResolveFreezeDuration(SkillConfig config)
    {
        if (freezeDuration > 0f)
            return freezeDuration;
        if (config?.baseDamage != null && config.baseDamage.Count > 1)
            return config.baseDamage[1];
        return 3f;
    }
}
