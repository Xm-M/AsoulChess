using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 读档时的上下文，供各插件判断是否处于读档模式
/// </summary>
public static class SaveLoadContext
{
    /// <summary>
    /// 是否从存档恢复（进入关卡时检测到有存档则为 true）
    /// </summary>
    public static bool IsLoadFromSave { get; set; }

    /// <summary>
    /// 当前要恢复的存档数据，读档模式下非空
    /// </summary>
    public static GameSaveData CurrentSaveData { get; set; }

    /// <summary>
    /// 读档流程是否已执行（防止重复执行）
    /// </summary>
    public static bool LoadFlowExecuted { get; set; }

    /// <summary>
    /// 清除读档上下文（离开关卡时调用）
    /// </summary>
    public static void Clear()
    {
        IsLoadFromSave = false;
        CurrentSaveData = null;
        LoadFlowExecuted = false;
    }
}
