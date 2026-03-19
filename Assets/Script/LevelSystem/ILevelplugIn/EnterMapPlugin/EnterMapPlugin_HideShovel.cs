using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterMapPlugin_HideShovel : ILevelPlugin
{
    public void OverPlugin(LevelController levelController)
    {
        UIManage.GetView<PlantsShop>().Shovel.SetActive(true);
    }

    public void StadgeEffect(LevelController levelController)
    {
        UIManage.GetView<PlantsShop>().Shovel.SetActive(false);
    }
}
