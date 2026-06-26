using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 肉鸽地图商店：掷货、购买、离开。不进战斗，不经过 <see cref="RoguelikeRewardFlow"/>。
/// </summary>
public static class RoguelikeShopFlow
{
    public static void EnsureOffersForNode(int nodeId)
    {
        var state = RoguelikeRunService.State;
        if (state == null || nodeId < 0)
            return;

        if (state.activeShopNodeId == nodeId
            && state.shopOffers != null
            && state.shopOffers.Count > 0)
            return;

        var economy = RoguelikeRunService.ActiveRunConfig?.economyConfig
                      ?? RoguelikeEconomyConfig.CreateRuntimeDefaults();

        state.activeShopNodeId = nodeId;
        state.shopOffers = economy.RollShopOffers(state, nodeId);
        RoguelikeRunService.SaveRun();
    }

    public static bool TryPurchase(int slotIndex)
    {
        var state = RoguelikeRunService.State;
        if (state == null || state.shopOffers == null)
            return false;
        if (slotIndex < 0 || slotIndex >= state.shopOffers.Count)
            return false;

        var offer = state.shopOffers[slotIndex];

        if (offer == null || offer.sold)
            return false;
        if (string.IsNullOrEmpty(offer.creatorChessName))
            return false;

        bool testFree = IsTestMode();
        if (!testFree && offer.price > state.runGold)
            return false;

        if (!RoguelikeRunPlantPool.AddPlant(offer.creatorChessName))
        {
            Debug.LogWarning(
                $"[RoguelikeShopFlow] 购买失败：{offer.creatorChessName}（可能已拥有）");
            return false;
        }

        if (!testFree)
            state.runGold -= offer.price;
        offer.sold = true;
        RoguelikeRunService.SaveRun();
        if (testFree)
            Debug.Log($"[RoguelikeShopFlow] Test 模式免费购买 {offer.creatorChessName}");
        else
            Debug.Log(
                $"[RoguelikeShopFlow] 购买 {offer.creatorChessName}（{offer.price} 金币），剩余 {state.runGold}");
        return true;
    }

    static bool IsTestMode() =>
        GameManage.instance != null && GameManage.instance.mode == GameMode.Test;

    public static void ClearShopState()
    {
        var state = RoguelikeRunService.State;
        if (state == null)
            return;

        state.activeShopNodeId = -1;
        if (state.shopOffers != null)
            state.shopOffers.Clear();
    }
}
