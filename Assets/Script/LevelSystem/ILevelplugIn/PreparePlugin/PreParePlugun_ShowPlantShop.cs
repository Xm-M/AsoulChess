using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 准备阶段打开植物商店。冒险关每次完整选卡；生存关首轮选卡后锁定，轮间只恢复顶栏等待开战。
/// </summary>
public class PreParePlugun_ShowPlantShop : ISaveableLevelPlugin, IRoundEndPlugin
{
    public int baseSunLight = 50;
    public List<PropertyCreator> cards;

    /// <summary>生存模式：首轮开战后的锁定手牌（内存缓存，读档时从 plantsShopData 同步）。</summary>
    PlantsShopSaveData lockedHand;

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData == null) return;

        SyncLockedHandFromSaveIfNeeded();
        int sun = SunLightPanel.instance != null ? SunLightPanel.instance.sunLight : 0;

        // 生存模式：选卡只写一次，阳光每轮末更新
        if (IsSurvivalLevel() && HasLockedCards())
        {
            lockedHand.sunLight = sun;
            saveData.plantsShopData = CloneHand(lockedHand);
            return;
        }

        var shop = UIManage.GetView<PlantsShop>();
        if (shop == null) return;

        var captured = new PlantsShopSaveData
        {
            selectedCreatorIds = new List<string>(),
            sunLight = sun
        };
        foreach (var icon in shop.currentShopIcons)
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
        int round = GetRoundIndex(levelController);

        if (round <= 1 && !HasLockedCards())
            RunFirstRoundPrepare(levelController);
        else
            RunSurvivalRoundPrepare();
    }

    /// <summary>生存模式开战时锁定手牌（仅写内存，轮末存档写入文件）。</summary>
    public void EnsureLockedHandFromShop()
    {
        if (HasLockedCards()) return;
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

    void RunFirstRoundPrepare(LevelController levelController)
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

    void RunSurvivalRoundPrepare()
    {
        var hand = GetLockedHandForDisplay();
        if (hand == null)
        {
            Debug.LogWarning("[ShowPlantShop] 生存轮间 Prepare 无锁定手牌，回落首轮选卡");
            RunFirstRoundPrepare(null);
            return;
        }

        var shop = UIManage.GetView<PlantsShop>();
        if (shop != null)
            shop.ShowLockedHand(hand, autoStart: false, restoreSunLight: false);
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
