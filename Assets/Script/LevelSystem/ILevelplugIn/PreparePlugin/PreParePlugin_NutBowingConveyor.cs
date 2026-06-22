using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreParePlugin_NutBowingConveyor : ILevelPlugin
{
    public List<PropertyCreator> creators;
    public GameObject redLine;
    [Tooltip("红线以右不可种植；默认 3 即列 0~2 可种，红线放在第 2 列")]
    public int plantMinColumnX = 3;

    GameObject line;

    public void StadgeEffect(LevelController levelController)
    {
        UIManage.Show<ConveyorPanel>();
        UIManage.GetView<ConveyorPanel>().InitCreator(creators, 6);
        (MapManage_PVZ.instance as MapManage_PVZ).WhenGameStart();

        if (MapManage.instance != null)
            MapManage.instance.plantMaxMapColumnX = plantMinColumnX - 1;

        var map = MapManage.instance;
        if (map != null)
        {
            for (int i = plantMinColumnX; i < map.mapSize.x; i++)
            {
                for (int j = 0; j < map.mapSize.y; j++)
                {
                    var c = map.tiles[i, j] != null ? map.tiles[i, j].GetComponent<Collider2D>() : null;
                    if (c != null) c.enabled = false;
                }
            }
        }

        line = GameObject.Instantiate(redLine);
        int lineCol = Mathf.Clamp(plantMinColumnX - 1, 0, MapManage.instance.mapSize.x - 1);
        int lineRow = MapManage.instance.mapSize.y / 2;
        line.transform.position = MapManage.instance.tiles[lineCol, lineRow].transform.position;
    }

    public void OverPlugin(LevelController levelController)
    {
        GameOver();
    }

    public void GameOver()
    {
        if (line != null)
        {
            GameObject.Destroy(line);
            line = null;
        }
        if (MapManage.instance != null)
            MapManage.instance.plantMaxMapColumnX = -1;
        UIManage.Close<ConveyorPanel>();
    }
}
