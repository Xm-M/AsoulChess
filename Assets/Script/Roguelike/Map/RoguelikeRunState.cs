using System.Collections.Generic;

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

    /// <summary>开局所选乐队（展示 / Meta）。</summary>
    public string selectedBandId;
    public string selectedBandName;

    public bool runActive;

    public RoguelikeMapNode CurrentNode => currentMap.GetNode(currentNodeId);

    public bool IsNodeCleared(int nodeId) => clearedNodeIds.Contains(nodeId);

    public bool IsNodeVisited(int nodeId) => visitedNodeIds.Contains(nodeId);

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
