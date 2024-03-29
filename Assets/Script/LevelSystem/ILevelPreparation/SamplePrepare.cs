using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplePrepare : ILevelPreparation
{
    public int baseSunLight;
    public void Prepare(LevelData levelData)
    {
        UIManage.Show<PlantsShop>();
        SunLightPanel.instance.ChangeSunLight(baseSunLight);
    }
}
