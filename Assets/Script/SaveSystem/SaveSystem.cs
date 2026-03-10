using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档系统：负责关卡存档的保存与读取
/// 仅在 IfGameStart=true 时退出会触发保存
/// 进入关卡时若存在该关卡存档则自动读档
/// </summary>
public static class SaveSystem
{
    private const string SaveFolder = "LevelSaves";
    private const string SaveExtension = ".json";

    private static string GetSavePath(string levelId)
    {
        string folder = Path.Combine(Application.persistentDataPath, SaveFolder);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, levelId + SaveExtension);
    }

    /// <summary>
    /// 检查指定关卡是否有存档
    /// </summary>
    public static bool HasSaveForLevel(LevelData levelData)
    {
        if (levelData == null) return false;
        string path = GetSavePath(levelData.sceneName);
        return File.Exists(path);
    }

    /// <summary>
    /// 保存当前关卡（需在 LeaveState 之前调用，此时游戏状态仍完整）
    /// </summary>
    public static void SaveCurrentLevel()
    {
        if (LevelManage.instance == null || LevelManage.instance.currentLevel == null)
        {
            Debug.LogWarning("[SaveSystem] 无法保存：LevelManage 或 currentLevel 为空");
            return;
        }

        if (!LevelManage.instance.IfGameStart)
        {
            Debug.Log("[SaveSystem] 游戏未开始，不触发保存");
            return;
        }

        GameSaveData data = CaptureCurrentState();
        if (data == null)
        {
            Debug.LogWarning("[SaveSystem] 采集存档数据失败");
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(data.levelId);
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[SaveSystem] 存档成功: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 存档失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载指定关卡的存档
    /// </summary>
    public static GameSaveData LoadSaveData(LevelData levelData)
    {
        if (levelData == null) return null;
        string path = GetSavePath(levelData.sceneName);
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log($"[SaveSystem] 读档成功: {path}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 读档失败: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 删除指定关卡的存档（如重新开始时可选）
    /// </summary>
    public static void DeleteSave(LevelData levelData)
    {
        if (levelData == null) return;
        string path = GetSavePath(levelData.sceneName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] 已删除存档: {path}");
        }
    }

    /// <summary>
    /// 采集当前游戏状态为存档数据
    /// </summary>
    private static GameSaveData CaptureCurrentState()
    {
        var level = LevelManage.instance.currentLevel;
        var controller = LevelManage.instance.currentController;
        if (level == null || controller == null)
            return null;

        var data = new GameSaveData
        {
            levelId = level.sceneName,
            saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            levelData = CaptureLevelProgress(controller),
            playerPlants = CapturePlayerPlants()
        };

        CapturePluginStates(level, data);
        return data;
    }

    private static LevelSaveData CaptureLevelProgress(LevelController controller)
    {
        return new LevelSaveData
        {
            currentWave = controller.GetCurrentWave(),
            t = controller.GetWaveTime(),
            mintime = controller.GetMintime(),
            maxtime = controller.GetMaxtime(),
            gameTime = TimerManage.GameTime
        };
    }

    private static System.Collections.Generic.List<ChessSaveData> CapturePlayerPlants()
    {
        var list = new System.Collections.Generic.List<ChessSaveData>();
        var team = ChessTeamManage.Instance?.GetTeam("Player");
        if (team == null) return list;

        foreach (var chess in team)
        {
            if (chess == null || chess.IfDeath) continue;
            var tile = chess.moveController?.standTile;
            if (tile == null) continue;

            list.Add(new ChessSaveData
            {
                creatorId = chess.propertyController?.creator?.chessName ?? "",
                tileX = tile.mapPos.x,
                tileY = tile.mapPos.y,
                hp = chess.propertyController.GetHp(),
                hpMax = chess.propertyController.GetMaxHp()
            });
        }

        return list;
    }

    /// <summary>
    /// 统一采集所有实现了 ISaveableLevelPlugin 的插件状态
    /// </summary>
    private static void CapturePluginStates(LevelData level, GameSaveData saveData)
    {
        if (level == null || saveData == null) return;
        foreach (var plugin in level.GetAllPlugins())
        {
            if (plugin is ISaveableLevelPlugin saveable)
                saveable.CaptureTo(saveData);
        }
    }
}
