using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火爆辣椒：对<strong>施法者所在行</strong>、<strong>tag 与自身不同</strong>的单位造成伤害（<c>GetAttack() * config.baseDamage[0]</c>）；
/// 伤害来源包含敌方队伍列表，并额外用 <c>Enemy | Unselectable</c> 层在行上检测，使气球等无法选中层单位也能吃到伤害（对该层目标使用 <see cref="DamageType.Real"/>）。
/// 大体型单位（如僵王）若碰撞体覆盖该行也会命中；同行 <see cref="ZombieKingIceBall"/> 会被清除。
/// 融化该行所有冰格（不分敌我）；不对己方（同 tag）造成伤害。
/// </summary>
[Serializable]
public class SkillEffect_Jalapeno : ISkillEffect
{
    [Tooltip("整行火焰/爆炸等表现；生成在该行地图中心（首列与末列 tile 中点）。需在对象池注册")]
    public GameObject rowEffect;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (!JalapenoRowSkill.TryGetUserRow(user, out int rowY, out MapManage map))
            return;

        JalapenoRowSkill.SpawnRowVisual(rowEffect, map, rowY, user);

        float damage = JalapenoRowSkill.ResolveDamage(user, config);
        foreach (var target in JalapenoRowSkill.CollectEnemiesOnRow(user, rowY, map))
            JalapenoRowSkill.ApplyRealDamage(user, target, damage);

        JalapenoRowSkill.ClearZombieKingIceBallsOnRow(rowY);
        JalapenoRowSkill.MeltIceOnRow(rowY);
    }
}
