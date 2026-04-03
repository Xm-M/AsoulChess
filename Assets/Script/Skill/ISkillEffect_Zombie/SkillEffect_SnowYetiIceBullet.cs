using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 远程雪怪冷却技能：向地图上随机一格方向发射一枚冰弹；伤害为 GetAttack() * config.baseDamage[0]（在 InitBullet 中传入）。
/// 可在技能上配置 takeBuffOnHit（通常为 FreezyBuff），通过 DamageMessege.takeBuff 在命中时施加；也可仅在子弹 prefab 的 Dm 上配置。
/// </summary>
[Serializable]
public class SkillEffect_SnowYetiIceBullet : ISkillEffect
{
    [Tooltip("冰弹预制体（需含 Bullet、Dm、bulletMove）")]
    public GameObject iceBulletPrefab;

    [Tooltip("命中时通过 DamageMessege.takeBuff 施加；留空则使用 prefab 上 Dm 的配置")]
    [SerializeReference]
    public Buff takeBuffOnHit;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || iceBulletPrefab == null || MapManage.instance == null) return;

        float mult = 1f;
        if (config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            mult = config.baseDamage[0];
        float damage = user.propertyController.GetAttack() * mult;

        Tile tile = PickRandomTile();
        if (tile == null) return;

        Vector2 start = user.transform.position;
        Vector2 targetWorld = tile.transform.position;
        Vector2 dir = (targetWorld - start).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = new Vector2(user.transform.right.x, user.transform.right.y);

        GameObject go = ObjectPool.instance.Create(iceBulletPrefab);
        Bullet b = go.GetComponent<Bullet>();
        if (b == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }
        if (b.Dm == null)
            b.Dm = new DamageMessege();
        b.InitBullet(user, start, null, dir, damage, 1f);
        b.targetPos = targetWorld;
        if (takeBuffOnHit != null)
            b.Dm.takeBuff = takeBuffOnHit;
    }

    static Tile PickRandomTile()
    {
        var m = MapManage.instance;
        if (m == null || m.tiles == null || m.mapSize.x <= 0 || m.mapSize.y <= 0) return null;
        int x = UnityEngine.Random.Range(0, m.mapSize.x);
        int y = UnityEngine.Random.Range(0, m.mapSize.y);
        return m.tiles[x, y];
    }
}
