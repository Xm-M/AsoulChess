using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Timoris 主动：以武器 <see cref="IGridFindTarget"/> 为范围，对体型不大于自身的敌人施加 <see cref="Buff_Fear"/>，
/// 并连续造成 <c>baseDamage[0]</c> 段伤害，每段为 <c>攻击力 × baseDamage[1]</c>。
/// </summary>
public class SkillEffect_Timoris : ISkillEffect
{
    [SerializeReference]
    [Tooltip("恐惧 Buff 模板（需在 Inspector 配置 FearEffect 等）")]
    public Buff_Fear fearBuff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || config == null || config.baseDamage == null || config.baseDamage.Count < 2)
            return;
        if (fearBuff == null)
            return;

 

        int userSize = user.propertyController.GetSize();
        int hitCount = Mathf.Max(1, Mathf.RoundToInt(config.baseDamage[0]));
        float dmgMul = config.baseDamage[1];
        float atk = user.propertyController.GetAttack();
        float damageEach = atk * dmgMul;

        for (int i = 0; i < targets.Count; i++)
        {
            Chess enemy = targets[i];
            if (enemy == null || enemy.IfDeath)
                continue;
            if (enemy.propertyController.GetSize() > userSize)
                continue;
            if(user.stateController.currentState.state.stateName==StateName.SkillState)
                enemy.buffController.AddBuff(fearBuff);

            for (int h = 0; h < hitCount; h++)
            {
                if (enemy.IfDeath)
                    break;
                var mes = new DamageMessege(user, enemy, damageEach, DamageType.Magic, ElementType.AOE&ElementType.Cutting);
                user.propertyController.TakeDamage(mes);
            }
        }
    }
}
