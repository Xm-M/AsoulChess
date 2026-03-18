using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 单一玩家存档系统。Test 模式下不进行存读档。
/// </summary>
public static class PlayerSaveSystem
{
    const string PlayerSaveFolder = "PlayerSaves";
    const string SaveExtension = ".json";
    public const string DefaultSaveName = "player";
    public const int CurrentSaveVersion = 2;

    /// <summary>Test 模式下是否跳过存读档</summary>
    static bool SkipSaveLoad => GameManage.instance != null && GameManage.instance.mode == GameMode.Test;

    static string GetSavePath()
    {
        string folder = Path.Combine(Application.persistentDataPath, PlayerSaveFolder);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, DefaultSaveName + SaveExtension);
    }

    /// <summary>是否存在存档</summary>
    public static bool HasSave()
    {
        if (SkipSaveLoad) return false;
        return File.Exists(GetSavePath());
    }

    /// <summary>加载存档，不存在则返回 null</summary>
    public static PlayerSaveData Load()
    {
        if (SkipSaveLoad) return null;
        string path = GetSavePath();
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<PlayerSaveData>(json);
            if (data != null)
            {
                Migrate(data);
                Debug.Log("[PlayerSaveSystem] 读档成功");
            }
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerSaveSystem] 读档失败: {e.Message}");
            return null;
        }
    }

    public static void Save(PlayerSaveData data)
    {
        if (SkipSaveLoad) return;
        if (data == null) return;

        data.saveVersion = CurrentSaveVersion;
        data.saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (string.IsNullOrEmpty(data.username)) data.username = DefaultSaveName;

        string json = JsonUtility.ToJson(data, true);
        try
        {
            File.WriteAllText(GetSavePath(), json);
            Debug.Log("[PlayerSaveSystem] 存档成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerSaveSystem] 存档失败: {e.Message}");
        }
    }

    /// <summary>创建新存档</summary>
    public static PlayerSaveData CreateNew()
    {
        return new PlayerSaveData
        {
            username = DefaultSaveName,
            saveVersion = CurrentSaveVersion,
            saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            completedLevelIds = new List<string>(),
            ownedCreatorIds = new List<string>(),
            coins = 0,
            shopUnlocked = false,
            bgmVolume = 1f,
            sfxVolume = 1f,
            fullscreen = true,
            difficultyLevel = 0,
            extras = new List<PlayerSaveExtraEntry>()
        };
    }

    /// <summary>删除存档</summary>
    public static void Delete()
    {
        if (SkipSaveLoad) return;
        string path = GetSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("[PlayerSaveSystem] 已删除存档");
        }
    }

    /// <summary>标记关卡已通关</summary>
    public static void MarkLevelCompleted(string levelName)
    {
        if (SkipSaveLoad) return;
        if (string.IsNullOrEmpty(levelName)) return;

        var data = Load() ?? CreateNew();
        if (!data.completedLevelIds.Contains(levelName))
        {
            data.completedLevelIds.Add(levelName);
            Save(data);
        }
    }

    /// <summary>检查关卡是否已通关</summary>
    public static bool IsLevelCompleted(string levelName)
    {
        if (SkipSaveLoad) return false;
        var data = Load();
        return data?.completedLevelIds.Contains(levelName) ?? false;
    }

    /// <summary>扩展数据读写</summary>
    public static T GetExtra<T>(PlayerSaveData data, string key, T defaultValue)
    {
        if (data?.extras == null) return defaultValue;
        var e = data.extras.Find(x => x.key == key);
        if (e == null) return defaultValue;
        try
        {
            if (typeof(T) == typeof(int) && int.TryParse(e.value, out int vi)) return (T)(object)vi;
            if (typeof(T) == typeof(float) && float.TryParse(e.value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float vf)) return (T)(object)vf;
            if (typeof(T) == typeof(string)) return (T)(object)(e.value ?? "");
            if (typeof(T) == typeof(bool) && bool.TryParse(e.value, out bool vb)) return (T)(object)vb;
        }
        catch { }
        return defaultValue;
    }

    public static void SetExtra<T>(PlayerSaveData data, string key, T value)
    {
        if (data == null) return;
        if (data.extras == null) data.extras = new List<PlayerSaveExtraEntry>();
        var e = data.extras.Find(x => x.key == key);
        if (e == null)
        {
            e = new PlayerSaveExtraEntry { key = key, type = typeof(T).Name, value = value?.ToString() ?? "" };
            data.extras.Add(e);
        }
        else
        {
            e.value = value?.ToString() ?? "";
        }
    }

    static void Migrate(PlayerSaveData data)
    {
        if (data.completedLevelIds == null) data.completedLevelIds = new List<string>();
        if (data.ownedCreatorIds == null) data.ownedCreatorIds = new List<string>();
        if (data.extras == null) data.extras = new List<PlayerSaveExtraEntry>();
        if (data.saveVersion < 2)
        {
            if (data.bgmVolume <= 0) data.bgmVolume = 1f;
            if (data.sfxVolume <= 0) data.sfxVolume = 1f;
            if (!data.fullscreen && data.screenWidth == 0) data.fullscreen = true;
        }
        data.saveVersion = CurrentSaveVersion;
    }
}

/// <summary>
/// 当前玩家上下文，供各系统读写玩家数据（单一存档）
/// </summary>
public static class PlayerSaveContext
{
    /// <summary>当前玩家存档数据（内存缓存）</summary>
    public static PlayerSaveData CurrentData { get; set; }

    /// <summary>加载并缓存当前玩家数据（有则读档，无则新建并保存）</summary>
    public static PlayerSaveData LoadCurrent()
    {
        CurrentData = PlayerSaveSystem.Load();
        if (CurrentData == null)
        {
            CurrentData = PlayerSaveSystem.CreateNew();
            PlayerSaveSystem.Save(CurrentData);
        }
        return CurrentData;
    }

    /// <summary>保存当前玩家数据</summary>
    public static void SaveCurrent()
    {
        if (CurrentData != null)
            PlayerSaveSystem.Save(CurrentData);
    }

    /// <summary>将存档中的设置应用到游戏（音量、分辨率等），读档后调用</summary>
    public static void ApplySettingsToGame()
    {
        var data = CurrentData;
        if (data == null) return;
        AudioManage.ChangeBGMValue(Mathf.Clamp01(data.bgmVolume));
        AudioManage.ChangeSoundEffect(Mathf.Clamp01(data.sfxVolume));
        // 分辨率：screenWidth/screenHeight 非 0 时应用，后续实现
        if (data.screenWidth > 0 && data.screenHeight > 0)
        {
            Screen.SetResolution(data.screenWidth, data.screenHeight, data.fullscreen);
        }
        else
        {
            Screen.fullScreen = data.fullscreen;
        }
    }
}
