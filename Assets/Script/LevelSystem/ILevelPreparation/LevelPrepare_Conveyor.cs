using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这是传送带模式的准备阶段 其实就没有准备阶段了 但是会看一眼僵尸 同时使用的栏会变成传送带
/// </summary>
public class LevelPrepare_Conveyor : ILevelPreparation
{
    public List<PropertyCreator> creators;
    public void Prepare(LevelData levelData)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
    }
}

public class LevelPrepare_NutBowling : ILevelPreparation
{
    public List<PropertyCreator> creators;
    public GameObject redLine;
    GameObject gameObject;
    GameObject line;
    public void Prepare(LevelData levelData)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators,4);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
        for (int i = 3; i < MapManage_PVZ.instance.mapSize.x; i++)
            for (int j = 0; j < MapManage.instance.mapSize.y; j++)
            {
                MapManage.instance.tiles[i, j].gameObject.layer = 0;
            }
        line=ObjectPool.instance.Create(redLine);
        line.transform.position = MapManage.instance.tiles[2, 2].transform.position;
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), GameOver);
    }
    public void GameOver()
    {
        ObjectPool.instance.Recycle(line);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), GameOver);
    }


}
