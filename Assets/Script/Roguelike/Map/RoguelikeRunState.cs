using System.Collections.Generic;
using UnityEngine;

/// <summary>一局肉鸽的运行时状态（可序列化进存档）。</summary>
[System.Serializable]
public class RoguelikeRunState
{
    public int runSeed;
    public int currentActIndex;
    public int currentNodeId = -1;

    public GeneratedRoguelikeMap currentMap = new GeneratedRoguelikeMap();

    /// <summary>已抵达过的节点（含当前）。</summary>
    public List<int> visitedNodeIds = new List<int>();

    /// <summary>已通关的节点（战斗胜利 / 非战斗房间已处理）。</summary>
    public List<int> clearedNodeIds = new List<int>();

    /// <summary>本 Run 已获得的植物（PropertyCreator.chessName），进战斗时写入 GameManage.playerOwnedCreators。</summary>
    public List<string> ownedPlantCreatorIds = new List<string>();

    /// <summary>本 Run 已获得的道具（PropItemData.propId），进战斗时写入 GameManage.playerOwnedProps。</summary>
    public List<string> ownedPropIds = new List<string>();

    /// <summary>开局所选乐队（展示 / Meta）。</summary>
    public string selectedBandId;
    public string selectedBandName;

    /// <summary>本 Run 持有金币（仅 Run 内有效，不写 PlayerSaveData）。</summary>
    public int runGold;

    /// <summary>当前商店节点 id；无商店或已离开时为 -1。</summary>
    public int activeShopNodeId = -1;

    /// <summary>当前商店商品（与 activeShopNodeId 对应，进档可恢复）。</summary>
    public List<RoguelikeShopOffer> shopOffers = new List<RoguelikeShopOffer>();

    /// <summary>已在休息房成功选择过一项的节点 id（每节点仅能选一次）。</summary>
    public List<int> restUsedNodeIds = new List<int>();

    /// <summary>本 Run 拥有的小推车数量；进战斗时 spawn = Min(地图行数, 本值)。</summary>
    public int runLawnMowerCount = 6;

    /// <summary>本 Run 每场战斗 loadout 上限（携带格）；仅肉鸽 PlantsShop.maxCount 读取。</summary>
    public int runLoadoutSlotCount = 10;

    public bool runActive;

    /// <summary>方案 A：实际生成数 = Min(地图行数, runLawnMowerCount)。</summary>
    public int ComputeLawnMowerSpawnCount(int mapRowCount)
    {
        if (mapRowCount <= 0)
            return 0;

        int owned = runLawnMowerCount > 0 ? runLawnMowerCount : 6;
        return owned < mapRowCount ? owned : mapRowCount;
    }

    /// <summary>Clamp 后的本关 loadout 上限。</summary>
    public int GetLoadoutSlotCount(RoguelikeEconomyConfig economy)
    {
        economy ??= RoguelikeRunService.ResolveEconomyConfig();
        int value = runLoadoutSlotCount > 0
            ? runLoadoutSlotCount
            : economy.GetInitialLoadoutSlotCount();
        return Mathf.Clamp(value, economy.GetMinLoadoutSlotCount(), economy.GetMaxLoadoutSlotCount());
    }

    public RoguelikeMapNode CurrentNode => currentMap.GetNode(currentNodeId);

    public bool IsNodeCleared(int nodeId) => clearedNodeIds.Contains(nodeId);

    public bool IsNodeVisited(int nodeId) => visitedNodeIds.Contains(nodeId);

    public bool IsRestChoiceUsed(int nodeId) => restUsedNodeIds != null && restUsedNodeIds.Contains(nodeId);

    public void MarkRestChoiceUsed(int nodeId)
    {
        if (restUsedNodeIds == null)
            restUsedNodeIds = new List<int>();
        if (!restUsedNodeIds.Contains(nodeId))
            restUsedNodeIds.Add(nodeId);
    }

    public void MarkVisited(int nodeId)
    {
        if (!visitedNodeIds.Contains(nodeId))
            visitedNodeIds.Add(nodeId);
    }

    public void MarkCleared(int nodeId)
    {
        if (!clearedNodeIds.Contains(nodeId))
            clearedNodeIds.Add(nodeId);
    }

    /// <summary>从当前节点出发，玩家可选的下一层节点（须在出边且上一格已 cleared，或当前为起点）。</summary>
    public List<RoguelikeMapNode> GetSelectableNextNodes()
    {
        var result = new List<RoguelikeMapNode>();
        var cur = CurrentNode;
        if (cur == null) return result;

        bool canLeave = cur.roomType == MapRoomType.Start || IsNodeCleared(currentNodeId);
        if (!canLeave) return result;

        for (int i = 0; i < cur.nextNodeIds.Count; i++)
        {
            var next = currentMap.GetNode(cur.nextNodeIds[i]);
            if (next != null)
                result.Add(next);
        }
        return result;
    }

    public bool CanSelectNode(int nodeId)
    {
        foreach (var n in GetSelectableNextNodes())
        {
            if (n.id == nodeId)
                return true;
        }
        return false;
    }
}
