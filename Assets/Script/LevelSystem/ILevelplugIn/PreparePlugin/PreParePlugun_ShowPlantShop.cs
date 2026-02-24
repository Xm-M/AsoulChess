using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 展示植物选择框
/// </summary>
public class PreParePlugun_ShowPlantShop : ILevelPlugin
{
    public int baseSunLight = 50;
    public void StadgeEffect(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        UIManage.Show<PlantsShop>();
        SunLightPanel.instance.SetSunLight(baseSunLight);
    }
    public void OverPlugin(LevelController levelController)
    {
        UIManage.Close<PlantsShop>();
    }
}