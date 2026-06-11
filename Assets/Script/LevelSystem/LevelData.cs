using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
/// <summary>
/// roomtype就是具体的每个房间对吧
/// 其实就是每个场景了
/// 一个roomType对应一个场景
///  把节点分为：1.进入场景 2.选卡时 3.开始游戏(进入游戏时 比如说羁绊) 4.关卡插件(在特定波数触发) 5.结束事件
///  无旗插件我可以做成有旗子插件；固定出怪我也能做成特定的波数效果 
///  然后僵尸的出怪逻辑:跟sample基本相同 但是有个血量的东西要改一下
/// </summary>

[CreateAssetMenu(fileName = "NewLevel", menuName = "Message/Level")]
public class LevelData : ScriptableObject
{
    public bool ifClearStadge;
    public string sceneName;
    public string levelName;
    public Sprite levelSprit;
    public LevelMode levelMode;

    [SerializeReference]
    [LabelText("进入地图插件"), FoldoutGroup("插件")]
    public List<ILevelPlugin> EnterMapPlugin;
    [SerializeReference]
    [LabelText("准备阶段插件"), FoldoutGroup("插件")]
    public List<ILevelPlugin> PreParePlugin;
    [SerializeReference]
    [LabelText("游戏开始插件"),FoldoutGroup("插件")]
    public List <ILevelPlugin> GameStartPlugin;
    [LabelText("最大波数"),FoldoutGroup("出怪数据")]
    public int MaxWave;//最大波数
    [LabelText("初始僵尸数"),FoldoutGroup("出怪数据")]
    public int n;//初始阳光
    [LabelText("倍率"),FoldoutGroup("出怪数据")]
    public int t;//
    [LabelText("僵尸列表")]
    public List<PropertyCreator> zombieList;
    [LabelText("出怪逻辑")]
    public CreateZombieType createZombieType;
    [LabelText("下一关")]
    public LevelData nextLevel;
    [SerializeReference]
    public ILevelOutcome outcome;

    [LabelText("读档时 Timeline 跳转时间"), FoldoutGroup("存档")]
    [Tooltip("读档时 Director 将跳转到此时间点（秒），跳过 EnterMap/GamePrepare 动画")]
    public float loadSkipToTime = 10f;

    [LabelText("肉鸽难度"), FoldoutGroup("肉鸽")]
    [Tooltip("肉鸽地图按层抽关时匹配用；0 表示未标注，仅在没有同难度候选时作为兜底池参与抽取")]
    public float roguelikeDifficulty;

    [LabelText("肉鸽关卡类型"), FoldoutGroup("肉鸽")]
    [Tooltip("区分普通/精英/事件等；None 表示未标注，仍可按所在 ActMapConfig 池子参与抽取")]
    public RoguelikeLevelKind roguelikeKind;

    /// <summary>
    /// 遍历所有插件（EnterMap + PrePare + GameStart）
    /// </summary>
    public IEnumerable<ILevelPlugin> GetAllPlugins()
    {
        if (EnterMapPlugin != null)
            foreach (var p in EnterMapPlugin) yield return p;
        if (PreParePlugin != null)
            foreach (var p in PreParePlugin) yield return p;
        if (GameStartPlugin != null)
            foreach (var p in GameStartPlugin) yield return p;
    }
  
}
/// <summary>
/// 这个是胜利后的结算阶段
/// </summary>
public interface ILevelOutcome
{
    void HandleOutcome(bool win, Vector3 lastZombiePos);
}

//[Flags]
public enum CreateZombieType
{ 
    一类有限制,
    二类有限制,
    一类无限制,
    二类无限制,
}

/// <summary>肉鸽 <see cref="LevelData"/> 用途标签（与地图节点 <see cref="MapRoomType"/> 对应，便于同池筛选）。</summary>
public enum RoguelikeLevelKind
{
    None = 0,
    Normal,
    Elite,
    Boss,
    Event,
}

