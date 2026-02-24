using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartPlugin_LimitShop : ILevelPlugin
{
    public int baseSunLight;
    public List<PropertyCreator> creators;
    public void OverPlugin(LevelController levelController)
    {
        //throw new System.NotImplementedException();
        UIManage.Close<LimitSelectShopPanel>();
       
    }

    public void StadgeEffect(LevelController levelController)
    {
        SunLightPanel.instance.SetSunLight(baseSunLight);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
        UIManage.GetView<LimitSelectShopPanel>().InitCreator(creators   );
        UIManage.Show<LimitSelectShopPanel>();
    }
}
