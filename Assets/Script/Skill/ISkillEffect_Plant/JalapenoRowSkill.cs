using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火爆辣椒 / 冰爆辣椒共用的「施法者所在行」敌方收集与伤害。
/// </summary>
public static class JalapenoRowSkill
{
    public const float RowOverlapRadius = 0.4f;

    public static bool TryGetUserRow(Chess user, out int rowY, out MapManage map)
    {
        rowY = 0;
        map = MapManage.instance;
        if (user?.moveController?.standTile == null || map == null)
            return false;
        rowY = user.moveController.standTile.mapPos.y;
        return true;
    }

    public static float ResolveDamage(Chess user, SkillConfig config)
    {
        float mult = 1f;
        if (config?.baseDamage != null && config.baseDamage.Count > 0)
            mult = config.baseDamage[0];
        return user.propertyController.GetAttack() * mult;
    }

    public static HashSet<Chess> CollectEnemiesOnRow(Chess user, int rowY, MapManage map)
    {
        var hit = new HashSet<Chess>();
        if (user == null || map == null)
            return hit;

        int unselLayer = LayerMask.NameToLayer("Unselectable");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int rowMask = 0;
        if (enemyLayer >= 0) rowMask |= 1 << enemyLayer;
        if (unselLayer >= 0) rowMask |= 1 << unselLayer;

        void TryAdd(Chess target)
        {
            if (target == null || target.IfDeath) return;
            if (target.CompareTag(user.tag)) return;
            if (!IsChessOnMapRow(target, rowY, map)) return;
            hit.Add(target);
        }

        var ctm = GameManage.instance?.chessTeamManage;
        if (ctm != null)
        {
            foreach (var chess in ctm.GetEnemyTeam(user.tag))
                TryAdd(chess);
        }

        if (rowMask != 0)
        {
            for (int x = 0; x < map.mapSize.x; x++)
            {
                Tile tile = map.tiles[x, rowY];
                if (tile == null) continue;
                var cols = Physics2D.OverlapCircleAll(tile.transform.position, RowOverlapRadius, rowMask);
                for (int i = 0; i < cols.Length; i++)
                    TryAdd(cols[i].GetComponentInParent<Chess>());
            }
        }

        return hit;
    }

    public static void ApplyRealDamage(Chess user, Chess target, float damage)
    {
        if (user == null || target == null || target.IfDeath)
            return;

        DamageMessege dm = user.skillController.DM;
        dm.damageFrom = user;
        dm.damageTo = target;
        dm.damage = damage;
        dm.damageType = DamageType.Real;
        user.propertyController.TakeDamage(dm);
    }

    public static void SpawnRowVisual(GameObject prefab, MapManage map, int rowY, Chess user)
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

    public static void ClearZombieKingIceBallsOnRow(int rowY)
    {
        var balls = Object.FindObjectsOfType<ZombieKingIceBall>();
        for (int i = 0; i < balls.Length; i++)
        {
            var ball = balls[i];
            if (ball != null && ball.IsOnMapRow(rowY))
                Object.Destroy(ball.gameObject);
        }
    }

    public static void ClearZombieKingFireBallsOnRow(int rowY)
    {
        var balls = Object.FindObjectsOfType<ZombieKingFireBall>();
        for (int i = 0; i < balls.Length; i++)
        {
            var ball = balls[i];
            if (ball != null && ball.IsOnMapRow(rowY))
                Object.Destroy(ball.gameObject);
        }
    }

    public static void MeltIceOnRow(int rowY)
    {
        var snow = Effect_Snow.GetInstanceOrNull();
        if (snow != null)
            snow.MeltIceOnRow(rowY);
    }

    /// <summary>常规单位看 standTile；大体型（僵王等）看碰撞体是否覆盖该行。</summary>
    public static bool IsChessOnMapRow(Chess chess, int rowY, MapManage map)
    {
        if (chess == null || map == null || !map.IfInMapRange(0, rowY)) return false;

        var stand = chess.moveController?.standTile;
        if (stand != null && stand.mapPos.y == rowY)
            return true;

        return ColliderIntersectsMapRow(chess.GetComponent<Collider2D>(), map, rowY);
    }

    static bool ColliderIntersectsMapRow(Collider2D col, MapManage map, int rowY)
    {
        if (col == null || map == null || !map.IfInMapRange(0, rowY)) return false;

        var refTile = map.tiles[0, rowY];
        if (refTile == null) return false;

        float rowCenterY = refTile.transform.position.y;
        float halfHeight = GetMapRowHalfHeight(map);
        Bounds b = col.bounds;
        return b.max.y >= rowCenterY - halfHeight && b.min.y <= rowCenterY + halfHeight;
    }

    static float GetMapRowHalfHeight(MapManage map)
    {
        if (map == null) return 0.5f;
        if (map.mapSize.y >= 2)
        {
            var t0 = map.tiles[0, 0];
            var t1 = map.tiles[0, 1];
            if (t0 != null && t1 != null)
                return Mathf.Abs(t1.transform.position.y - t0.transform.position.y) * 0.5f;
        }
        return map.tileSize.y * 0.5f;
    }
}
