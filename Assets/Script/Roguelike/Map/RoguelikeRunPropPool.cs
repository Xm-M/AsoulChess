using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本 Run 内已获得道具（propId）→ 注入 <see cref="GameManage.playerOwnedProps"/> 供战斗道具栏。
/// </summary>
public static class RoguelikeRunPropPool
{
    public static bool AddProp(string propId)
    {
        if (string.IsNullOrEmpty(propId) || RoguelikeRunService.State == null)
            return false;

        var ids = RoguelikeRunService.State.ownedPropIds;
        if (ids == null)
            ids = RoguelikeRunService.State.ownedPropIds = new List<string>();

        if (ids.Contains(propId))
            return false;

        if (ResolveProp(propId) == null)
        {
            Debug.LogWarning($"[RoguelikeRunPropPool] 未找到道具 propId: {propId}");
            return false;
        }

        ids.Add(propId);
        RoguelikeRunService.SaveRun();
        return true;
    }

    /// <summary>进肉鸽战斗关前：用本 Run 道具列表覆盖 GameManage.playerOwnedProps。</summary>
    public static void ApplyRunPropsToGame()
    {
        if (GameManage.instance == null || RoguelikeRunService.State == null)
            return;

        GameManage.instance.playerOwnedProps = ResolveProps(RoguelikeRunService.State.ownedPropIds);
    }

    /// <summary>Run 结束或放弃后恢复主线存档道具池。</summary>
    public static void RestoreMainlinePropsFromPlayerSave()
    {
        PlayerSaveContext.ApplyPlayerPropsToGame();
    }

    public static List<PropItemData> ResolveProps(List<string> propIds)
    {
        var result = new List<PropItemData>();
        if (propIds == null || propIds.Count == 0)
            return result;

        for (int i = 0; i < propIds.Count; i++)
        {
            var p = ResolveProp(propIds[i]);
            if (p != null)
                result.Add(p);
        }
        return result;
    }

    public static PropItemData ResolveProp(string propId)
    {
        if (string.IsNullOrEmpty(propId) || GameManage.instance?.allProps == null)
            return null;

        for (int i = 0; i < GameManage.instance.allProps.Count; i++)
        {
            var p = GameManage.instance.allProps[i];
            if (p != null && p.GetPropId() == propId)
                return p;
        }
        return null;
    }
}
