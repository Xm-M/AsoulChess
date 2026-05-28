using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本 Run 内可用植物（creator.chessName）→ 注入 <see cref="GameManage.playerOwnedCreators"/> 供选卡/商店。
/// </summary>
public static class RoguelikeRunPlantPool
{
    public static void InitializeForNewRun(RunMapConfig config)
    {
        InitializeFromChessNames(CollectFallbackStarterIds(config));
    }

    public static void InitializeFromBand(BandMes band)
    {
        if (band == null)
            return;

        var ids = band.GetStartingMemberChessNames();
        if (ids.Count == 0)
        {
            Debug.LogWarning($"[RoguelikeRunPlantPool] 乐队 [{band.bandName}] 无初始成员");
            return;
        }

        InitializeFromChessNames(ids);
    }

    static List<string> CollectFallbackStarterIds(RunMapConfig config)
    {
        var list = new List<string>();
        if (config?.startingPlantCreatorIds != null && config.startingPlantCreatorIds.Count > 0)
        {
            for (int i = 0; i < config.startingPlantCreatorIds.Count; i++)
            {
                string id = config.startingPlantCreatorIds[i];
                if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                    list.Add(id);
            }
        }
        else
        {
            var player = PlayerSaveContext.CurrentData ?? PlayerSaveContext.LoadCurrent();
            if (player?.ownedCreatorIds != null)
            {
                for (int i = 0; i < player.ownedCreatorIds.Count; i++)
                {
                    string id = player.ownedCreatorIds[i];
                    if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                        list.Add(id);
                }
            }
        }

        if (list.Count == 0)
            list.Add("广井菊里");
        return list;
    }

    static void InitializeFromChessNames(List<string> ids)
    {
        if (RoguelikeRunService.State == null)
            return;

        RoguelikeRunService.State.ownedPlantCreatorIds = new List<string>(ids);
    }

    public static bool AddPlant(string creatorChessName)
    {
        if (string.IsNullOrEmpty(creatorChessName) || RoguelikeRunService.State == null)
            return false;

        var ids = RoguelikeRunService.State.ownedPlantCreatorIds;
        if (ids == null)
            ids = RoguelikeRunService.State.ownedPlantCreatorIds = new List<string>();

        if (ids.Contains(creatorChessName))
            return false;

        ids.Add(creatorChessName);
        RoguelikeRunService.SaveRun();
        return true;
    }

    /// <summary>进肉鸽战斗关前：用本 Run 植物列表覆盖 GameManage.playerOwnedCreators。</summary>
    public static void ApplyRunPlantsToGame()
    {
        if (GameManage.instance == null || RoguelikeRunService.State == null)
            return;

        var creators = ResolveCreators(RoguelikeRunService.State.ownedPlantCreatorIds);
        GameManage.instance.playerOwnedCreators = creators;
    }

    /// <summary>Run 结束或放弃后恢复主线存档植物池。</summary>
    public static void RestoreMainlinePlantsFromPlayerSave()
    {
        PlayerSaveContext.ApplyPlayerChessToGame();
    }

    public static List<PropertyCreator> ResolveCreators(List<string> creatorIds)
    {
        var result = new List<PropertyCreator>();
        if (creatorIds == null || creatorIds.Count == 0)
            return result;

        for (int i = 0; i < creatorIds.Count; i++)
        {
            var c = ResolveCreator(creatorIds[i]);
            if (c != null)
                result.Add(c);
        }
        return result;
    }

    static PropertyCreator ResolveCreator(string chessName)
    {
        if (string.IsNullOrEmpty(chessName) || GameManage.instance?.allChess == null)
            return null;
        for (int i = 0; i < GameManage.instance.allChess.Count; i++)
        {
            var c = GameManage.instance.allChess[i];
            if (c != null && c.chessName == chessName)
                return c;
        }
        Debug.LogWarning($"[RoguelikeRunPlantPool] 未找到植物 creator: {chessName}");
        return null;
    }
}
