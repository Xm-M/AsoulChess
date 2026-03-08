using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class HorMove : FindTileMethod
{
    //public Vector2 moveDir; 
    Chess chess;
    public override Tile FindNextTile(Chess c)
    {
        this.chess=c;
        if (c.moveController.standTile == null) return null;
        Vector2Int currentPos=c.moveController.standTile.mapPos;
        Vector2Int nextPos = new Vector2Int(currentPos.x + (int)c.transform.right.x, currentPos.y);
        //Debug.Log(currentPos);
        if (nextPos.x >= 0 && nextPos.x < MapManage.instance.mapSize.x)
            return MapManage.instance.tiles[nextPos.x, nextPos.y];
        else if (nextPos.x >= MapManage.instance.mapSize.x)
        {
            return (MapManage.instance as MapManage_PVZ).deathTile;
        }
        else if (nextPos.x < 0)
        {
            return (MapManage.instance as MapManage_PVZ).roomTile[currentPos.y];
        }
        return c.moveController.standTile;
    }
 
     
}
