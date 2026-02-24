using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreParePlugin_NutBowingConveyor :ILevelPlugin
{
    public List<PropertyCreator> creators;
    public GameObject redLine;
    GameObject line;
    public void StadgeEffect(LevelController levelController)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators, 6);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();
        for (int i = 3; i < MapManage_PVZ.instance.mapSize.x; i++)
            for (int j = 0; j < MapManage.instance.mapSize.y; j++)
            {
                MapManage.instance.tiles[i, j].gameObject.layer = 0;
            }
        //line = ObjectPool.instance.Create(redLine);
        line = GameObject.Instantiate(redLine);
        line.transform.position = MapManage.instance.tiles[2, 2].transform.position;
        //EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), GameOver);
    }
    public void OverPlugin(LevelController levelController)
    {
        GameOver();
    }
    public void GameOver()
    {
        //ObjectPool.instance.Recycle(line);
        GameObject.Destroy(line);
        UIManage.Close<ConveyorPanel>();
    }
}
