using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ?????????
/// </summary>
public class PreParePlugun_ShowPlantShop : ISaveableLevelPlugin
{
    public int baseSunLight = 50;
    /// <summary>???????????????????????????10??????????????????????10??????????????????????</summary>
    public List<PropertyCreator> cards;

    public void CaptureTo(GameSaveData saveData)
    {
        if (saveData == null) return;
        var shop = UIManage.GetView<PlantsShop>();
        if (shop == null) return;

        saveData.plantsShopData = new PlantsShopSaveData
        {
            selectedCreatorIds = new List<string>(),
            sunLight = SunLightPanel.instance != null ? SunLightPanel.instance.sunLight : 0
        };

        foreach (var icon in shop.currentShopIcons)
        {
            if (icon != null && icon.good != null)
                saveData.plantsShopData.selectedCreatorIds.Add(icon.good.chessName);
        }
    }

    public void StadgeEffect(LevelController levelController)
    {
        if (cards != null && cards.Count > 0)
            PlantsShop.OverrideCreators = cards;
        UIManage.Show<PlantsShop>();
        PlantsShop.OverrideCreators = null;
        if (!SaveLoadContext.IsLoadFromSave)
        {
            SunLightPanel.instance.SetSunLight(baseSunLight);
            var shop = UIManage.GetView<PlantsShop>();
            var creators = cards != null && cards.Count > 0
                ? cards
                : (GameManage.instance?.playerOwnedCreators != null && GameManage.instance.playerOwnedCreators.Count > 0
                    ? GameManage.instance.playerOwnedCreators
                    : GameManage.instance?.allChess);
            if (shop != null && creators != null && creators.Count > 0 && creators.Count < shop.maxCount)
            {
                foreach (var icon in shop.allSelectIcons)
                {
                    if (icon != null) shop.AddSelection(icon);
                }
                shop.GameStart();
            }
        }
    }

    public void OverPlugin(LevelController levelController)
    {
        UIManage.Close<PlantsShop>();
    }
}