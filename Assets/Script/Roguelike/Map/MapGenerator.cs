using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 这个是杀戮尖塔的地图
/// 尖塔式 DAG：固定宽网格上逐层选点（候选 ∪ 最小出路 ∪ 随机）→ 普通层不交叉连线 → Boss 前一层全员直连 Boss。
/// </summary>
public static class MapGenerator
{
    public static GeneratedRoguelikeMap Generate(ActMapConfig config, int seed)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        int attempts = Math.Max(1, config.maxGenerateAttempts);
        for (int i = 0; i < attempts; i++)
        {
            int trySeed = seed + i;
            var map = TryGenerateOnce(config, trySeed);
            if (map != null)
                return map;
        }

        throw new InvalidOperationException($"[MapGenerator] {config.name} 在 {attempts} 次尝试后仍无法生成合法地图（seed={seed}）");
    }

    static GeneratedRoguelikeMap TryGenerateOnce(ActMapConfig cfg, int seed)
    {
        var rng = new System.Random(seed);
        var map = new GeneratedRoguelikeMap { actId = cfg.actId, seed = seed };

        if (!BuildTopology(map, cfg, rng))
            return null;

        AssignRoomTypes(map, cfg, rng);

        if (!ValidateMap(map, cfg, out _))
            return null;

        map.RebuildIndex();
        return map;
    }

    static bool BuildTopology(GeneratedRoguelikeMap map, ActMapConfig cfg, System.Random rng)
    {
        int layers = cfg.layerCount;
        int gridW = cfg.mapWidth;
        int bossX = ResolveBossGridX(cfg);

        if (layers < 3 || gridW < 3)
            return false;

        var layerNodes = new List<RoguelikeMapNode>[layers];
        for (int i = 0; i < layers; i++)
            layerNodes[i] = new List<RoguelikeMapNode>();

        int nextId = 0;

        var start = CreateNode(ref nextId, 0, bossX);
        map.nodes.Add(start);
        layerNodes[0].Add(start);

        for (int y = 0; y < layers - 2; y++)
            GenerateNextLayerNodes(map, layerNodes[y], layerNodes[y + 1], y + 1, cfg, rng, ref nextId);

        int bossLayer = layers - 1;
        var boss = CreateNode(ref nextId, bossLayer, bossX);
        map.nodes.Add(boss);
        layerNodes[bossLayer].Add(boss);

        for (int y = 0; y < layers - 2; y++)
        {
            var fromIds = ToIdList(layerNodes[y]);
            var toIds = ToIdList(layerNodes[y + 1]);
            ConnectLayersNonCrossing(map, fromIds, toIds, rng);
        }

        ConnectAllPreBossToBoss(layerNodes[layers - 2], boss);

        PruneUnreachable(map);
        return map.nodes.Count > 0 && map.GetStartNode() != null && map.GetBossNode() != null;
    }

    static int ResolveBossGridX(ActMapConfig cfg)
    {
        int w = cfg.mapWidth;
        int x = cfg.bossGridX >= 0 ? cfg.bossGridX : w / 2;
        if (x < 0 || x >= w)
            x = w / 2;
        return x;
    }

    static RoguelikeMapNode CreateNode(ref int nextId, int layer, int slot)
    {
        return new RoguelikeMapNode
        {
            id = nextId++,
            layer = layer,
            slot = slot,
            roomType = MapRoomType.Normal,
        };
    }

    static List<int> ToIdList(List<RoguelikeMapNode> nodes)
    {
        var ids = new List<int>(nodes.Count);
        for (int i = 0; i < nodes.Count; i++)
            ids.Add(nodes[i].id);
        return ids;
    }

    /// <summary>
    /// 由上一层得到候选集；先算最小出路集 M，再随机本层总数 n∈[|M|, maxNodesPerLayer]，
    /// 从候选\M 中无放回抽 n-|M| 个，本层节点 = M ∪ 随机集（总数不超过 maxNodesPerLayer）。
    /// </summary>
    static void GenerateNextLayerNodes(
        GeneratedRoguelikeMap map,
        List<RoguelikeMapNode> fromLayer,
        List<RoguelikeMapNode> toLayer,
        int nextLayer,
        ActMapConfig cfg,
        System.Random rng,
        ref int nextId)
    {
        if (fromLayer == null || fromLayer.Count == 0)
            return;

        int gridW = cfg.mapWidth;
        var candidates = BuildCandidateSlots(fromLayer, gridW);
        if (candidates.Count == 0)
            return;

        var minSet = BuildMinimumOutletSet(fromLayer, candidates, gridW);

        int minCount = minSet.Count;
        int upperExclusive = cfg.maxNodesPerLayer + 1;
        int targetCount = minCount;
        bool rolledN = upperExclusive > minCount;
        if (rolledN)
            targetCount = rng.Next(minCount, upperExclusive);

        int extraCount = targetCount - minCount;
        var randomSet = BuildRandomPickSetExcludingMin(candidates, minSet, extraCount, rng);

        var finalXs = new HashSet<int>(minSet);
        foreach (int x in randomSet)
            finalXs.Add(x);

        if (cfg.debugLayerNodeSelection)
            LogLayerNodeSelectionDebug(map, cfg, nextLayer, fromLayer.Count, candidates, minSet, minCount,
                upperExclusive, rolledN, targetCount, extraCount, randomSet, finalXs);

        var ordered = new List<int>(finalXs);
        ordered.Sort();

        for (int i = 0; i < ordered.Count; i++)
        {
            int x = ordered[i];
            if (NodeExistsAt(toLayer, x))
                continue;
            var node = CreateNode(ref nextId, nextLayer, x);
            map.nodes.Add(node);
            toLayer.Add(node);
        }
    }

    static HashSet<int> BuildCandidateSlots(List<RoguelikeMapNode> fromLayer, int gridW)
    {
        var candidates = new HashSet<int>();
        for (int i = 0; i < fromLayer.Count; i++)
        {
            int px = fromLayer[i].slot;
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = px + dx;
                if (x >= 0 && x < gridW)
                    candidates.Add(x);
            }
        }
        return candidates;
    }

    /// <summary>
    /// 最小出路集 M：上一层每个节点在 {x-1,x,x+1}∩C 中至少有一个落点；允许多个父节点共用同一 x。
    /// 按 slot 从左到右处理，优先选已在 M 中的列（尽量缩小 |M|），否则优先正上方再左右。
    /// </summary>
    static HashSet<int> BuildMinimumOutletSet(List<RoguelikeMapNode> fromLayer, HashSet<int> candidates, int gridW)
    {
        var minSet = new HashSet<int>();
        var sorted = new List<RoguelikeMapNode>(fromLayer);
        sorted.Sort((a, b) => a.slot.CompareTo(b.slot));

        for (int i = 0; i < sorted.Count; i++)
        {
            int pick = PickOutletForParent(sorted[i].slot, gridW, candidates, minSet);
            if (pick >= 0)
                minSet.Add(pick);
        }
        return minSet;
    }

    static int PickOutletForParent(int fromSlot, int gridW, HashSet<int> candidates, HashSet<int> minSet)
    {
        int[] deltas = { 0, -1, 1 };
        for (int pass = 0; pass < 2; pass++)
        {
            bool requireAlreadyInM = pass == 0;
            for (int d = 0; d < deltas.Length; d++)
            {
                int x = fromSlot + deltas[d];
                if (x < 0 || x >= gridW) continue;
                if (!candidates.Contains(x)) continue;
                if (requireAlreadyInM && !minSet.Contains(x)) continue;
                return x;
            }
        }
        return -1;
    }

    /// <summary>
    /// 从候选里去掉最小出路集后，无放回随机抽取 <paramref name="extraCount"/> 个坐标。
    /// </summary>
    static void LogLayerNodeSelectionDebug(
        GeneratedRoguelikeMap map,
        ActMapConfig cfg,
        int nextLayer,
        int fromLayerCount,
        HashSet<int> candidates,
        HashSet<int> minSet,
        int minCount,
        int upperExclusive,
        bool rolledN,
        int targetCount,
        int extraCount,
        HashSet<int> randomSet,
        HashSet<int> finalXs)
    {
        int optionalPoolSize = 0;
        foreach (int x in candidates)
        {
            if (!minSet.Contains(x))
                optionalPoolSize++;
        }

        var sb = new StringBuilder(256);
        sb.Append("[MapGenerator] 选点 layer=").Append(nextLayer);
        sb.Append(" seed=").Append(map.seed);
        sb.Append(" act=").Append(cfg.name);
        sb.Append("\n  上一层节点数=").Append(fromLayerCount);
        sb.Append(" maxNodesPerLayer=").Append(cfg.maxNodesPerLayer);
        sb.Append("\n  |C|=").Append(candidates.Count).Append(" C=[").Append(FormatSlots(candidates)).Append(']');
        sb.Append("\n  |M|=").Append(minCount).Append(" M=[").Append(FormatSlots(minSet)).Append(']');
        if (rolledN)
            sb.Append("\n  n=Random.Next(").Append(minCount).Append(", ").Append(upperExclusive)
                .Append(") → ").Append(targetCount).Append("  (含 ").Append(minCount).Append("..").Append(cfg.maxNodesPerLayer).Append(')');
        else
            sb.Append("\n  n=").Append(targetCount).Append("  (未掷骰: |M|≥max+1 或上界≤|M|，upperExclusive=").Append(upperExclusive).Append(')');
        sb.Append("\n  额外抽取 extra=n-|M|=").Append(extraCount);
        sb.Append("  可选池|C\\M|=").Append(optionalPoolSize);
        sb.Append("  实际抽到=").Append(randomSet.Count);
        sb.Append(" R=[").Append(FormatSlots(randomSet)).Append(']');
        sb.Append("\n  最终 |final|=").Append(finalXs.Count).Append(" slots=[").Append(FormatSlots(finalXs)).Append(']');
        if (finalXs.Count != targetCount)
            sb.Append("  ⚠ 与目标 n 不一致");
        Debug.Log(sb.ToString());
    }

    static string FormatSlots(HashSet<int> slots)
    {
        if (slots == null || slots.Count == 0)
            return string.Empty;
        var list = new List<int>(slots);
        list.Sort();
        return string.Join(",", list);
    }

    static HashSet<int> BuildRandomPickSetExcludingMin(
        HashSet<int> candidates,
        HashSet<int> minSet,
        int extraCount,
        System.Random rng)
    {
        var randomSet = new HashSet<int>();
        if (extraCount <= 0 || candidates.Count == 0)
            return randomSet;

        var pool = new List<int>();
        foreach (int x in candidates)
        {
            if (!minSet.Contains(x))
                pool.Add(x);
        }

        int picks = Math.Min(extraCount, pool.Count);
        for (int i = 0; i < picks; i++)
        {
            int idx = rng.Next(pool.Count);
            randomSet.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return randomSet;
    }

    static bool NodeExistsAt(List<RoguelikeMapNode> layer, int slot)
    {
        for (int i = 0; i < layer.Count; i++)
        {
            if (layer[i].slot == slot)
                return true;
        }
        return false;
    }

    /// <summary>Boss 前一层：每个节点都连到 Boss，不受 |Δslot|≤1 与不交叉规则限制。</summary>
    static void ConnectAllPreBossToBoss(List<RoguelikeMapNode> preBossLayer, RoguelikeMapNode boss)
    {
        if (boss == null || preBossLayer == null)
            return;
        for (int i = 0; i < preBossLayer.Count; i++)
            AddEdge(preBossLayer[i], boss);
    }

    static void AddEdge(RoguelikeMapNode from, RoguelikeMapNode to)
    {
        if (from.id == to.id) return;
        if (!from.nextNodeIds.Contains(to.id))
            from.nextNodeIds.Add(to.id);
        if (!to.prevNodeIds.Contains(from.id))
            to.prevNodeIds.Add(from.id);
    }

    /// <summary>
    /// 连接相邻两层（非 Boss 层）：仅连 slot 相差 ≤1；边不交叉。
    /// </summary>
    static void ConnectLayersNonCrossing(
        GeneratedRoguelikeMap map,
        List<int> fromLayerIds,
        List<int> toLayerIds,
        System.Random rng,
        float extraEdgeChance = 0.2f)
    {
        if (fromLayerIds == null || toLayerIds == null || fromLayerIds.Count == 0 || toLayerIds.Count == 0)
            return;

        var layerEdges = new List<(int fromSlot, int toSlot)>();

        var toNodes = new List<RoguelikeMapNode>(toLayerIds.Count);
        for (int i = 0; i < toLayerIds.Count; i++)
            toNodes.Add(map.nodes[toLayerIds[i]]);
        toNodes.Sort((a, b) => a.slot.CompareTo(b.slot));

        int minParentSlot = 0;
        for (int i = 0; i < toNodes.Count; i++)
        {
            var to = toNodes[i];
            var parent = PickParentForToNode(map, fromLayerIds, to, minParentSlot, layerEdges);
            if (parent != null && TryAddEdgeNonCrossing(parent, to, layerEdges))
                minParentSlot = parent.slot;
        }

        var fromNodes = new List<RoguelikeMapNode>(fromLayerIds.Count);
        for (int i = 0; i < fromLayerIds.Count; i++)
            fromNodes.Add(map.nodes[fromLayerIds[i]]);
        fromNodes.Sort((a, b) => a.slot.CompareTo(b.slot));

        for (int i = 0; i < fromNodes.Count; i++)
        {
            var from = fromNodes[i];
            if (from.nextNodeIds.Count > 0)
                continue;
            TryAddOutgoingToAdjacent(map, toLayerIds, from, layerEdges, preferSameSlot: true);
        }

        if (extraEdgeChance > 0f)
        {
            for (int fi = 0; fi < fromNodes.Count; fi++)
            {
                var from = fromNodes[fi];
                for (int d = -1; d <= 1; d++)
                {
                    if (rng.NextDouble() >= extraEdgeChance)
                        continue;
                    var to = FindNodeOnLayerBySlot(map, toLayerIds, from.slot + d);
                    if (to == null)
                        continue;
                    TryAddEdgeNonCrossing(from, to, layerEdges);
                }
            }
        }
    }

    static RoguelikeMapNode FindNodeOnLayerBySlot(GeneratedRoguelikeMap map, List<int> layerIds, int slot)
    {
        for (int i = 0; i < layerIds.Count; i++)
        {
            var n = map.nodes[layerIds[i]];
            if (n.slot == slot)
                return n;
        }
        return null;
    }

    static bool EdgesCross(int fromSlotA, int toSlotA, int fromSlotB, int toSlotB)
    {
        if (fromSlotA == fromSlotB || toSlotA == toSlotB)
            return false;
        return (fromSlotA < fromSlotB && toSlotA > toSlotB)
            || (fromSlotA > fromSlotB && toSlotA < toSlotB);
    }

    static bool WouldCross(List<(int fromSlot, int toSlot)> edges, int fromSlot, int toSlot)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            var e = edges[i];
            if (EdgesCross(fromSlot, toSlot, e.fromSlot, e.toSlot))
                return true;
        }
        return false;
    }

    static bool TryAddEdgeNonCrossing(RoguelikeMapNode from, RoguelikeMapNode to, List<(int fromSlot, int toSlot)> layerEdges)
    {
        if (Math.Abs(from.slot - to.slot) > 1)
            return false;
        if (WouldCross(layerEdges, from.slot, to.slot))
            return false;
        AddEdge(from, to);
        layerEdges.Add((from.slot, to.slot));
        return true;
    }

    static RoguelikeMapNode PickParentForToNode(
        GeneratedRoguelikeMap map,
        List<int> fromLayerIds,
        RoguelikeMapNode to,
        int minParentSlot,
        List<(int fromSlot, int toSlot)> layerEdges)
    {
        RoguelikeMapNode best = null;
        int bestDist = int.MaxValue;

        void Consider(RoguelikeMapNode from, bool requireMinSlot)
        {
            if (from == null) return;
            if (requireMinSlot && from.slot < minParentSlot) return;
            if (Math.Abs(from.slot - to.slot) > 1) return;
            if (WouldCross(layerEdges, from.slot, to.slot)) return;
            int dist = Math.Abs(from.slot - to.slot);
            if (dist < bestDist || (dist == bestDist && from.slot < best.slot))
            {
                bestDist = dist;
                best = from;
            }
        }

        for (int i = 0; i < fromLayerIds.Count; i++)
            Consider(map.nodes[fromLayerIds[i]], requireMinSlot: true);

        if (best == null)
        {
            for (int i = 0; i < fromLayerIds.Count; i++)
                Consider(map.nodes[fromLayerIds[i]], requireMinSlot: false);
        }

        return best;
    }

    static void TryAddOutgoingToAdjacent(
        GeneratedRoguelikeMap map,
        List<int> toLayerIds,
        RoguelikeMapNode from,
        List<(int fromSlot, int toSlot)> layerEdges,
        bool preferSameSlot)
    {
        int[] order = preferSameSlot
            ? new[] { 0, -1, 1 }
            : new[] { -1, 0, 1 };
        for (int o = 0; o < order.Length; o++)
        {
            var to = FindNodeOnLayerBySlot(map, toLayerIds, from.slot + order[o]);
            if (to == null)
                continue;
            if (TryAddEdgeNonCrossing(from, to, layerEdges))
                return;
        }
    }

    static void PruneUnreachable(GeneratedRoguelikeMap map)
    {
        var start = map.GetStartNode();
        var boss = map.GetBossNode();
        if (start == null || boss == null) return;

        var reachableForward = new HashSet<int>();
        var q = new Queue<int>();
        q.Enqueue(start.id);
        reachableForward.Add(start.id);
        while (q.Count > 0)
        {
            int id = q.Dequeue();
            var n = FindNode(map.nodes, id);
            if (n == null) continue;
            for (int i = 0; i < n.nextNodeIds.Count; i++)
            {
                int nid = n.nextNodeIds[i];
                if (reachableForward.Add(nid))
                    q.Enqueue(nid);
            }
        }

        var reachableBackward = new HashSet<int>();
        q.Clear();
        q.Enqueue(boss.id);
        reachableBackward.Add(boss.id);
        while (q.Count > 0)
        {
            int id = q.Dequeue();
            var n = FindNode(map.nodes, id);
            if (n == null) continue;
            for (int i = 0; i < n.prevNodeIds.Count; i++)
            {
                int pid = n.prevNodeIds[i];
                if (reachableBackward.Add(pid))
                    q.Enqueue(pid);
            }
        }

        var keep = new HashSet<int>();
        foreach (int id in reachableForward)
        {
            if (reachableBackward.Contains(id))
                keep.Add(id);
        }

        for (int i = map.nodes.Count - 1; i >= 0; i--)
        {
            if (!keep.Contains(map.nodes[i].id))
                map.nodes.RemoveAt(i);
        }

        var keepIds = new HashSet<int>(keep);
        foreach (var n in map.nodes)
        {
            n.nextNodeIds.RemoveAll(id => !keepIds.Contains(id));
            n.prevNodeIds.RemoveAll(id => !keepIds.Contains(id));
        }
    }

    static RoguelikeMapNode FindNode(List<RoguelikeMapNode> nodes, int id)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == id)
                return nodes[i];
        }
        return null;
    }

    static void AssignRoomTypes(GeneratedRoguelikeMap map, ActMapConfig cfg, System.Random rng)
    {
        var start = map.GetStartNode();
        if (start != null)
            start.roomType = cfg.startRoomType;

        var boss = map.GetBossNode();
        if (boss != null)
            boss.roomType = MapRoomType.Boss;

        int bossLayer = boss != null ? boss.layer : cfg.layerCount - 1;
        int preBossLayer = bossLayer - 1;
        if (preBossLayer >= 1)
        {
            var preBossNodes = map.GetNodesOnLayer(preBossLayer);
            for (int i = 0; i < preBossNodes.Count; i++)
                preBossNodes[i].roomType = cfg.preBossRoomType;
        }

        int noEliteUntil = (int)Math.Floor(cfg.layerCount * cfg.noEliteFirstLayerFraction);

        var assignable = new List<RoguelikeMapNode>();
        for (int i = 0; i < map.nodes.Count; i++)
        {
            var n = map.nodes[i];
            if (n.layer == 0 || n.layer >= bossLayer)
                continue;
            if (n.layer == preBossLayer && cfg.preBossRoomType != MapRoomType.Normal)
                continue;
            assignable.Add(n);
        }

        Shuffle(assignable, rng);

        int elitePlaced = 0;
        int lastEliteLayer = -9999;
        for (int i = 0; i < assignable.Count && elitePlaced < cfg.eliteCombatCount; i++)
        {
            var n = assignable[i];
            if (n.roomType != MapRoomType.Normal) continue;
            if (n.layer < cfg.eliteEarliestLayer || n.layer < noEliteUntil) continue;
            if (n.layer - lastEliteLayer < cfg.eliteMinLayerSpacing) continue;
            n.roomType = MapRoomType.Elite;
            elitePlaced++;
            lastEliteLayer = n.layer;
        }

        PlaceByQuota(assignable, MapRoomType.Shop, cfg.shopCount, rng, (n) =>
            n.layer >= cfg.shopEarliestLayer && n.layer <= cfg.shopLatestLayer);

        PlaceByQuota(assignable, MapRoomType.Rest, cfg.restCount, rng, null);

        PlaceByQuota(assignable, MapRoomType.Event, cfg.eventCount, rng, null);
    }

    static void PlaceByQuota(List<RoguelikeMapNode> nodes, MapRoomType type, int quota, System.Random rng, Func<RoguelikeMapNode, bool> filter)
    {
        if (quota <= 0) return;
        Shuffle(nodes, rng);
        int placed = 0;
        for (int i = 0; i < nodes.Count && placed < quota; i++)
        {
            var n = nodes[i];
            if (n.roomType != MapRoomType.Normal) continue;
            if (filter != null && !filter(n)) continue;
            n.roomType = type;
            placed++;
        }
    }

    static void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    static bool ValidateMap(GeneratedRoguelikeMap map, ActMapConfig cfg, out string error)
    {
        var start = map.GetStartNode();
        var boss = map.GetBossNode();
        if (start == null || boss == null)
        {
            error = "缺少起点或 Boss";
            return false;
        }

        foreach (var n in map.nodes)
        {
            if (n.layer < boss.layer && n.nextNodeIds.Count == 0)
            {
                error = $"节点 {n.id} 无出边";
                return false;
            }
        }

        int eliteCount = 0;
        foreach (var n in map.nodes)
            if (n.roomType == MapRoomType.Elite) eliteCount++;

        if (eliteCount < cfg.eliteCombatCount)
        {
            error = $"精英数量不足 {eliteCount}/{cfg.eliteCombatCount}";
            return false;
        }

        error = null;
        return true;
    }
}
