using System.Collections.Generic;
using UnityEngine;

/// <summary>生成器输出的单幕地图 DAG（纯数据，不依赖 MonoBehaviour）。</summary>
[System.Serializable]
public class GeneratedRoguelikeMap
{
    public int actId;
    public int seed;
    public List<RoguelikeMapNode> nodes = new List<RoguelikeMapNode>();

    readonly Dictionary<int, RoguelikeMapNode> _byId = new Dictionary<int, RoguelikeMapNode>();

    public void RebuildIndex()
    {
        _byId.Clear();
        for (int i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (n != null)
                _byId[n.id] = n;
        }
    }

    public RoguelikeMapNode GetNode(int id)
    {
        _byId.TryGetValue(id, out var n);
        return n;
    }

    public List<RoguelikeMapNode> GetNodesOnLayer(int layer)
    {
        var list = new List<RoguelikeMapNode>();
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].layer == layer)
                list.Add(nodes[i]);
        }
        list.Sort((a, b) => a.slot.CompareTo(b.slot));
        return list;
    }

    public RoguelikeMapNode GetStartNode()
    {
        var layer0 = GetNodesOnLayer(0);
        return layer0.Count > 0 ? layer0[0] : null;
    }

    public RoguelikeMapNode GetBossNode()
    {
        if (nodes.Count == 0) return null;
        int maxLayer = 0;
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].layer > maxLayer) maxLayer = nodes[i].layer;
        var last = GetNodesOnLayer(maxLayer);
        return last.Count > 0 ? last[0] : null;
    }
}

[System.Serializable]
public class RoguelikeMapNode
{
    public int id;
    public int layer;
    public int slot;
    public MapRoomType roomType;

    /// <summary>出边：可前往的下一层节点 id。</summary>
    public List<int> nextNodeIds = new List<int>();

    /// <summary>入边（生成后填充，便于 UI 与校验）。</summary>
    public List<int> prevNodeIds = new List<int>();
}
