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
    public string sceneName;
    public string levelName;
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
    [LabelText("最大波数")]
    public CreateZombieType createZombieType;
    [LabelText("下一关")]
    public LevelData nextLevel;

    /// <summary>
    /// 这里往下都是要删掉的东西 暂时放一下
    /// </summary>
    public bool win;
    [SerializeReference]
    public ILevelPreparation preparation;
    [SerializeReference]
    public IZombieSpawner spawner;
    [SerializeReference]
    public ILevelOutcome outcome;
    /// <summary>
    /// 准备阶段要做的事情
    /// </summary>
    public  void PrepareStage()
    {
        win = false;
        preparation?.Prepare(this);
        spawner?.Prepare(this);
        

    }
    /// <summary>
    /// 刚进入游戏时的事件
    /// </summary>
    public  void StartGameStage(){
        spawner?.StartSpawning(this);
    }
    /// <summary>
    /// 游戏结束时的事件
    /// </summary>
    public void OverGameStage(){
        spawner?.OverSpawning(this);
        outcome?.HandleOutcome(spawner.CheckWinCondition());
    }
    /// <summary>
    /// 离开关卡会触发的事件
    /// </summary>
    public  void LeaveStage()
    {
        spawner?.LeaveSpawning(this);
    }
}
/// <summary>
/// 这个接口主要是负责选卡模式，还有控制游戏开始
/// </summary>
public interface ILevelPreparation
{
    public void Prepare(LevelData levelData);
}
/// <summary>
/// 这个接口是僵尸的生成方式
/// </summary>
public interface IZombieSpawner
{

    public void Prepare(LevelData levelData);
    public void StartSpawning(LevelData levelData);
    public void OverSpawning(LevelData levelData);
    public void LeaveSpawning(LevelData levelData);
    bool CheckWinCondition();
}
/// <summary>
/// 这个是胜利后的结算阶段
/// </summary>
public interface ILevelOutcome
{
    public void HandleOutcome(bool win);
}

//[Flags]
public enum CreateZombieType
{
    一类有限制,
    二类有限制,
    一类无限制,
    二类无限制
}

