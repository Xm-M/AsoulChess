using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 单幕（Act）肉鸽地图生成参数：拓扑、房间配额、关卡池、节奏规则。
/// 进入战斗节点时从 <see cref="normalLevelPool"/> / <see cref="eliteLevelPool"/> 抽取 <see cref="LevelData"/>。
/// </summary>
[CreateAssetMenu(fileName = "ActMapConfig", menuName = "Roguelike/Act Map Config")]
public class ActMapConfig : ScriptableObject
{
    [FoldoutGroup("幕信息"), LabelText("Act Id")]
    public int actId = 1;

    [FoldoutGroup("幕信息")]
    public string displayName = "Act 1";

    [FoldoutGroup("幕信息"), Tooltip("策划参考，不参与生成")]
    public int targetMinutes = 35;

    [FoldoutGroup("拓扑"), Min(3)]
    public int layerCount = 15;

    [FoldoutGroup("拓扑"), Min(3), Tooltip("网格列数（杀戮尖塔为 7）；节点 slot 即列坐标 0..mapWidth-1")]
    public int mapWidth = 7;

    [FoldoutGroup("拓扑"), Min(1), Tooltip("除起点/Boss 外，单层最多节点数（杀戮尖塔约 6）")]
    public int maxNodesPerLayer = 6;

    [FoldoutGroup("拓扑"), Tooltip("Boss 列坐标；-1 表示 mapWidth/2")]
    public int bossGridX = -1;

    [FoldoutGroup("拓扑"), Min(1), HideInInspector]
    public int minWidthPerLayer = 3;

    [FoldoutGroup("拓扑"), Min(1), HideInInspector]
    public int maxWidthPerLayer = 4;

    [FoldoutGroup("房间配额"), Min(0)]
    public int normalCombatCount = 4;

    [FoldoutGroup("房间配额"), Min(0)]
    public int eliteCombatCount = 1;

    [FoldoutGroup("房间配额"), Min(0)]
    public int restCount = 2;

    [FoldoutGroup("房间配额"), Min(0)]
    public int shopCount = 1;

    [FoldoutGroup("房间配额"), Min(0)]
    public int eventCount = 2;

    [FoldoutGroup("Boss")]
    public LevelData bossLevel;

    [FoldoutGroup("Boss"), Min(1)]
    public int bossFlagCount = 3;

    [FoldoutGroup("关卡池")]
    public List<LevelData> normalLevelPool = new List<LevelData>();

    [FoldoutGroup("关卡池")]
    public List<LevelData> eliteLevelPool = new List<LevelData>();

    [FoldoutGroup("关卡池"), Tooltip("事件/小游戏关卡（坚果保龄球、锤僵尸等）")]
    public List<LevelData> eventLevelPool = new List<LevelData>();

    [FoldoutGroup("关卡池-难度匹配"), Tooltip("layer 0 / 起点侧参考难度（如 1.0）")]
    public float roguelikeDifficultyAtStart = 1f;

    [FoldoutGroup("关卡池-难度匹配"), Tooltip("Boss 前一层参考难度（如 3.0）；中间层线性插值")]
    public float roguelikeDifficultyAtPreBoss = 3f;

    [FoldoutGroup("关卡池-难度匹配"), Min(0.01f), Tooltip("目标难度 ± 半宽；在此区间内的 LevelData 优先入选")]
    public float roguelikeDifficultyMatchHalfRange = 0.5f;

    [FoldoutGroup("关卡池-难度匹配"), Tooltip("精英节点在层目标难度上额外加算")]
    public float roguelikeEliteDifficultyBonus = 0.5f;

    [FoldoutGroup("生成规则")]
    public MapRoomType startRoomType = MapRoomType.Start;

    [FoldoutGroup("生成规则"), Min(1)]
    public int eliteEarliestLayer = 3;

    [FoldoutGroup("生成规则"), Min(1)]
    public int eliteMinLayerSpacing = 3;

    [FoldoutGroup("生成规则")]
    public MapRoomType preBossRoomType = MapRoomType.Rest;

    [FoldoutGroup("生成规则"), Min(1)]
    public int shopEarliestLayer = 4;

    [FoldoutGroup("生成规则"), Min(1)]
    public int shopLatestLayer = 999;

    [FoldoutGroup("生成规则"), Range(0f, 0.5f), Tooltip("前若干比例层禁止精英（0.25 = 前 25%）")]
    public float noEliteFirstLayerFraction = 0.25f;

    [FoldoutGroup("生成规则"), Min(1)]
    public int maxGenerateAttempts = 32;

    [FoldoutGroup("生成规则"), Tooltip("开启后在 Console 输出每层选点 n 的随机过程（候选/M/随机/最终）")]
    public bool debugLayerNodeSelection;

    void OnValidate()
    {
        if (maxWidthPerLayer < minWidthPerLayer)
            maxWidthPerLayer = minWidthPerLayer;
        if (!Validate(out var err))
            Debug.LogWarning($"[ActMapConfig] {name}: {err}", this);
    }

    public bool Validate(out string error)
    {
        if (layerCount < 3)
        {
            error = "layerCount 至少为 3（起点 / 中间 / Boss）";
            return false;
        }
        if (mapWidth < 3)
        {
            error = "mapWidth 至少为 3";
            return false;
        }
        if (maxNodesPerLayer < 1)
        {
            error = "maxNodesPerLayer 至少为 1";
            return false;
        }
        if (bossGridX >= 0 && bossGridX >= mapWidth)
        {
            error = "bossGridX 须在 [0, mapWidth) 或 -1（居中）";
            return false;
        }
        if (eliteCombatCount < 0 || restCount < 0 || shopCount < 0 || eventCount < 0)
        {
            error = "配额不能为负";
            return false;
        }
        if (bossLevel == null && normalLevelPool.Count == 0 && eliteLevelPool.Count == 0)
        {
            error = "未配置 bossLevel 且关卡池为空，战斗节点将无法抽关";
            return false;
        }
        error = null;
        return true;
    }
}
