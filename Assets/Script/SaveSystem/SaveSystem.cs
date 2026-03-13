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
    /// <summary>当前存档格式版本，结构变更时递增</summary>
    public const int CurrentSaveVersion = 1;

    /// <summary>上次读档是否失败（供 UI 提示用）</summary>
    public static bool LastLoadFailed { get; private set; }
    /// <summary>上次读档失败原因</summary>
    public static string LastLoadError { get; private set; }

    private static string GetSavePath(string levelId)
    {
        string folder = Path.Combine(Application.persistentDataPath, SaveFolder);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, levelId + SaveExtension);
    }

    /// <summary>
    /// 以关卡名为单位生成存档路径（多个 LevelData 可对应同一场景）
    /// </summary>
    private static string GetLevelSaveId(LevelData levelData)
    {
        if (levelData == null || string.IsNullOrEmpty(levelData.levelName)) return null;
        string id = levelData.levelName;
        foreach (char c in Path.GetInvalidFileNameChars())
            id = id.Replace(c, '_');
        return id;
    }

    /// <summary>
    /// 检查指定关卡是否有存档
    /// </summary>
    public static bool HasSaveForLevel(LevelData levelData)
    {
        if (levelData == null) return false;
        string id = GetLevelSaveId(levelData);
        if (string.IsNullOrEmpty(id)) return false;
        return File.Exists(GetSavePath(id));
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

        string levelId = GetLevelSaveId(LevelManage.instance.currentLevel);
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("[SaveSystem] 关卡名为空，无法保存");
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(levelId);
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
    /// 加载指定关卡的存档。失败时删除损坏的存档文件，并设置 LastLoadFailed / LastLoadError
    /// </summary>
    public static GameSaveData LoadSaveData(LevelData levelData)
    {
        LastLoadFailed = false;
        LastLoadError = null;
        if (levelData == null) return null;
        string id = GetLevelSaveId(levelData);
        if (string.IsNullOrEmpty(id)) return null;
        string path = GetSavePath(id);
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null)
            {
                LastLoadFailed = true;
                LastLoadError = "存档解析结果为空";
                DeleteCorruptedSave(path);
                return null;
            }
            // 读档时校验关卡名，避免同场景不同关卡的存档混用
            if (data.levelId != levelData.levelName)
            {
                LastLoadFailed = true;
                LastLoadError = $"存档关卡不匹配: 存档为 [{data.levelId}]，当前 [{levelData.levelName}]";
                Debug.LogWarning($"[SaveSystem] {LastLoadError}");
                return null;
            }
            if (data.saveVersion < 1 || data.saveVersion > CurrentSaveVersion)
            {
                Debug.LogWarning($"[SaveSystem] 存档版本不兼容: {data.saveVersion}，当前 {CurrentSaveVersion}");
            }
            Debug.Log($"[SaveSystem] 读档成功: {path} (v{data.saveVersion})");
            return data;
        }
        catch (Exception e)
        {
            LastLoadFailed = true;
            LastLoadError = e.Message;
            Debug.LogError($"[SaveSystem] 读档失败: {e.Message}\n{e.StackTrace}");
            DeleteCorruptedSave(path);
            return null;
        }
    }

    static void DeleteCorruptedSave(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveSystem] 已删除损坏的存档: {path}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] 删除损坏存档失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除指定关卡的存档（如重新开始时可选）
    /// </summary>
    public static void DeleteSave(LevelData levelData)
    {
        if (levelData == null) return;
        string id = GetLevelSaveId(levelData);
        if (string.IsNullOrEmpty(id)) return;
        string path = GetSavePath(id);
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

        var playerPlants = CapturePlayerPlants();
        var data = new GameSaveData
        {
            saveVersion = CurrentSaveVersion,
            levelId = level.levelName,
            saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            levelData = CaptureLevelProgress(controller),
            playerPlants = playerPlants,
            buffRegistry = BuffDatabase.CaptureRegistry(playerPlants)
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

            var buffs = chess.buffController?.GetSaveData();
            var state = chess.stateController?.currentState?.state?.stateName ?? StateName.IdleState;
            var sc = chess.skillController;
            var skillEffectFired = state == StateName.SkillState && sc != null && sc.skillEffectFiredThisCast;
            var ctxData = sc?.context?.WriteToSaveData();
            SkillStateSaveData skillStateData = null;
            SkillRuntimeSaveData skillRuntimeData = null;
            if (sc?.activeSkill != null)
            {
                skillStateData = new SkillStateSaveData();
                sc.activeSkill.WriteToSaveData(skillStateData);
                if (skillEffectFired && sc.activeSkill is IHasRuntimeInfo hi && hi.Runtime != null)
                {
                    skillRuntimeData = new SkillRuntimeSaveData();
                    hi.Runtime.WriteToSaveData(skillRuntimeData);
                }
            }
            list.Add(new ChessSaveData
            {
                creatorId = chess.propertyController?.creator?.chessName ?? "",
                tileX = tile.mapPos.x,
                tileY = tile.mapPos.y,
                hp = chess.propertyController.GetHp(),
                hpMax = chess.propertyController.GetMaxHp(),
                buffs = buffs ?? new System.Collections.Generic.List<BuffSaveData>(),
                stateName = (int)state,
                skillEffectFired = skillEffectFired,
                skillContextData = ctxData?.keys?.Count > 0 ? ctxData : null,
                skillStateData = !string.IsNullOrEmpty(skillStateData?.skillType) ? skillStateData : null,
                skillRuntimeData = skillRuntimeData
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
