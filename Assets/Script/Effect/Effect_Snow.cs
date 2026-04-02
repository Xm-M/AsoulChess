using System.Collections.Generic;
using UnityEngine;

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

    readonly Dictionary<Vector2Int, IceCell> _iceByMapPos = new Dictionary<Vector2Int, IceCell>();

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
    }

    void OnDestroy()
    {
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

    /// <summary>地图内可铺冰：X 最大为 <c>mapSize.x - 2</c>（最右一列为僵尸出生列）。</summary>
    public static bool IsValidIceMapPos(Vector2Int p, Vector2Int mapSize)
    {
        if (mapSize.y <= 0) return false;
        if (p.y < 0 || p.y >= mapSize.y) return false;
        if (mapSize.x < 2) return false;
        if (p.x < 0 || p.x > mapSize.x - 2) return false;
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
    }

    /// <summary>在指定格铺冰或仅刷新持续时间；冰块由本组件持有的预制体生成。</summary>
    public bool PlaceOrRefreshIce(Vector2Int mapPos, float lifetimeSeconds, string ownerTag)
    {
        if (iceCellPrefab == null || MapManage.instance == null) return false;
        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceMapPos(mapPos, mapSize)) return false;

        if (_iceByMapPos.TryGetValue(mapPos, out IceCell existing) && existing != null)
        {
            existing.ResetLifetime(lifetimeSeconds);
            return true;
        }

        if (_iceByMapPos.ContainsKey(mapPos))
            _iceByMapPos.Remove(mapPos);

        Tile tile = MapManage.instance.tiles[mapPos.x, mapPos.y];
        if (tile == null) return false;
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
        return true;
    }

    /// <summary>
    /// 从起点八向 BFS：遍历中遇同归属冰只 <see cref="IceCell.ResetLifetime"/> 并继续扩展；
    /// 仅在「空位成功生成新冰」时返回 true 并结束；Player 遇敌方冰占领一格后结束并返回 true。
    /// </summary>
    public bool TryBfsPlaceFirstEmptyIce(Vector2Int startMapPos, float lifetimeSeconds, string ownerTag)
    {
        if (MapManage.instance == null || iceCellPrefab == null) return false;

        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceMapPos(startMapPos, mapSize))
            startMapPos = ClampStartToValid(startMapPos, mapSize);

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startMapPos);
        visited.Add(startMapPos);

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            if (!IsValidIceMapPos(p, mapSize))
                continue;

            if (TryGetIceCell(p, out IceCell cell))
            {
                if (ownerTag == "Player" && cell.IceOwnerTag == "Enemy")
                {
                    cell.SetOwnerAndLifetime(ownerTag, lifetimeSeconds);
                    return true;
                }
                if (cell.IceOwnerTag == ownerTag)
                    cell.ResetLifetime(lifetimeSeconds);
            }
            else if (CanPlaceIceAt(p) && PlaceOrRefreshIce(p, lifetimeSeconds, ownerTag))
                return true;

            for (int i = 0; i < 8; i++)
            {
                Vector2Int np = new Vector2Int(p.x + BfsDx[i], p.y + BfsDy[i]);
                if (visited.Contains(np)) continue;
                if (!IsValidIceMapPos(np, mapSize)) continue;
                visited.Add(np);
                queue.Enqueue(np);
            }
        }

        return false;
    }

    bool CanPlaceIceAt(Vector2Int p)
    {
        if (MapManage.instance == null) return false;
        var mapSize = MapManage.instance.mapSize;
        if (!IsValidIceMapPos(p, mapSize)) return false;
        Tile tile = MapManage.instance.tiles[p.x, p.y];
        return CanPlaceIceOnTile(tile);
    }

    /// <summary>
    /// 开局铺冰：与 <see cref="Effect_Smoke.HideSmokeInColumns"/> 对称，列索引 ≤ <paramref name="excludeColumnsUpToInclusive"/> 的列不铺冰，
    /// 其余可铺列仅对非 Water 格铺冰。
    /// </summary>
    public void PlaceInitialIceAfterExcludedColumns(int excludeColumnsUpToInclusive, float lifetimeSeconds, string ownerTag)
    {
        if (iceCellPrefab == null || MapManage.instance == null) return;
        var map = MapManage.instance;
        var mapSize = map.mapSize;
        for (int x = excludeColumnsUpToInclusive + 1; x <= mapSize.x - 2; x++)
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
        int x = Mathf.Clamp(start.x, 0, Mathf.Max(0, mapSize.x - 2));
        int y = Mathf.Clamp(start.y, 0, Mathf.Max(0, mapSize.y - 1));
        return new Vector2Int(x, y);
    }
}
