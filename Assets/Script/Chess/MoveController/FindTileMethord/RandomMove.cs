using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 随机移动 给猫猫使用的
/// </summary>
public class RandomMove : FindTileMethod
{

    public override Tile FindNextTile(Chess c)
    {
        int dx = Random.Range(-1, 2);
        int dy = Random.Range(-1, 2);
        int x = c.moveController.standTile.mapPos.x + dx;
        x = Mathf.Min(x, MapManage_PVZ.instance.mapSize.x-1);
        x = Mathf.Max(x, 0);
        int y = c.moveController.standTile.mapPos.y + dy;
        y = Mathf.Min(y, MapManage_PVZ.instance.mapSize.y-1);
        y = Mathf.Max(y,0);
        return MapManage.instance.tiles[x,y];
    }
 
}
