using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// roomtype就是具体的每个房间对吧
/// 其实就是每个场景了
/// 一个roomType对应一个场景
/// </summary>

[CreateAssetMenu(fileName = "NewLevel", menuName = "Message/Level")]
public class LevelData : ScriptableObject
{
    public string sceneName;
    public string levelName;
    public LevelMode levelMode;
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
        preparation.Prepare(this);
        spawner.Prepare(this);
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
        spawner.OverSpawning(this);
        outcome?.HandleOutcome(spawner.CheckWinCondition());
    }
    /// <summary>
    /// 离开关卡会触发的事件
    /// </summary>
    public  void LeaveStage()
    {

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
    bool CheckWinCondition();
}
/// <summary>
/// 这个是胜利后的结算阶段
/// </summary>
public interface ILevelOutcome
{
    public void HandleOutcome(bool win);
}

