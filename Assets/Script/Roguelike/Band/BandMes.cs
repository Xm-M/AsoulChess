using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽开局可选「乐队」：背景、名称、简介、初始植物（PropertyCreator）。
/// 配置在 <see cref="RoguelikeBandCatalog"/> 或 <see cref="RunMapConfig.startingBands"/>。
/// </summary>
[System.Serializable]
public class BandMes
{
    [Tooltip("唯一 id，可选；存档 Meta 用")]
    public string bandId;

    public string bandName;

    [TextArea(2, 8)]
    public string bandDescription;

    public Sprite backgroundImage;

    [Tooltip("本 Run 开局获得的初始植物（creator 资产）")]
    public List<PropertyCreator> startingMembers = new List<PropertyCreator>();

    public List<string> GetStartingMemberChessNames()
    {
        var ids = new List<string>();
        if (startingMembers == null)
            return ids;

        for (int i = 0; i < startingMembers.Count; i++)
        {
            var c = startingMembers[i];
            if (c == null || string.IsNullOrEmpty(c.chessName))
                continue;
            if (!ids.Contains(c.chessName))
                ids.Add(c.chessName);
        }
        return ids;
    }

    public bool IsValidForRun(out string error)
    {
        if (string.IsNullOrWhiteSpace(bandName))
        {
            error = "bandName 为空";
            return false;
        }

        if (GetStartingMemberChessNames().Count == 0)
        {
            error = $"乐队 [{bandName}] 未配置 startingMembers";
            return false;
        }

        error = null;
        return true;
    }
}
