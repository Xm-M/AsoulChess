using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 开局在第 <see cref="startColumnX"/> 列至可种植最后一列（<c>mapSize.x - 2</c>）范围内随机生成 <see cref="spawnCount"/> 个墓碑；
/// 每波「一大波」（<see cref="EventName.WaveZombieComming"/>，wave % 10 == 0）对场上墓碑调用 <see cref="ISkill.ReturnCD"/>，使其各召唤一次僵尸。
/// </summary>
public class GameStartPlugin_Tombstone : ILevelPlugin
{
    [LabelText("墓碑 PropertyCreator")]
    public PropertyCreator tombstoneCreator;

    [LabelText("起始列 n（含，0 起）")]
    [Min(0)]
    public int startColumnX = 3;

    [LabelText("生成数量 m")]
    [Min(1)]
    public int spawnCount = 3;

    [LabelText("队伍 Tag")]
    public string teamTag = "Enemy";

    readonly List<Chess> _tombstones = new List<Chess>();

    public void StadgeEffect(LevelController levelController)
    {
        Unregister();
        _tombstones.Clear();

        if (!SaveLoadContext.IsLoadFromSave)
            SpawnInitialTombstones();

        Register();
    }

    public void OverPlugin(LevelController levelController)
    {
        Unregister();
        _tombstones.Clear();
    }

    void Register()
    {
        EventController.Instance.AddListener(EventName.WaveZombieComming.ToString(), OnBigWave);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void Unregister()
    {
        EventController.Instance.RemoveListener(EventName.WaveZombieComming.ToString(), OnBigWave);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnLeaveLevel()
    {
        Unregister();
        _tombstones.Clear();
    }

    void SpawnInitialTombstones()
    {
        if (tombstoneCreator == null || spawnCount <= 0)
            return;

        var positions = GetRandomPositions(spawnCount);
        for (int i = 0; i < positions.Count; i++)
            SpawnTombstoneAt(positions[i]);
    }

    void OnBigWave()
    {
        PruneDeadTombstones();
        for (int i = 0; i < _tombstones.Count; i++)
        {
            Chess chess = _tombstones[i];
            if (chess == null || chess.IfDeath)
                continue;
            chess.skillController?.activeSkill?.ReturnCD();
        }
    }

    void SpawnTombstoneAt(Vector2Int grid)
    {
        var map = MapManage.instance;
        if (map == null || !map.IfInMapRange(grid.x, grid.y))
            return;

        Tile tile = map.tiles[grid.x, grid.y];
        if (tile == null || !IsTileFree(tile))
            return;

        Chess chess = ChessTeamManage.Instance.CreateChess(tombstoneCreator, tile, teamTag);
        tile.PlantChess(chess);
        _tombstones.Add(chess);
        chess.OnRemove.AddListener(OnTombstoneRemoved);
    }

    void OnTombstoneRemoved(Chess chess)
    {
        if (chess != null)
            _tombstones.Remove(chess);
    }

    void PruneDeadTombstones()
    {
        for (int i = _tombstones.Count - 1; i >= 0; i--)
        {
            Chess chess = _tombstones[i];
            if (chess == null || chess.IfDeath)
                _tombstones.RemoveAt(i);
        }
    }

    List<Vector2Int> GetRandomPositions(int count)
    {
        var map = MapManage.instance;
        if (map == null || map.tiles == null || count <= 0)
            return new List<Vector2Int>();

        int xMin = Mathf.Max(0, startColumnX);
        int xMax = GetPlantableMaxColumnX(map);
        if (xMin > xMax)
            return new List<Vector2Int>();

        var blocked = new HashSet<Vector2Int>();
        for (int i = 0; i < _tombstones.Count; i++)
        {
            Chess tb = _tombstones[i];
            if (tb == null || tb.IfDeath)
                continue;
            Tile st = tb.moveController?.standTile;
            if (st != null)
                blocked.Add(st.mapPos);
        }

        var all = new List<Vector2Int>();
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (!map.IfInMapRange(x, y))
                    continue;
                var cell = new Vector2Int(x, y);
                if (blocked.Contains(cell))
                    continue;
                Tile tile = map.tiles[x, y];
                if (tile == null || !IsTileFree(tile))
                    continue;
                all.Add(cell);
            }
        }

        for (int i = 0; i < all.Count; i++)
        {
            int j = Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }

        count = Mathf.Min(count, all.Count);
        return all.Count == 0 ? new List<Vector2Int>() : all.GetRange(0, count);
    }

    /// <summary>与种植/索敌一致：最右一列为出生格，可种植上限为 <c>mapSize.x - 2</c>。</summary>
    static int GetPlantableMaxColumnX(MapManage map)
    {
        return Mathf.Max(0, map.mapSize.x - 2);
    }

    static bool IsTileFree(Tile tile)
    {
        if (tile.stander != null)
            return false;
        if (tile.chessesIntile == null)
            return true;
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            Chess c = tile.chessesIntile[i];
            if (c != null && !c.IfDeath)
                return false;
        }
        return true;
    }
}
