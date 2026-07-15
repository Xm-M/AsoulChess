using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 全图冰块统一管理：在 Inspector 配置 <see cref="iceCellPrefab"/>，由 <see cref="WeatherManage"/> 经 <see cref="GameStartPlugin_Snow"/> 在开局创建；
/// 冰车、初雪等只调用 <see cref="PlaceOrRefreshIce"/> / <see cref="TryBfsPlaceFirstEmptyIce"/>，不再各自带预制体；己方冰可覆盖敌方冰。
/// 每 <see cref="Vector2Int"/> 至多一块 <see cref="IceCell"/>；实例作为子物体挂在 Effect_Snow 下便于回收时清理。
/// </summary>
public class Effect_Snow : MonoBehaviour
{
    public static Effect_Snow Instance { get; private set; }

    [Tooltip("带 IceCell 的冰块预制体；全局唯一配置")]
    [SerializeField]
    GameObject iceCellPrefab;

    [SerializeField]
    [Tooltip("生成的冰块是否作为本物体子节点（推荐 true，便于关卡结束与对象池回收）")]
    bool parentIceUnderThis = true;

    [SerializeField]
    [Tooltip("推车列铺冰且格上有小推车时，在其上生成的占位植物（美术冰块就绪后替换配置）")]
    PropertyCreator iceCoverOnMowerCreator;

    const string DefaultIceCoverCreatorName = "南瓜罩";

    readonly Dictionary<Vector2Int, IceCell> _iceByMapPos = new Dictionary<Vector2Int, IceCell>();
    readonly Dictionary<Vector2Int, Chess> _iceCoverByMapPos = new Dictionary<Vector2Int, Chess>();
    readonly HashSet<Chess> _activeHatsuyukis = new HashSet<Chess>();
    readonly Dictionary<Chess, float> _hatsuyukiCoeffs = new Dictionary<Chess, float>();
    readonly HashSet<Chess> _hookedChess = new HashSet<Chess>();
    readonly DamageMessege _enterIceDm = new DamageMessege();

    bool _listeningForChessSpawn;
    UnityAction<Chess, Tile> _reachHandler;

    static readonly int[] BfsDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
    static readonly int[] BfsDy = { -1, -1, -1, 0, 0, 1, 1, 1 };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _reachHandler = OnChessReachTile;
    }

    void OnDestroy()
    {
        StopHatsuyukiListening();
        _activeHatsuyukis.Clear();
        _hatsuyukiCoeffs.Clear();
        if (Instance == this)
            Instance = null;
    }

    /// <summary>由 <see cref="WeatherManage.GetOrCreateSnow"/> 在生成后调用；可检查配置。</summary>
    public void InitSnow()
    {
        if (iceCellPrefab == null)
            Debug.LogError("[Effect_Snow] iceCellPrefab 未配置，无法铺冰。", this);
    }

    /// <summary>
    /// 优先 <see cref="WeatherManage.GetSnowOrNull"/>；若无且 <see cref="GameManage.weatherManage"/> 已配置雪地预制体，
    /// 则 <see cref="WeatherManage.EnsureSnow"/> 创建并初始化（无需关卡挂 <see cref="GameStartPlugin_Snow"/>）。
    /// 否则回退静态 Instance / 场景查找。
    /// </summary>
    public static Effect_Snow GetInstanceOrNull()
    {
        if (GameManage.instance?.weatherManage != null)
        {
            var wm = GameManage.instance.weatherManage;
            Effect_Snow s = wm.GetSnowOrNull();
            if (s != null) return s;
            return wm.EnsureSnow();
        }
        if (Instance != null) return Instance;
        return FindObjectOfType<Effect_Snow>(true);
    }

    /// <summary>初雪进场：登记来源与伤害系数，并在首只初雪时开始监听全场进格。</summary>
    public void RegisterHatsuyuki(Chess hatsuyuki, float iceEnterDamageCoeff)
    {
        if (hatsuyuki == null) return;
        _activeHatsuyukis.Add(hatsuyuki);
        _hatsuyukiCoeffs[hatsuyuki] = iceEnterDamageCoeff;
        if (_activeHatsuyukis.Count == 1)
            StartHatsuyukiListening();
    }

    /// <summary>初雪离场：最后一只注销时拆除进格监听。</summary>
    public void UnregisterHatsuyuki(Chess hatsuyuki)
    {
        if (hatsuyuki == null) return;
        _activeHatsuyukis.Remove(hatsuyuki);
        _hatsuyukiCoeffs.Remove(hatsuyuki);
        if (_activeHatsuyukis.Count == 0)
            StopHatsuyukiListening();
    }

    /// <summary>地图内可铺冰：主网格 <c>0 .. mapSize.x-1</c>，或推车列 <see cref="GridFindTargetGeometry.LawnMowerColumnMapX"/>。</summary>
    public static bool IsValidIceMapPos(Vector2Int p, Vector2Int mapSize)
    {
        return IsValidIceCellPos(p, mapSize);
    }

    static bool IsValidIceCellPos(Vector2Int p, Vector2Int mapSize)
    {
        if (mapSize.y <= 0 || p.y < 0 || p.y >= mapSize.y)
            return false;
        if (p.x == GridFindTargetGeometry.LawnMowerColumnMapX)
            return GridFindTargetGeometry.TryGetRoomTile(p.y, out _);
        if (mapSize.x <= 0 || p.x < 0 || p.x >= mapSize.x)
            return false;
        return true;
    }

    /// <summary>仅非 <see cref="TileType.Water"/> 地块可铺冰（含 Water 标志的格不铺）。</summary>
    public static bool CanPlaceIceOnTile(Tile tile)
    {
        if (tile == null) return false;
        return (tile.tileType & TileType.Water) == 0;
    }

    public bool HasIceAt(Vector2Int mapPos)
    {
        if (!_iceByMapPos.TryGetValue(mapPos, out IceCell ice) || ice == null)
        {
            if (_iceByMapPos.ContainsKey(mapPos))
                _iceByMapPos.Remove(mapPos);
            return false;
        }
        return true;
    }

    /// <summary>获取该格已存在的 <see cref="IceCell"/>；无冰或已失效则 false。</summary>
    public bool TryGetIceCell(Vector2Int mapPos, out IceCell ice)
    {
        if (!_iceByMapPos.TryGetValue(mapPos, out ice) || ice == null)
        {
            if (_iceByMapPos.ContainsKey(mapPos))
                _iceByMapPos.Remove(mapPos);
            return false;
        }
        return true;
    }

    public void UnregisterIce(Vector2Int mapPos)
    {
        _iceByMapPos.Remove(mapPos);
        RemoveIceCoverAt(mapPos);
    }

    /// <summary>融掉所有登记中的冰（关卡结束/回收 Effect_Snow 前调用）。</summary>
    public void ClearAllIce()
    {
        var keys = new List<Vector2Int>(_iceByMapPos.Keys);
        foreach (Vector2Int k in keys)
        {
            if (_iceByMapPos.TryGetValue(k, out IceCell ice) && ice != null)
                ice.Melt();
        }
        _iceByMapPos.Clear();
        ClearAllIceCovers();
        _activeHatsuyukis.Clear();
        _hatsuyukiCoeffs.Clear();
        StopHatsuyukiListening();
    }

    /// <summary>融掉地图上一整行（固定 <paramref name="mapPosY"/>）的冰，不分敌我归属。</summary>
    public void MeltIceOnRow(int mapPosY)
    {
        if (MapManage.instance == null) return;
        int w = MapManage.instance.mapSize.x;
        for (int x = 0; x < w; x++)
        {
            var p = new Vector2Int(x, mapPosY);
            if (TryGetIceCell(p, out IceCell ice) && ice != null)
                ice.Melt();
        }

        var mowerKey = new Vector2Int(GridFindTargetGeometry.LawnMowerColumnMapX, mapPosY);
        if (TryGetIceCell(mowerKey, out IceCell mowerIce) && mowerIce != null)
            mowerIce.Melt();
    }

    /// <summary>
    /// 在指定格铺冰；若已有冰且归属与 <paramref name="ownerTag"/> 相同则刷新时长；
    /// 若已有冰且归属不同（如冰车被动 <c>IceTrail</c> 碾过异阵营雪块）则改为与来源同 <c>tag</c> 并刷新时长。
    /// <paramref name="hatsuyukiSource"/> 非空时标记为初雪冰（非初雪铺冰应传 null 并清除标记）。
    /// </summary>
    public bool PlaceOrRefreshIce(Vector2Int mapPos, float lifetimeSeconds, string ownerTag, Chess hatsuyukiSource = null)
    {
        if (iceCellPrefab == null || MapManage.instance == null) return false;
        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceCellPos(mapPos, mapSize)) return false;

        if (_iceByMapPos.TryGetValue(mapPos, out IceCell existing) && existing != null)
        {
            bool ownerChanged = existing.IceOwnerTag != ownerTag;
            if (ownerChanged)
                existing.SetOwnerAndLifetime(ownerTag, lifetimeSeconds);
            else
                existing.ResetLifetime(lifetimeSeconds);

            ApplyHatsuyukiSourceToCell(existing, hatsuyukiSource, ownerChanged);
            if (GridFindTargetGeometry.TryResolveTileAt(mapPos.x, mapPos.y, MapManage.instance, out Tile refreshTile))
                TrySpawnIceCoverOnLawnMower(mapPos, refreshTile);
            return true;
        }

        if (_iceByMapPos.ContainsKey(mapPos))
            _iceByMapPos.Remove(mapPos);

        if (!GridFindTargetGeometry.TryResolveTileAt(mapPos.x, mapPos.y, MapManage.instance, out Tile tile))
            return false;
        if (!CanPlaceIceOnTile(tile)) return false;

        Vector3 pos = tile.transform.position;
        GameObject go = Instantiate(iceCellPrefab, pos, Quaternion.identity);
        if (parentIceUnderThis)
            go.transform.SetParent(transform, true);

        IceCell ice = go.GetComponent<IceCell>();
        if (ice == null)
        {
            Destroy(go);
            return false;
        }

        ice.Init(pos, ownerTag, lifetimeSeconds, mapPos);
        _iceByMapPos[mapPos] = ice;
        ApplyHatsuyukiSourceToCell(ice, hatsuyukiSource, applyStanderDamage: true);
        TrySpawnIceCoverOnLawnMower(mapPos, tile);
        return true;
    }

    /// <summary>
    /// 从起点八向 BFS：遍历中遇同归属冰只 <see cref="IceCell.ResetLifetime"/> 并继续扩展；
    /// 仅在「空位成功生成新冰」或占领异阵营冰时返回 true 并结束。
    /// </summary>
    public bool TryBfsPlaceFirstEmptyIce(Vector2Int startMapPos, float lifetimeSeconds, string ownerTag, Chess hatsuyukiSource = null)
    {
        if (MapManage.instance == null || iceCellPrefab == null) return false;

        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceCellPos(startMapPos, mapSize))
            startMapPos = ClampStartToValid(startMapPos, mapSize);

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startMapPos);
        visited.Add(startMapPos);

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            if (!IsValidIceCellPos(p, mapSize))
                continue;

            if (TryGetIceCell(p, out IceCell cell))
            {
                if (cell.IceOwnerTag != ownerTag)
                {
                    cell.SetOwnerAndLifetime(ownerTag, lifetimeSeconds);
                    ApplyHatsuyukiSourceToCell(cell, hatsuyukiSource, applyStanderDamage: true);
                    return true;
                }
                cell.ResetLifetime(lifetimeSeconds);
            }
            else if (CanPlaceIceAt(p) && PlaceOrRefreshIce(p, lifetimeSeconds, ownerTag, hatsuyukiSource))
                return true;

            for (int i = 0; i < 8; i++)
            {
                Vector2Int np = new Vector2Int(p.x + BfsDx[i], p.y + BfsDy[i]);
                if (visited.Contains(np)) continue;
                if (!IsValidIceCellPos(np, mapSize)) continue;
                visited.Add(np);
                queue.Enqueue(np);
            }
        }

        return false;
    }

    /// <summary>单位进入初雪冰格时结算一次伤害（铺冰初雪须存活）。</summary>
    public void TryApplyHatsuyukiIceEnterDamage(Chess victim, Vector2Int mapPos)
    {
        if (victim == null || victim.IfDeath) return;
        if (!TryGetIceCell(mapPos, out IceCell ice)) return;

        Chess source = ice.HatsuyukiSource;
        if (source == null || source.IfDeath) return;
        if (!IsEnemyOf(source, victim)) return;
        if (!_hatsuyukiCoeffs.TryGetValue(source, out float coeff) || coeff <= 0f) return;

        float atk = source.propertyController != null ? source.propertyController.GetAttack() : 0f;
        if (atk <= 0f) return;

        _enterIceDm.damageFrom = source;
        _enterIceDm.damageTo = victim;
        _enterIceDm.damage = atk * coeff;
        _enterIceDm.damageType = DamageType.Magic;
        _enterIceDm.damageElementType = ElementType.AOE;
        victim.propertyController?.GetDamage(_enterIceDm);
    }

    void ApplyHatsuyukiSourceToCell(IceCell cell, Chess hatsuyukiSource, bool applyStanderDamage)
    {
        if (cell == null) return;

        if (hatsuyukiSource != null)
        {
            cell.SetHatsuyukiSource(hatsuyukiSource);
            if (applyStanderDamage && cell.MapPos.x != int.MinValue)
                TryApplyStanderEnterDamage(cell.MapPos);
        }
        else
            cell.ClearHatsuyukiSource();
    }

    void TryApplyStanderEnterDamage(Vector2Int mapPos)
    {
        if (MapManage.instance == null) return;
        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceCellPos(mapPos, mapSize)) return;

        if (!GridFindTargetGeometry.TryResolveTileAt(mapPos.x, mapPos.y, MapManage.instance, out Tile tile))
            return;
        if (tile?.stander != null)
            TryApplyHatsuyukiIceEnterDamage(tile.stander, mapPos);
    }

    void StartHatsuyukiListening()
    {
        if (_listeningForChessSpawn) return;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        _listeningForChessSpawn = true;
        HookAllChessOnField();
    }

    void StopHatsuyukiListening()
    {
        if (!_listeningForChessSpawn) return;
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), OnChessEnterWar);
        _listeningForChessSpawn = false;

        var copy = new List<Chess>(_hookedChess);
        for (int i = 0; i < copy.Count; i++)
            UnhookReach(copy[i]);
        _hookedChess.Clear();
    }

    void OnChessEnterWar(Chess chess) => HookReach(chess);

    void HookAllChessOnField()
    {
        var ctm = GameManage.instance?.chessTeamManage;
        if (ctm == null) return;

        foreach (Chess c in ctm.GetTeam("Player"))
            HookReach(c);
        foreach (Chess c in ctm.GetTeam("Enemy"))
            HookReach(c);
    }

    void HookReach(Chess chess)
    {
        if (chess == null || chess.moveController == null || _hookedChess.Contains(chess)) return;
        chess.moveController.OnReachTile.AddListener(_reachHandler);
        _hookedChess.Add(chess);
    }

    void UnhookReach(Chess chess)
    {
        if (chess == null || chess.moveController == null) return;
        chess.moveController.OnReachTile.RemoveListener(_reachHandler);
        _hookedChess.Remove(chess);
    }

    void OnChessReachTile(Chess walker, Tile tile)
    {
        if (tile == null || _activeHatsuyukis.Count == 0) return;
        Vector2Int iceKey = GridFindTargetGeometry.TileToIceKey(tile);
        TryApplyHatsuyukiIceEnterDamage(walker, iceKey);
    }

    static bool IsEnemyOf(Chess source, Chess target)
    {
        if (source == null || target == null || source.tag == target.tag) return false;
        var ctm = GameManage.instance?.chessTeamManage;
        if (ctm == null) return false;
        List<Chess> enemies = ctm.GetEnemyTeam(source.tag);
        return enemies != null && enemies.Contains(target);
    }

    bool CanPlaceIceAt(Vector2Int p)
    {
        if (MapManage.instance == null) return false;
        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceCellPos(p, mapSize)) return false;
        if (!GridFindTargetGeometry.TryResolveTileAt(p.x, p.y, MapManage.instance, out Tile tile))
            return false;
        return CanPlaceIceOnTile(tile);
    }

    /// <summary>
    /// 开局铺冰：与 <see cref="Effect_Smoke.HideSmokeInColumns"/> 对称，列索引 ≤ <paramref name="excludeColumnsUpToInclusive"/> 的列不铺冰，
    /// 其余可铺列（含最右列）仅对非 Water 格铺冰。
    /// </summary>
    public void PlaceInitialIceAfterExcludedColumns(int excludeColumnsUpToInclusive, float lifetimeSeconds, string ownerTag)
    {
        if (iceCellPrefab == null || MapManage.instance == null) return;
        var map = MapManage.instance;
        var mapSize = map.mapSize;
        for (int x = excludeColumnsUpToInclusive + 1; x <= mapSize.x - 1; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector2Int p = new Vector2Int(x, y);
                if (!IsValidIceMapPos(p, mapSize)) continue;
                PlaceOrRefreshIce(p, lifetimeSeconds, ownerTag);
            }
        }
    }

    static Vector2Int ClampStartToValid(Vector2Int start, Vector2Int mapSize)
    {
        int x = Mathf.Clamp(start.x, 0, Mathf.Max(0, mapSize.x - 1));
        int y = Mathf.Clamp(start.y, 0, Mathf.Max(0, mapSize.y - 1));
        return new Vector2Int(x, y);
    }

    /// <summary>推车列有冰且格上存在小推车时，生成占位罩子（当前用南瓜罩配置）。</summary>
    void TrySpawnIceCoverOnLawnMower(Vector2Int mapPos, Tile tile)
    {
        if (mapPos.x != GridFindTargetGeometry.LawnMowerColumnMapX || tile == null)
            return;
        if (_iceCoverByMapPos.TryGetValue(mapPos, out Chess existing) && existing != null && !existing.IfDeath)
            return;
        if (FindLawnMowerOnTile(tile) == null)
            return;

        PropertyCreator creator = ResolveIceCoverCreator();
        if (creator == null || !creator.IfCanPlant(tile))
            return;

        Chess cover = ChessTeamManage.Instance.CreateChess(creator, tile, "Player");
        tile.PlantChess(cover);
        _iceCoverByMapPos[mapPos] = cover;
        cover.OnRemove.AddListener(_ => _iceCoverByMapPos.Remove(mapPos));
    }

    static Chess FindLawnMowerOnTile(Tile tile)
    {
        if (tile.chessesIntile == null) return null;
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            Chess c = tile.chessesIntile[i];
            if (c == null || c.IfDeath) continue;
            if (c.GetComponentInChildren<CarArmor>() != null)
                return c;
        }
        return null;
    }

    PropertyCreator ResolveIceCoverCreator()
    {
        if (iceCoverOnMowerCreator != null)
            return iceCoverOnMowerCreator;
        var all = GameManage.instance?.allChess;
        if (all == null) return null;
        for (int i = 0; i < all.Count; i++)
        {
            PropertyCreator c = all[i];
            if (c != null && c.chessName == DefaultIceCoverCreatorName)
                return c;
        }
        return null;
    }

    void RemoveIceCoverAt(Vector2Int mapPos)
    {
        if (!_iceCoverByMapPos.TryGetValue(mapPos, out Chess cover))
            return;
        _iceCoverByMapPos.Remove(mapPos);
        if (cover != null && !cover.IfDeath)
            cover.Death();
    }

    void ClearAllIceCovers()
    {
        var keys = new List<Vector2Int>(_iceCoverByMapPos.Keys);
        for (int i = 0; i < keys.Count; i++)
            RemoveIceCoverAt(keys[i]);
    }
}
