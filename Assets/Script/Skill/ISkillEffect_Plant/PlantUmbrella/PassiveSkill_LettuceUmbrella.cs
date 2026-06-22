using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 莴苣保护伞被动：保护范围内弹飞子弹/打断蹦极；更大预警范围内提前播放伞面动画。
/// </summary>
public class PassiveSkill_LettuceUmbrella : ISkillEffect
{
    static readonly List<Tile> TileBuffer = new List<Tile>(9);
    static readonly HashSet<Chess> BungeeScanBuffer = new HashSet<Chess>();

    [LabelText("检测间隔(秒)")]
    [MinValue(0.02f)]
    public float interval = 0.05f;

    [LabelText("保护范围(格半径)")]
    [Tooltip("1 = 3×3，仅在此范围内真正弹飞子弹")]
    [MinValue(0)]
    public int protectTileRadius = 1;

    [LabelText("伞面预警范围(格半径)")]
    [Tooltip("应 ≥ 保护范围；子弹进入此范围即播放伞面动画，略早于实际弹飞")]
    [MinValue(0)]
    public int warnTileRadius = 2;

    [LabelText("子弹弹飞速度")]
    [MinValue(1f)]
    public float bulletKnockSpeed = 18f;

    [LabelText("出屏回收边距(世界单位)")]
    [MinValue(0f)]
    public float offScreenPadding = 1.5f;

    [LabelText("可打断的蹦极棋子名")]
    public string[] bungeeChessNames = { BungeeRetreatHelper.BungeePuppetChessName };

    [LabelText("蹦极弹飞延迟(秒)")]
    [Tooltip("检测到蹦极后立即播放伞面动画；持续存在达此时间后强制蹦极 skill_raise")]
    [MinValue(0f)]
    public float bungeeRetreatDelay = 1f;

    [LabelText("伞面反馈 Animator Trigger")]
    [Tooltip("留空则不播反馈动画")]
    public string deflectAnimTrigger = "Deflect";

    [LabelText("反馈动画最短间隔(秒)")]
    [MinValue(0f)]
    public float deflectAnimCooldown = 0.25f;

    Chess _user;
    Timer _timer;
    float _lastDeflectAnimTime = float.NegativeInfinity;
    readonly List<Bullet> _knockedBullets = new List<Bullet>();
    readonly Dictionary<Chess, float> _bungeeFirstSeenTime = new Dictionary<Chess, float>();
    readonly HashSet<Chess> _bungeeRetreatTriggered = new HashSet<Chess>();
    readonly List<Chess> _bungeeCleanupBuffer = new List<Chess>();

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        _user = user;
        if (GameManage.instance?.timerManage == null) return;

        _timer = GameManage.instance.timerManage.AddTimer(OnTick, interval, true);
        user.OnRemove.AddListener(OnChessRemove);
        OnTick();
    }

    void OnTick()
    {
        if (_user == null || _user.IfDeath) return;

        var centerTile = _user.moveController?.standTile;
        if (centerTile == null) return;

        int protectRadius = protectTileRadius;
        int warnRadius = Mathf.Max(warnTileRadius, protectRadius);

        ProcessWarnBullets(centerTile, warnRadius);
        ProcessBullets(centerTile, protectRadius);
        ProcessBungees(centerTile);
        RecycleOffScreenBullets();
    }

    /// <summary>更大范围：仅触发伞面动画，不处理子弹。</summary>
    void ProcessWarnBullets(Tile centerTile, int tileRadius)
    {
        var map = MapManage.instance;
        if (map == null || string.IsNullOrEmpty(deflectAnimTrigger)) return;

        float radius = GetTileRadiusWorld(map, tileRadius);
        Vector2 center = centerTile.transform.position;
        var hits = Physics2D.OverlapCircleAll(center, radius, PlantUmbrellaBulletKnock.BulletLayerMask);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var bullet = hits[i].GetComponent<Bullet>();
            if (bullet == null || !IsEnemyBullet(bullet)) continue;
            if (!IsBulletInTileRadius(bullet, centerTile, tileRadius)) continue;

            TryPlayDeflectFeedback();
            return;
        }
    }

    bool ProcessBullets(Tile centerTile, int tileRadius)
    {
        var map = MapManage.instance;
        if (map == null) return false;

        float radius = GetTileRadiusWorld(map, tileRadius);
        Vector2 center = centerTile.transform.position;
        var hits = Physics2D.OverlapCircleAll(center, radius, PlantUmbrellaBulletKnock.BulletLayerMask);
        if (hits == null || hits.Length == 0) return false;

        Vector2 knockDir = Vector2.up;
        bool any = false;
        for (int i = 0; i < hits.Length; i++)
        {
            var bullet = hits[i].GetComponent<Bullet>();
            if (bullet == null || !IsEnemyBullet(bullet)) continue;
            if (!IsBulletInTileRadius(bullet, centerTile, tileRadius)) continue;

            if (PlantUmbrellaBulletKnock.TryKnockOffScreen(bullet, knockDir, bulletKnockSpeed, offScreenPadding))
            {
                if (!_knockedBullets.Contains(bullet))
                    _knockedBullets.Add(bullet);
                any = true;
            }
            else
            {
                PlantUmbrellaBulletKnock.DestroyOrRecycle(bullet);
                any = true;
            }
        }
        return any;
    }

    static bool IsEnemyBullet(Bullet bullet)
    {
        return bullet?.shooter != null && !bullet.shooter.IfDeath && bullet.shooter.CompareTag("Enemy");
    }

    void ProcessBungees(Tile centerTile)
    {
        CollectTilesInRadius(centerTile, protectTileRadius, TileBuffer);
        BungeeScanBuffer.Clear();

        for (int ti = 0; ti < TileBuffer.Count; ti++)
        {
            var tile = TileBuffer[ti];
            if (tile?.chessesIntile == null) continue;
            for (int ci = 0; ci < tile.chessesIntile.Count; ci++)
            {
                var chess = tile.chessesIntile[ci];
                if (chess == null || chess == _user || chess.IfDeath) continue;
                if (!BungeeRetreatHelper.IsBungeeChess(chess, bungeeChessNames)) continue;
                BungeeScanBuffer.Add(chess);
            }
        }

        _bungeeCleanupBuffer.Clear();
        foreach (var kv in _bungeeFirstSeenTime)
        {
            var bungee = kv.Key;
            if (bungee == null || bungee.IfDeath || !BungeeScanBuffer.Contains(bungee))
                _bungeeCleanupBuffer.Add(bungee);
        }
        for (int i = 0; i < _bungeeCleanupBuffer.Count; i++)
            _bungeeFirstSeenTime.Remove(_bungeeCleanupBuffer[i]);

        foreach (var bungee in BungeeScanBuffer)
        {
            if (_bungeeRetreatTriggered.Contains(bungee)) continue;

            if (!_bungeeFirstSeenTime.TryGetValue(bungee, out float firstSeen))
            {
                _bungeeFirstSeenTime[bungee] = Time.time;
                TryPlayDeflectFeedback(force: true);
                continue;
            }

            if (Time.time - firstSeen < bungeeRetreatDelay) continue;

            if (BungeeRetreatHelper.TryForceRetreat(bungee, requireDropPhase: false))
            {
                _bungeeRetreatTriggered.Add(bungee);
                _bungeeFirstSeenTime.Remove(bungee);
            }
        }
    }

    void RecycleOffScreenBullets()
    {
        for (int i = _knockedBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _knockedBullets[i];
            if (bullet == null)
            {
                _knockedBullets.RemoveAt(i);
                continue;
            }
            if (PlantUmbrellaBulletKnock.IsOffScreen(bullet.transform.position, offScreenPadding))
            {
                bullet.RecycleBullet();
                _knockedBullets.RemoveAt(i);
            }
        }
    }

    static float GetTileRadiusWorld(MapManage map, int tileRadius)
    {
        return Mathf.Max(map.tileSize.x, map.tileSize.y) * (tileRadius + 0.65f);
    }

    static bool IsBulletInTileRadius(Bullet bullet, Tile center, int tileRadius)
    {
        if (bullet == null || center == null || MapManage.instance == null) return false;
        var map = MapManage.instance;
        if (map.tiles == null || map.tiles[0, 0] == null) return false;

        Vector2 origin = map.tiles[0, 0].transform.position;
        Vector2 pos = bullet.transform.position;
        int x = Mathf.RoundToInt((pos.x - origin.x) / map.tileSize.x);
        int y = Mathf.RoundToInt((pos.y - origin.y) / map.tileSize.y);

        if (!map.IfInMapRange(x, y)) return false;
        var c = center.mapPos;
        return Mathf.Abs(x - c.x) <= tileRadius && Mathf.Abs(y - c.y) <= tileRadius;
    }

    static void CollectTilesInRadius(Tile center, int tileRadius, List<Tile> list)
    {
        list.Clear();
        if (center == null || MapManage.instance == null) return;
        var p = center.mapPos;
        var map = MapManage.instance;
        for (int dy = -tileRadius; dy <= tileRadius; dy++)
        {
            for (int dx = -tileRadius; dx <= tileRadius; dx++)
            {
                int x = p.x + dx, y = p.y + dy;
                if (!map.IfInMapRange(x, y)) continue;
                var t = map.tiles[x, y];
                if (t != null)
                    list.Add(t);
            }
        }
    }

    void TryPlayDeflectFeedback(bool force = false)
    {
        if (string.IsNullOrEmpty(deflectAnimTrigger)) return;
        if (!force && Time.time - _lastDeflectAnimTime < deflectAnimCooldown) return;
        var anim = _user?.animatorController?.animator;
        if (anim == null) return;
        anim.SetTrigger(deflectAnimTrigger);
        _lastDeflectAnimTime = Time.time;
    }

    void OnChessRemove(Chess chess)
    {
        _timer?.Stop();
        _timer = null;
        _knockedBullets.Clear();
        _bungeeFirstSeenTime.Clear();
        _bungeeRetreatTriggered.Clear();
        if (_user != null)
            _user.OnRemove.RemoveListener(OnChessRemove);
        _user = null;
    }
}
