using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 准备阶段打开植物商店。冒险关每次完整选卡；生存关 13 格（前 10 核心锁 / 后 3 机动可换）。
/// </summary>
public class PreParePlugun_ShowPlantShop : ISaveableLevelPlugin, IRoundEndPlugin
{
    public int baseSunLight = 50;
    public List<PropertyCreator> cards;

    /// <summary>生存模式：最近一轮开战时的手牌（内存缓存，读档时从 plantsShopData 同步）。</summary>
    PlantsShopSaveData lockedHand;

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData == null) return;

        SyncLockedHandFromSaveIfNeeded();
        int sun = SunLightPanel.instance != null ? SunLightPanel.instance.sunLight : 0;

        // 生存模式：优先写当前商店手牌；否则写缓存并刷新阳光
        if (IsSurvivalLevel() && HasLockedCards())
        {
            var shop = UIManage.GetView<PlantsShop>();
            if (shop != null && shop.currentShopIcons != null && shop.currentShopIcons.Count > 0)
            {
                EnsureLockedHandFromShop();
                if (lockedHand != null)
                    lockedHand.sunLight = sun;
            }
            else if (lockedHand != null)
            {
                lockedHand.sunLight = sun;
            }

            if (lockedHand != null)
            {
                saveData.plantsShopData = CloneHand(lockedHand);
                return;
            }
        }

        var liveShop = UIManage.GetView<PlantsShop>();
        if (liveShop == null) return;

        var captured = new PlantsShopSaveData
        {
            selectedCreatorIds = new List<string>(),
            sunLight = sun
        };
        foreach (var icon in liveShop.currentShopIcons)
        {
            if (icon != null && icon.good != null)
                captured.selectedCreatorIds.Add(icon.good.chessName);
        }

        if (captured.selectedCreatorIds.Count == 0)
            return;

        saveData.plantsShopData = captured;
        if (IsSurvivalLevel())
            lockedHand = CloneHand(captured);
    }

    public void StadgeEffect(LevelController levelController)
    {
        if (!IsSurvival(levelController))
        {
            RunAdventurePrepare(levelController);
            return;
        }

        SyncLockedHandFromSaveIfNeeded();
        RunSurvivalPrepare(levelController);
    }

    /// <summary>清空选卡内存缓存（重新开始 / 无档进关时调用，避免 LevelData 插件残留）。</summary>
    public void ClearLockedHand()
    {
        lockedHand = null;
    }

    /// <summary>生存模式开战时锁定手牌（覆盖写入，支持轮间重选）。</summary>
    public void EnsureLockedHandFromShop()
    {
        var shop = UIManage.GetView<PlantsShop>();
        if (shop == null || shop.currentShopIcons == null || shop.currentShopIcons.Count == 0)
            return;

        lockedHand = new PlantsShopSaveData
        {
            selectedCreatorIds = new List<string>(),
            sunLight = SunLightPanel.instance != null ? SunLightPanel.instance.sunLight : 0
        };
        foreach (var icon in shop.currentShopIcons)
        {
            if (icon != null && icon.good != null)
                lockedHand.selectedCreatorIds.Add(icon.good.chessName);
        }
    }

    public void RoundOverPlugin(LevelController levelController)
    {
        EnsureLockedHandFromShop();
        if (HasLockedCards() && SunLightPanel.instance != null)
            lockedHand.sunLight = SunLightPanel.instance.sunLight;
        UIManage.Close<PlantsShop>();
    }

    public void OverPlugin(LevelController levelController)
    {
        UIManage.Close<PlantsShop>();
    }

    void RunAdventurePrepare(LevelController levelController)
    {
        if (cards != null && cards.Count > 0)
            PlantsShop.OverrideCreators = cards;
        UIManage.Show<PlantsShop>();
        PlantsShop.OverrideCreators = null;
        if (!SaveLoadContext.IsLoadFromSave)
        {
            SunLightPanel.instance.SetSunLight(baseSunLight);
            RunAutoSelectIfFewCards();
        }
    }

    /// <summary>
    /// 生存每轮完整选卡：Timeline 暂停点依赖开战按钮；轮间保留阳光，并预勾上一轮手牌方便改卡。
    /// </summary>
    void RunSurvivalPrepare(LevelController levelController)
    {
        if (cards != null && cards.Count > 0)
            PlantsShop.OverrideCreators = cards;
        UIManage.Show<PlantsShop>();
        PlantsShop.OverrideCreators = null;

        int round = GetRoundIndex(levelController);
        bool firstFreshSelect = round <= 1 && !HasLockedCards() && !SaveLoadContext.IsLoadFromSave;

        if (firstFreshSelect)
            SunLightPanel.instance.SetSunLight(baseSunLight);

        PrefillPreviousHand(levelController);

        if (!SaveLoadContext.IsLoadFromSave)
            RunAutoSelectIfFewCards();
    }

    void PrefillPreviousHand(LevelController levelController)
    {
        // 无档新开局（第1轮）不得用 SO 上残留的 lockedHand
        if (!SaveLoadContext.IsLoadFromSave && GetRoundIndex(levelController) <= 1)
            return;

        var hand = GetLockedHandForDisplay();
        var shop = UIManage.GetView<PlantsShop>();
        if (hand?.selectedCreatorIds == null || hand.selectedCreatorIds.Count == 0 || shop == null)
            return;

        foreach (var id in hand.selectedCreatorIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            foreach (var icon in shop.allSelectIcons)
            {
                if (icon == null || icon.select == null || icon.ifSelect) continue;
                if (icon.select.chessName != id) continue;
                icon.SelectCard();
                break;
            }
        }

        // 轮间/读档：按预填顺序锁定前 10 核心格
        if (shop.currentSelectIcons != null && shop.currentSelectIcons.Count > 0)
            shop.EnableSurvivalCoreLock();
    }

    void RunAutoSelectIfFewCards()
    {
        var shop = UIManage.GetView<PlantsShop>();
        var creators = cards != null && cards.Count > 0
            ? cards
            : (GameManage.instance?.playerOwnedCreators != null && GameManage.instance.playerOwnedCreators.Count > 0
                ? GameManage.instance.playerOwnedCreators
                : GameManage.instance?.allChess);
        if (shop == null || creators == null || creators.Count == 0 || creators.Count >= shop.maxCount)
            return;

        // 生存：卡池不足核心格数时不能自动开战
        if (IsSurvivalLevel() && creators.Count < PlantsShop.SurvivalCoreSlotCount)
            return;

        foreach (var icon in shop.allSelectIcons)
        {
            if (icon != null) shop.AddSelection(icon);
        }
        shop.GameStart();
        shop.GetComponent<Animator>().Play("end");
        EnsureLockedHandFromShop();
    }

    PlantsShopSaveData GetLockedHandForDisplay()
    {
        SyncLockedHandFromSaveIfNeeded();
        return HasLockedCards() ? lockedHand : null;
    }

    void SyncLockedHandFromSaveIfNeeded()
    {
        if (HasLockedCards()) return;
        var saved = SaveLoadContext.CurrentSaveData?.plantsShopData;
        if (saved?.selectedCreatorIds != null && saved.selectedCreatorIds.Count > 0)
            lockedHand = CloneHand(saved);
    }

    /// <summary>读档时由 Controller 调用，同步锁定手牌与阳光到插件缓存。</summary>
    public void SyncLockedHandFromSave(GameSaveData save)
    {
        if (save?.plantsShopData == null) return;
        if (save.plantsShopData.selectedCreatorIds == null || save.plantsShopData.selectedCreatorIds.Count == 0)
            return;
        lockedHand = CloneHand(save.plantsShopData);
    }

    static PlantsShopSaveData CloneHand(PlantsShopSaveData src)
    {
        if (src == null) return null;
        return new PlantsShopSaveData
        {
            sunLight = src.sunLight,
            selectedCreatorIds = src.selectedCreatorIds != null
                ? new List<string>(src.selectedCreatorIds)
                : new List<string>()
        };
    }

    bool HasLockedCards() => lockedHand?.selectedCreatorIds != null && lockedHand.selectedCreatorIds.Count > 0;

    static bool IsSurvival(LevelController controller) =>
        controller?.levelData?.levelMode == LevelMode.SurvivalMode;

    static bool IsSurvivalLevel() =>
        LevelManage.instance?.currentLevel?.levelMode == LevelMode.SurvivalMode;

    static int GetRoundIndex(LevelController controller) =>
        controller is LevelController_Endless endless ? endless.RunState.selectionIndex : 1;
}
