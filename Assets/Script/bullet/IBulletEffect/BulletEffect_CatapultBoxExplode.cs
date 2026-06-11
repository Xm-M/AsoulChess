using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 投小丑盒子弹命中效果：<see cref="explodeChance"/> 概率在落点 3×3 范围内对植物造成
/// <see cref="damageMultiplier"/> 倍攻击力的爆炸伤害（与直伤叠加）。
/// </summary>
[Serializable]
public class BulletEffect_CatapultBoxExplode : IBulletEffect
{
    [Range(0f, 1f)]
    public float explodeChance = 0.1f;

    public float damageMultiplier = 10f;

    [Tooltip("可选：爆炸特效预制体（需已注册 ObjectPool）")]
    public GameObject explosionEffect;

    static readonly List<Tile> TileBuffer = new List<Tile>(9);
    static readonly HashSet<Chess> HitPlants = new HashSet<Chess>();

    public void OnBulletHit(Bullet bullet)
    {
        if (bullet == null || bullet.shooter == null || bullet.shooter.IfDeath)
            return;
        if (UnityEngine.Random.value >= explodeChance)
            return;

        Tile center = bullet.Dm.damageTo?.moveController?.standTile;
        if (center == null || MapManage.instance == null)
            return;

        Chess shooter = bullet.shooter;
        float damage = shooter.propertyController.GetAttack() * damageMultiplier;

        if (explosionEffect != null && ObjectPool.instance != null)
        {
            GameObject fx = ObjectPool.instance.Create(explosionEffect);
            if (fx != null)
                fx.transform.position = center.transform.position;
        }

        CollectTiles3x3(center, TileBuffer);
        HitPlants.Clear();
        string plantTag = shooter.CompareTag("Enemy") ? "Player" : "Enemy";

        for (int i = 0; i < TileBuffer.Count; i++)
        {
            Tile tile = TileBuffer[i];
            if (tile == null)
                continue;

            TryDamagePlant(tile.stander, plantTag, shooter, damage, HitPlants);

            if (tile.chessesIntile == null)
                continue;
            for (int j = 0; j < tile.chessesIntile.Count; j++)
                TryDamagePlant(tile.chessesIntile[j], plantTag, shooter, damage, HitPlants);
        }
    }

    static void TryDamagePlant(Chess plant, string plantTag, Chess shooter, float damage, HashSet<Chess> hit)
    {
        if (plant == null || plant.IfDeath || !plant.CompareTag(plantTag))
            return;
        if (!hit.Add(plant))
            return;

        DamageMessege dm = shooter.skillController != null ? shooter.skillController.DM : new DamageMessege();
        dm.damageFrom = shooter;
        dm.damageTo = plant;
        dm.damage = damage;
        dm.damageElementType = ElementType.Explode;
        shooter.propertyController.TakeDamage(dm);
    }

    static void CollectTiles3x3(Tile center, List<Tile> list)
    {
        list.Clear();
        if (center == null || MapManage.instance == null)
            return;

        Vector2Int p = center.mapPos;
        MapManage map = MapManage.instance;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = p.x + dx;
                int y = p.y + dy;
                if (!map.IfInMapRange(x, y))
                    continue;
                Tile t = map.tiles[x, y];
                if (t != null)
                    list.Add(t);
            }
        }
    }
}
