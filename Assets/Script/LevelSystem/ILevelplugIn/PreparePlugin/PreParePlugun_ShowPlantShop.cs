using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 展示植物选择框
/// </summary>
public class PreParePlugun_ShowPlantShop : ISaveableLevelPlugin
{
    public int baseSunLight = 50;

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
        UIManage.Show<PlantsShop>();
        if (!SaveLoadContext.IsLoadFromSave)
        {
            SunLightPanel.instance.SetSunLight(baseSunLight);
        }
    }

    public void OverPlugin(LevelController levelController)
    {
        UIManage.Close<PlantsShop>();
    }
}