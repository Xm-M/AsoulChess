using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 祐天寺若麦主动：对技能索敌范围内敌人造成 攻击力×<see cref="SkillConfig.baseDamage"/>[0] 伤害。
/// 索敌优先 <see cref="searchOverride"/>，否则与 <see cref="SkillReady_IfTargetInRange"/> 共用配置。
/// </summary>
public class SkillEffect_Mujica_Nyamu : ISkillEffect
{
    [SerializeReference]
    [Tooltip("可选；为空则读取 activeSkill.readyChecker 中的 IFindTarget")]
    public IFindTarget searchOverride;

    readonly List<Chess> _enemies = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || config?.baseDamage == null || config.baseDamage.Count == 0)
            return;

        var skill = user.skillController?.activeSkill;
        MujicaSkillFindTarget.CollectEnemies(user, skill, _enemies, searchOverride);
        if (_enemies.Count == 0)
            return;

        float damage = user.propertyController.GetAttack() * config.baseDamage[0];
        for (int i = 0; i < _enemies.Count; i++)
        {
            Chess enemy = _enemies[i];
            if (enemy == null || enemy.IfDeath)
                continue;

            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = enemy;
            user.skillController.DM.damage = damage;
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    }
}
