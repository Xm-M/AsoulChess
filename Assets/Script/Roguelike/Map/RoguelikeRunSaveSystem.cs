using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 肉鸽单槽 Run 存档：persistentDataPath/RoguelikeRuns/active.json
/// </summary>
public static class RoguelikeRunSaveSystem
{
    const string Folder = "RoguelikeRuns";
    const string ActiveFileName = "active";
    const string Extension = ".json";

    static bool SkipSaveLoad => GameManage.instance != null && GameManage.instance.mode == GameMode.Test;

    static string GetActivePath()
    {
        string dir = Path.Combine(Application.persistentDataPath, Folder);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return Path.Combine(dir, ActiveFileName + Extension);
    }

    public static bool HasActiveRunSave()
    {
        if (SkipSaveLoad) return false;
        return File.Exists(GetActivePath());
    }

    public static void Save(RoguelikeRunSaveData data) => TrySave(data);

    public static bool TrySave(RoguelikeRunSaveData data)
    {
        if (data == null)
            return false;

        if (SkipSaveLoad)
        {
            Debug.LogWarning("[RoguelikeRunSaveSystem] Test 模式跳过肉鸽 Run 存档（active.json 不会写入）");
            return false;
        }

        data.saveVersion = RoguelikeRunSaveData.CurrentSaveVersion;
        data.saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetActivePath(), json);
            Debug.Log("[RoguelikeRunSaveSystem] 存档成功");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoguelikeRunSaveSystem] 存档失败: {e.Message}");
            return false;
        }
    }

    public static bool TryLoad(out RoguelikeRunSaveData data)
    {
        data = null;
        if (SkipSaveLoad)
            return false;

        string path = GetActivePath();
        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<RoguelikeRunSaveData>(json);
            if (data?.state == null)
            {
                Debug.LogWarning("[RoguelikeRunSaveSystem] 存档解析为空");
                DeleteActiveSave();
                return false;
            }

            if (data.state.currentMap == null)
                data.state.currentMap = new GeneratedRoguelikeMap();
            if (data.state.visitedNodeIds == null)
                data.state.visitedNodeIds = new System.Collections.Generic.List<int>();
            if (data.state.clearedNodeIds == null)
                data.state.clearedNodeIds = new System.Collections.Generic.List<int>();
            if (data.state.ownedPlantCreatorIds == null)
                data.state.ownedPlantCreatorIds = new System.Collections.Generic.List<string>();
            if (data.state.ownedPropIds == null)
                data.state.ownedPropIds = new System.Collections.Generic.List<string>();
            if (data.state.restUsedNodeIds == null)
                data.state.restUsedNodeIds = new System.Collections.Generic.List<int>();

            data.state.currentMap.RebuildIndex();
            Debug.Log("[RoguelikeRunSaveSystem] 读档成功");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[RoguelikeRunSaveSystem] 读档失败: {e.Message}");
            DeleteActiveSave();
            return false;
        }
    }

    public static void DeleteActiveSave()
    {
        if (SkipSaveLoad)
            return;
        string path = GetActivePath();
        if (!File.Exists(path))
            return;
        try
        {
            File.Delete(path);
            Debug.Log("[RoguelikeRunSaveSystem] 已删除 active Run 存档");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RoguelikeRunSaveSystem] 删除失败: {e.Message}");
        }
    }

    /// <summary>按 asset name 解析 RunMapConfig；fallback 为 StartUI 上配置的同名校验。</summary>
    public static RunMapConfig ResolveRunConfig(string runConfigName, RunMapConfig fallback)
    {
        if (!string.IsNullOrEmpty(runConfigName))
        {
            if (fallback != null && fallback.name == runConfigName)
                return fallback;

            var all = Resources.LoadAll<RunMapConfig>("");
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].name == runConfigName)
                    return all[i];
            }
        }

        return fallback;
    }
}
