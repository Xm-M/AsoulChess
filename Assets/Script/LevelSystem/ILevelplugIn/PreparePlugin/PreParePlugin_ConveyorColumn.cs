using System.Collections.Generic;
using UnityEngine;

/// <summary>排山倒海：固定序前缀 + 随机池传送带，列种植卡。</summary>
public class PreParePlugin_ConveyorColumn : ILevelPlugin
{
    public List<PropertyCreator> fixedPrefixCreators;
    public List<PropertyCreator> loopCreators;
    public float spawnInterval = 6f;

    public void StadgeEffect(LevelController levelController)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitColumnMode(fixedPrefixCreators, loopCreators, spawnInterval);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
    }

    public void OverPlugin(LevelController levelController)
    {
        UIManage.Close<ConveyorPanel>();
    }
}
