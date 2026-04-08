using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火爆辣椒：对<strong>施法者所在行</strong>、<strong>tag 与自身不同</strong>的单位造成伤害（<c>GetAttack() * config.baseDamage[0]</c>）；
/// 伤害来源包含敌方队伍列表，并额外用 <c>Enemy | Unselectable</c> 层在行上检测，使气球等无法选中层单位也能吃到伤害（对该层目标使用 <see cref="DamageType.Real"/>）。
/// 融化该行所有冰格（不分敌我）；不对己方（同 tag）造成伤害。
/// </summary>
[Serializable]
public class SkillEffect_Jalapeno : ISkillEffect
{
    const float RowOverlapRadius = 0.4f;

    [Tooltip("整行火焰/爆炸等表现；生成在该行地图中心（首列与末列 tile 中点）。需在对象池注册")]
    public GameObject rowEffect;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || user.moveController?.standTile == null) return;

        int rowY = user.moveController.standTile.mapPos.y;
        var map = MapManage.instance;
        if (map == null) return;

        SpawnRowVisual(rowEffect, map, rowY, user);

        float mult = 1f;
        if (config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            mult = config.baseDamage[0];
        float damage = user.propertyController.GetAttack() * mult;

        int unselLayer = LayerMask.NameToLayer("Unselectable");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int rowMask = 0;
        if (enemyLayer >= 0) rowMask |= 1 << enemyLayer;
        if (unselLayer >= 0) rowMask |= 1 << unselLayer;

        var hit = new HashSet<Chess>();

        void TryDamage(Chess target)
        {
            if (target == null || target.IfDeath) return;
            if (target.CompareTag(user.tag)) return;
            if (target.moveController?.standTile == null) return;
            if (target.moveController.standTile.mapPos.y != rowY) return;
            if (!hit.Add(target)) return;

            DamageMessege dm = user.skillController.DM;
            dm.damageFrom = user;
            dm.damageTo = target;
            dm.damage = damage;
            dm.damageType = DamageType.Real;
            user.propertyController.TakeDamage(dm);
        }

        var ctm = GameManage.instance != null ? GameManage.instance.chessTeamManage : null;
        if (ctm != null)
        {
            foreach (var chess in ctm.GetEnemyTeam(user.tag))
                TryDamage(chess);
        }

        if (rowMask != 0)
        {
            for (int x = 0; x < map.mapSize.x; x++)
            {
                Tile tile = map.tiles[x, rowY];
                if (tile == null) continue;
                Vector2 p = tile.transform.position;
                var cols = Physics2D.OverlapCircleAll(p, RowOverlapRadius, rowMask);
                for (int i = 0; i < cols.Length; i++)
                {
                    var ch = cols[i].GetComponentInParent<Chess>();
                    TryDamage(ch);
                }
            }
        }

        var snow = Effect_Snow.GetInstanceOrNull();
        if (snow != null)
            snow.MeltIceOnRow(rowY);
    }

    static void SpawnRowVisual(GameObject prefab, MapManage map, int rowY, Chess user)
    {
        if (prefab == null || map == null || ObjectPool.instance == null) return;
        if (!map.IfInMapRange(0, rowY)) return;

        Vector3 pos;
        var t0 = map.tiles[0, rowY];
        var tLast = map.tiles[map.mapSize.x - 1, rowY];
        if (t0 != null && tLast != null)
            pos = (t0.transform.position + tLast.transform.position) * 0.5f;
        else if (user.moveController?.standTile != null)
            pos = user.moveController.standTile.transform.position;
        else
            return;

        GameObject fx = ObjectPool.instance.Create(prefab);
        if (fx != null)
            fx.transform.position = pos;
    }
}
