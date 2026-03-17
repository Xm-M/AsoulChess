using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 基于用户名的玩家存档系统。
/// Test 模式下不进行存读档。
/// </summary>
public static class PlayerSaveSystem
{
    const string PlayerSaveFolder = "PlayerSaves";
    const string SaveExtension = ".json";
    public const int CurrentSaveVersion = 1;

    /// <summary>Test 模式下是否跳过存读档</summary>
    static bool SkipSaveLoad => GameManage.instance != null && GameManage.instance.mode == GameMode.Test;

    static string GetSavePath(string username)
    {
        string folder = Path.Combine(Application.persistentDataPath, PlayerSaveFolder);
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        return Path.Combine(folder, SafeFileName(username) + SaveExtension);
    }

    static string SafeFileName(string username)
    {
        if (string.IsNullOrEmpty(username)) return "default";
        foreach (char c in Path.GetInvalidFileNameChars())
            username = username.Replace(c, '_');
        return username;
    }

    /// <summary>枚举 PlayerSaves 文件夹，返回所有存档的用户名（文件名去掉 .json 后缀）。Test 模式下返回空列表。</summary>
    public static List<string> GetAllSaveUsernames()
    {
        if (SkipSaveLoad) return new List<string>();

        string folder = Path.Combine(Application.persistentDataPath, PlayerSaveFolder);
        if (!Directory.Exists(folder)) return new List<string>();

        var usernames = new List<string>();
        try
        {
            foreach (string filePath in Directory.EnumerateFiles(folder, "*" + SaveExtension))
            {
                string filename = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(filename) || !filename.EndsWith(SaveExtension, StringComparison.OrdinalIgnoreCase))
                    continue;
                string username = filename.Substring(0, filename.Length - SaveExtension.Length);
                if (!string.IsNullOrEmpty(username))
                    usernames.Add(username);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerSaveSystem] 枚举存档失败: {e.Message}");
        }
        return usernames;
    }

    public static bool HasSave(string username)
    {
        if (SkipSaveLoad) return false;
        if (string.IsNullOrEmpty(username)) return false;
        return File.Exists(GetSavePath(username));
    }

    public static PlayerSaveData Load(string username)
    {
        if (SkipSaveLoad) return null;
        if (string.IsNullOrEmpty(username)) return null;
        string path = GetSavePath(username);
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<PlayerSaveData>(json);
            if (data != null)
            {
                Migrate(data);
                Debug.Log($"[PlayerSaveSystem] 读档成功: {username}");
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
        if (data == null || string.IsNullOrEmpty(data.username)) return;

        data.saveVersion = CurrentSaveVersion;
        data.saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath(data.username);
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[PlayerSaveSystem] 存档成功: {data.username}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerSaveSystem] 存档失败: {e.Message}");
        }
    }

    public static PlayerSaveData CreateNew(string username)
    {
        if (string.IsNullOrEmpty(username)) username = "default";
        return new PlayerSaveData
        {
            username = username,
            saveVersion = CurrentSaveVersion,
            saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            completedLevelIds = new List<string>(),
            ownedCreatorIds = new List<string>(),
            coins = 0,
            extras = new List<PlayerSaveExtraEntry>()
        };
    }

    public static void Delete(string username)
    {
        if (SkipSaveLoad) return;
        if (string.IsNullOrEmpty(username)) return;
        string path = GetSavePath(username);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[PlayerSaveSystem] 已删除存档: {username}");
        }
    }

    /// <summary>标记关卡已通关</summary>
    public static void MarkLevelCompleted(string username, string levelName)
    {
        if (SkipSaveLoad) return;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(levelName)) return;

        var data = Load(username) ?? CreateNew(username);
        if (!data.completedLevelIds.Contains(levelName))
        {
            data.completedLevelIds.Add(levelName);
            Save(data);
        }
    }

    /// <summary>检查关卡是否已通关</summary>
    public static bool IsLevelCompleted(string username, string levelName)
    {
        if (SkipSaveLoad) return false;
        var data = Load(username);
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
        if (data.saveVersion >= CurrentSaveVersion) return;
        if (data.completedLevelIds == null) data.completedLevelIds = new List<string>();
        if (data.ownedCreatorIds == null) data.ownedCreatorIds = new List<string>();
        if (data.extras == null) data.extras = new List<PlayerSaveExtraEntry>();
        data.saveVersion = CurrentSaveVersion;
    }
}

/// <summary>
/// 当前玩家上下文，供各系统读写玩家数据
/// </summary>
public static class PlayerSaveContext
{
    /// <summary>当前登录的用户名</summary>
    public static string CurrentUsername { get; set; }

    /// <summary>当前玩家存档数据（内存缓存，Load 后需手动更新）</summary>
    public static PlayerSaveData CurrentData { get; set; }

    /// <summary>加载并缓存当前玩家数据</summary>
    public static PlayerSaveData LoadCurrent()
    {
        if (string.IsNullOrEmpty(CurrentUsername))
        {
            CurrentData = null;
            return null;
        }
        CurrentData = PlayerSaveSystem.Load(CurrentUsername) ?? PlayerSaveSystem.CreateNew(CurrentUsername);
        return CurrentData;
    }

    /// <summary>保存当前玩家数据</summary>
    public static void SaveCurrent()
    {
        if (CurrentData != null)
            PlayerSaveSystem.Save(CurrentData);
    }
}
