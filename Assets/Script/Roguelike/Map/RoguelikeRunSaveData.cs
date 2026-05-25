using System;

/// <summary>
/// 单槽肉鸽 Run 存档（仅地图选路，不含战斗中途快照）。
/// </summary>
[Serializable]
public class RoguelikeRunSaveData
{
    public const int CurrentSaveVersion = 1;

    public int saveVersion = CurrentSaveVersion;
    public long saveTimestamp;
    /// <summary>RunMapConfig 的 asset name，读档时解析。</summary>
    public string runConfigName;
    public int pendingNodeId = -1;
    public RoguelikeRunState state = new RoguelikeRunState();
}
