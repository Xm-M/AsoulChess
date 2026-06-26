using System.Collections.Generic;
using UnityEngine;

/// <summary>魂灵之影落点：四邻 BFS，限定在主人 <see cref="IGridFindTarget"/> 攻击格内，上限 3。</summary>
public static class WisadelShadowPlacer
{
    static readonly HashSet<Vector2Int> AttackCellsBuffer = new HashSet<Vector2Int>();

    public static int TrySummon(Chess master, PropertyCreator shadowCreator, int count, GameObject summonEffect = null)
    {
        if (master == null || shadowCreator == null || count <= 0)
            return 0;
        if (GameManage.instance?.chessTeamManage == null || MapManage.instance == null)
            return 0;

        var list = GetOrCreateShadowList(master);
        PruneDead(list);
        if (list.Count >= 3)
            return 0;

        int toSummon = Mathf.Min(count, 3 - list.Count);
        var occupied = BuildOccupiedSet(master, list);
        int summoned = 0;

        for (int i = 0; i < toSummon; i++)
        {
            Tile place = FindBestTile(master, shadowCreator, occupied);
            if (place == null)
                break;

            Chess shadow = GameManage.instance.chessTeamManage.CreateChess(shadowCreator, place, master.tag);
            if (shadow == null)
                break;

            shadow.skillController.context.Set(WisadelKeys.Master, master);
            list.Add(shadow);
            occupied.Add(place);
            summoned++;

            shadow.OnRemove.AddListener(removed => OnShadowRemoved(master, removed));
            if (summonEffect != null)
            {
                GameObject fx = ObjectPool.instance.Create(summonEffect);
                fx.transform.position = shadow.transform.position;
            }
        }

        master.skillController.context.Set(WisadelKeys.Shadows, list);
        if (summoned > 0)
            WisadelStealthHelper.Refresh(master);
        return summoned;
    }

    public static void DestroyAllShadows(Chess master)
    {
        if (master?.skillController?.context == null)
            return;

        if (!master.skillController.context.TryGet(WisadelKeys.Shadows, out List<Chess> list) || list == null)
            return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            Chess s = list[i];
            if (s != null && !s.IfDeath)
                s.Death();
        }
        list.Clear();
        master.skillController.context.Set(WisadelKeys.Shadows, list);
    }

    static void OnShadowRemoved(Chess master, Chess shadow)
    {
        if (master?.skillController?.context == null)
            return;
        if (!master.skillController.context.TryGet(WisadelKeys.Shadows, out List<Chess> list) || list == null)
            return;
        list.Remove(shadow);
        master.skillController.context.Set(WisadelKeys.Shadows, list);
        WisadelStealthHelper.Refresh(master);
    }

    static HashSet<Tile> BuildOccupiedSet(Chess master, List<Chess> shadows)
    {
        var occupied = new HashSet<Tile>();
        Tile masterTile = master.moveController?.standTile;
        if (masterTile != null)
            occupied.Add(masterTile);

        for (int i = 0; i < shadows.Count; i++)
        {
            Tile t = shadows[i]?.moveController?.standTile;
            if (t != null)
                occupied.Add(t);
        }
        return occupied;
    }

    static Tile FindBestTile(Chess master, PropertyCreator shadowCreator, HashSet<Tile> occupied)
    {
        Tile start = master.moveController?.standTile;
        if (start == null)
            return null;

        if (!WisadelGridHelper.TryCollectAttackCells(master, AttackCellsBuffer))
            return null;

        var candidates = new List<Tile>();
        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Tile tile = queue.Dequeue();
            foreach (Tile neighbor in MapManage.instance.NearTile(tile))
            {
                if (neighbor == null || !visited.Add(neighbor))
                    continue;
                if (!WisadelGridHelper.IsTileInAttackCells(neighbor, AttackCellsBuffer))
                    continue;

                queue.Enqueue(neighbor);

                if (occupied.Contains(neighbor))
                    continue;
                if (!IsValidDeployTile(shadowCreator, neighbor))
                    continue;

                candidates.Add(neighbor);
            }
        }

        if (candidates.Count == 0)
            return null;

        candidates.Sort((a, b) => ComparePlacementTile(master, a, b));
        return candidates[0];
    }

    static bool IsValidDeployTile(PropertyCreator shadowCreator, Tile tile)
    {
        if (tile == null || shadowCreator == null)
            return false;
        if (!MapManage.instance.IsPlantColumnAllowed(tile.mapPos.x))
            return false;
        return shadowCreator.IfCanPlant(tile);
    }

    static int ComparePlacementTile(Chess master, Tile a, Tile b)
    {
        Tile start = master.moveController.standTile;
        int da = WisadelGridHelper.ManhattanDistance(start, a);
        int db = WisadelGridHelper.ManhattanDistance(start, b);
        if (da != db) return da.CompareTo(db);
        if (a.mapPos.y != b.mapPos.y) return a.mapPos.y.CompareTo(b.mapPos.y);
        return a.mapPos.x.CompareTo(b.mapPos.x);
    }

    static List<Chess> GetOrCreateShadowList(Chess master)
    {
        if (!master.skillController.context.TryGet(WisadelKeys.Shadows, out List<Chess> list) || list == null)
        {
            list = new List<Chess>();
            master.skillController.context.Set(WisadelKeys.Shadows, list);
        }
        return list;
    }

    static void PruneDead(List<Chess> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null || list[i].IfDeath)
                list.RemoveAt(i);
        }
    }
}
