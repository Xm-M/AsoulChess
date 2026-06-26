using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Timoris 主动：对技能索敌范围内、体型不大于自身的敌人施加 <see cref="Buff_Fear"/>，
/// 并连续造成 <c>baseDamage[0]</c> 段伤害，每段为 <c>攻击力 × baseDamage[1]</c>。
/// 索敌优先 <see cref="searchOverride"/>，否则与 <see cref="SkillReady_IfTargetInRange"/> 共用配置。
/// </summary>
public class SkillEffect_Timoris : ISkillEffect
{
    [SerializeReference]
    [Tooltip("恐惧 Buff 模板（需在 Inspector 配置 FearEffect 等）")]
    public Buff_Fear fearBuff;

    [SerializeReference]
    [Tooltip("可选；为空则读取 activeSkill.readyChecker 中的 IFindTarget")]
    public IFindTarget searchOverride;

    readonly List<Chess> _enemies = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || config == null || config.baseDamage == null || config.baseDamage.Count < 2)
            return;
        if (fearBuff == null)
            return;

        var skill = user.skillController?.activeSkill;
        MujicaSkillFindTarget.CollectEnemies(user, skill, _enemies, searchOverride);
        if (_enemies.Count == 0)
            return;

        int userSize = user.propertyController.GetSize();
        int hitCount = Mathf.Max(1, Mathf.RoundToInt(config.baseDamage[0]));
        float dmgMul = config.baseDamage[1];
        float atk = user.propertyController.GetAttack();
        float damageEach = atk * dmgMul;

        for (int i = 0; i < _enemies.Count; i++)
        {
            Chess enemy = _enemies[i];
            if (enemy == null || enemy.IfDeath)
                continue;
            if (enemy.propertyController.GetSize() > userSize)
                continue;

            var stateName = user.stateController.currentState.state.stateName;
            if (stateName == StateName.SkillState || stateName == StateName.FeverState)
                enemy.buffController.AddBuff(fearBuff);

            for (int h = 0; h < hitCount; h++)
            {
                if (enemy.IfDeath)
                    break;
                var mes = new DamageMessege(user, enemy, damageEach, DamageType.Magic, ElementType.AOE & ElementType.Cutting);
                user.propertyController.TakeDamage(mes);
            }
        }
    }
}
