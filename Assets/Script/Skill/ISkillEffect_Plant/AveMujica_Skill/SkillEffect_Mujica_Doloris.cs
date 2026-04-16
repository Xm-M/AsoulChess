using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Doloris 主动：为范围内友军施加可配置的 <see cref="Buff_BaseValueBuff_TimeValueBuff"/>；
/// 对攻击范围内随机一名敌人造成伤害（攻击力×<see cref="SkillConfig.baseDamage"/>[0]）；
/// 并治疗一名友军（逻辑同被动：比例最低 / 优先受伤丰川祥子 +25%）。
/// </summary>
public class SkillEffect_Mujica_Doloris : ISkillEffect
{
    [SerializeReference]
    [Tooltip("施加给每名范围内友军的时长 Buff；内层 valueBuff 在 Inspector 自行配置")]
    public Buff_BaseValueBuff_TimeValueBuff allyTimeValueBuff;

    readonly List<Chess> _allyScratch = new List<Chess>();
    readonly List<Chess> _enemyScratch = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || config == null || config.baseDamage == null || config.baseDamage.Count == 0)
            return;
        if (!(user.equipWeapon?.weapon is  Weapon_Sample ws) || !(ws.findTarget is   IGridFindTarget))
            return;
        //user.GetComponent<AudioPlayer>().RandomPlay();
        float coef = config.baseDamage[0];
        IGridFindTarget grid= ws.findTarget as IGridFindTarget;
        MujicaDolorisGrid.CollectAlliesInAttackGrid(user, grid, _allyScratch);
        if (allyTimeValueBuff != null)
        {
            for (int i = 0; i < _allyScratch.Count; i++)
            {
                Chess ally = _allyScratch[i];
                if (ally == null || ally.IfDeath)
                    continue;
                Buff_BaseValueBuff_TimeValueBuff clone = CloneTimeValueBuff(allyTimeValueBuff);
                if (clone != null)
                    ally.buffController.AddBuff(clone);
            }
        }

        ws.findTarget.FindTarget(user, _enemyScratch);
        if (_enemyScratch.Count > 0)
        {
            int idx = Random.Range(0, _enemyScratch.Count);
            Chess enemy = _enemyScratch[idx];
            if (enemy != null && !enemy.IfDeath)
            {
                float dmg = user.propertyController.GetAttack() * coef;
                if (dmg > 0f)
                {
                    var mes = new DamageMessege(user, enemy, dmg, DamageType.Magic, ElementType.None);
                    user.propertyController.TakeDamage(mes);
                }
            }
        }

        if (MujicaDolorisGrid.TryPickHealTarget(user, _allyScratch, out Chess healTarget, out bool sakiBonus))
        {
            float atk = user.propertyController.GetAttack();
            float heal = atk * coef * (sakiBonus ? 1.25f : 1f);
            if (heal > 0f)
                healTarget.propertyController.Heal(heal);
        }
    }

    static Buff_BaseValueBuff_TimeValueBuff CloneTimeValueBuff(Buff_BaseValueBuff_TimeValueBuff src)
    {
        if (src == null)
            return null;
        var c = new Buff_BaseValueBuff_TimeValueBuff
        {
            buffName = src.buffName,
            continueTime = src.continueTime,
            valueBuff = src.valueBuff != null ? src.valueBuff.Clone() : null
        };
        return c;
    }
}
