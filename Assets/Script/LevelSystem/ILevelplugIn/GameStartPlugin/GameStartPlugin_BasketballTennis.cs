using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 投篮网球 MiniMode：最左列守护植物、最右列投小丑盒；插件定时随机行投篮；
/// 守护植物周围自动反弹敌方子弹。胜负：全灭投篮车胜利，守护植物死亡失败。
/// </summary>
public class GameStartPlugin_BasketballTennis : ILevelPlugin
{
    [LabelText("投小丑盒 PropertyCreator")]
    public PropertyCreator catapultCreator;

    [LabelText("各行守护植物（索引=行 y）")]
    public List<BasketballGuardRowConfig> guardRows = new List<BasketballGuardRowConfig>();

    [LabelText("投篮间隔（秒）")]
    [Min(0.5f)]
    public float attackIntervalSeconds = 3f;

    [LabelText("小丑盒子弹预制体")]
    public GameObject bulletPrefab;

    [LabelText("自动反弹半径（格）")]
    [Min(0.5f)]
    public float autoDeflectTileRadius = 3f;

    [LabelText("自动反弹检测间隔（秒）")]
    [Min(0.02f)]
    public float autoDeflectCheckInterval = 0.05f;

    readonly List<Chess> _catapults = new List<Chess>();
    readonly List<Chess> _guards = new List<Chess>();
    Timer _attackTimer;
    Timer _deflectTimer;
    bool _gameEnded;

    public void StadgeEffect(LevelController levelController)
    {
        _gameEnded = false;
        _catapults.Clear();
        _guards.Clear();

        if (SaveLoadContext.IsLoadFromSave)
            return;

        SpawnFieldUnits(levelController);
        _attackTimer = GameManage.instance.timerManage.AddTimer(TriggerRandomShoot, attackIntervalSeconds, true);
        _deflectTimer = GameManage.instance.timerManage.AddTimer(AutoDeflectTick, autoDeflectCheckInterval, true);
    }

    public void OverPlugin(LevelController levelController)
    {
        _attackTimer?.Stop();
        _attackTimer = null;
        _deflectTimer?.Stop();
        _deflectTimer = null;
        UnbindAll();
        _catapults.Clear();
        _guards.Clear();
        _gameEnded = false;
    }

    void SpawnFieldUnits(LevelController levelController)
    {
        if (catapultCreator == null || guardRows == null || guardRows.Count == 0)
            return;

        var map = MapManage.instance;
        if (map == null || map.tiles == null)
            return;

        int catapultColumn = GetCatapultColumnX(map);

        for (int row = 0; row < guardRows.Count; row++)
        {
            BasketballGuardRowConfig cfg = guardRows[row];
            if (cfg?.guardPlant == null)
                continue;
            if (!map.IfInMapRange(0, row))
                continue;

            Tile plantTile = map.tiles[0, row];
            if (plantTile == null)
                continue;

            Chess guard = ChessTeamManage.Instance.CreateChess(cfg.guardPlant, plantTile, "Player");
            plantTile.PlantChess(guard);
            _guards.Add(guard);
            guard.OnRemove.AddListener(OnGuardRemoved);

            Tile spawnTile = map.IfInMapRange(catapultColumn, row)
                ? map.tiles[catapultColumn, row]
                : null;
            if (spawnTile == null)
                continue;

            Chess catapult = ChessTeamManage.Instance.CreateChess(catapultCreator, spawnTile, "Enemy");
            spawnTile.PlantChess(catapult);
            FreezeCatapult(catapult);
            _catapults.Add(catapult);
            catapult.OnRemove.AddListener(OnCatapultRemoved);
            levelController.RegisterExternalWaveZombie(catapult);
        }
    }

    void AutoDeflectTick()
    {
        if (_gameEnded || !LevelManage.instance.IfGameStart || bulletPrefab == null)
            return;

        float radius = GetAutoDeflectWorldRadius();
        for (int i = 0; i < _guards.Count; i++)
        {
            Chess guard = _guards[i];
            if (guard == null || guard.IfDeath)
                continue;

            Vector2 center = guard.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, BasketballBulletDeflect.BulletLayerMask);
            for (int j = 0; j < hits.Length; j++)
            {
                Bullet bullet = hits[j].GetComponent<Bullet>();
                if (bullet == null || bullet.shooter == null || bullet.shooter.IfDeath)
                    continue;
                if (!bullet.shooter.CompareTag("Enemy"))
                    continue;
                BasketballBulletDeflect.TryDeflect(bullet, guard, bulletPrefab, center);
            }
        }
    }

    float GetAutoDeflectWorldRadius()
    {
        var map = MapManage.instance;
        if (map == null)
            return autoDeflectTileRadius;
        return autoDeflectTileRadius * map.tileSize.x;
    }

    static void FreezeCatapult(Chess catapult)
    {
        if (catapult?.equipWeapon == null || catapult.moveController == null)
            return;

        catapult.equipWeapon.AttackAble = false;
        catapult.equipWeapon.StopAttack();
        catapult.propertyController.ChangeMoveAcceleRate(-catapult.propertyController.GetMoveAcceleRate());
        catapult.moveController.StopMove();
        catapult.stateController?.ChangeState(StateName.IdleState);
    }

    void TriggerRandomShoot()
    {
        if (_gameEnded || !LevelManage.instance.IfGameStart)
            return;

        var alive = new List<Chess>();
        for (int i = 0; i < _catapults.Count; i++)
        {
            Chess c = _catapults[i];
            if (c != null && !c.IfDeath)
                alive.Add(c);
        }
        if (alive.Count == 0)
            return;

        Chess shooter = alive[UnityEngine.Random.Range(0, alive.Count)];
        Chess target = GetGuardForRow(GetRowY(shooter));
        if (target == null || target.IfDeath)
            return;

        FireCatapultShot(shooter);
    }

    static void FireCatapultShot(Chess shooter)
    {
        if (shooter?.equipWeapon?.weapon == null)
            return;

        AttackController equip = shooter.equipWeapon;
        if (equip.weapon.FindEnemy(shooter) <= 0)
            return;

        bool prevAble = equip.AttackAble;
        equip.AttackAble = true;
        equip.Attack();
        equip.AttackAble = prevAble;
        equip.StopAttack();
    }

    static int GetCatapultColumnX(MapManage map)
    {
        return Mathf.Max(0, map.mapSize.x - 2);
    }

    Chess GetGuardForRow(int rowY)
    {
        if (rowY >= 0 && rowY < _guards.Count)
            return _guards[rowY];
        for (int i = 0; i < _guards.Count; i++)
        {
            Chess g = _guards[i];
            if (g != null && !g.IfDeath)
                return g;
        }
        return null;
    }

    static int GetRowY(Chess chess)
    {
        return chess?.moveController?.standTile?.mapPos.y ?? -1;
    }

    void OnGuardRemoved(Chess chess)
    {
        if (_gameEnded || chess == null)
            return;
        if (!LevelManage.instance.IfGameStart)
            return;
        EndGame(false);
    }

    void OnCatapultRemoved(Chess chess)
    {
        if (_gameEnded || chess == null)
            return;
        for (int i = _catapults.Count - 1; i >= 0; i--)
        {
            if (_catapults[i] == null || _catapults[i].IfDeath)
                _catapults.RemoveAt(i);
        }
    }

    void EndGame(bool win)
    {
        if (_gameEnded)
            return;
        _gameEnded = true;
        _attackTimer?.Stop();
        _attackTimer = null;
        _deflectTimer?.Stop();
        _deflectTimer = null;
        LevelManage.instance.GameOver(win);
    }

    void UnbindAll()
    {
        for (int i = 0; i < _guards.Count; i++)
        {
            Chess g = _guards[i];
            if (g != null)
                g.OnRemove.RemoveListener(OnGuardRemoved);
        }
        for (int i = 0; i < _catapults.Count; i++)
        {
            Chess c = _catapults[i];
            if (c != null)
                c.OnRemove.RemoveListener(OnCatapultRemoved);
        }
    }
}

[Serializable]
public class BasketballGuardRowConfig
{
    [LabelText("守护植物")]
    public PropertyCreator guardPlant;
}

/// <summary>销毁敌方子弹并以守护植物名义生成射向原发射者的反弹弹。</summary>
static class BasketballBulletDeflect
{
    internal static readonly int BulletLayerMask = 1 << 6;

    public static void TryDeflect(Bullet original, Chess plantShooter, GameObject bulletPrefab, Vector2 deflectCenter)
    {
        if (original == null || plantShooter == null || plantShooter.IfDeath || bulletPrefab == null)
            return;

        Chess enemyShooter = original.shooter;
        if (enemyShooter == null || enemyShooter.IfDeath)
            return;

        float damage = original.Dm.damage;
        float rate = original.rate;
        Vector2 spawnPos = original.transform.position;
        original.RecycleBullet();

        GameObject go = ObjectPool.instance.Create(bulletPrefab);
        if (go == null)
            return;
        Bullet reflected = go.GetComponent<Bullet>();
        if (reflected == null)
            return;

        Vector2 dir = ((Vector2)enemyShooter.transform.position - spawnPos).normalized;
        if (dir.sqrMagnitude < 1e-4f)
            dir = ((Vector2)enemyShooter.transform.position - deflectCenter).normalized;
        if (dir.sqrMagnitude < 1e-4f)
            dir = Vector2.right;

        reflected.InitBullet(plantShooter, spawnPos, enemyShooter, dir, damage, rate);
        reflected.Dm.damageTo = enemyShooter;
    }
}
