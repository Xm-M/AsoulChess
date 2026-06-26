using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>魂灵之影索敌：主人攻击格内、无本主人残影标记、距主人最近 1 名敌人。</summary>
[Serializable]
public class FindTarget_WisadelShadowUnmarked : IFindTarget
{
    static readonly List<Tile> TileBuffer = new List<Tile>();
    static readonly List<Chess> CandidateBuffer = new List<Chess>();
    static readonly HashSet<Vector2Int> AttackCellsBuffer = new HashSet<Vector2Int>();

    public void FindTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        if (user?.skillController?.context == null)
            return;
        if (!user.skillController.context.TryGet(WisadelKeys.Master, out Chess master)
            || master == null
            || master.IfDeath)
            return;

        if (!WisadelGridHelper.TryCollectAttackCells(master, AttackCellsBuffer))
            return;

        MapManage map = MapManage.instance;
        if (map == null)
            return;

        TileBuffer.Clear();
        foreach (Vector2Int pos in AttackCellsBuffer)
        {
            if (!map.IfInMapRange(pos.x, pos.y))
                continue;
            Tile tile = map.tiles[pos.x, pos.y];
            if (tile != null)
                TileBuffer.Add(tile);
        }

        WisadelGridHelper.CollectChessOverlapOnTiles(master, TileBuffer, CandidateBuffer);

        Chess best = null;
        float bestDist = float.MaxValue;
        Vector2 masterPos = master.transform.position;

        for (int i = 0; i < CandidateBuffer.Count; i++)
        {
            Chess enemy = CandidateBuffer[i];
            if (enemy == null || enemy.IfDeath)
                continue;
            if (Buff_WisadelMark.HasMarkFrom(enemy, master))
                continue;

            float dist = Vector2.Distance(masterPos, enemy.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = enemy;
            }
        }

        if (best != null)
            targets.Add(best);
    }
}
