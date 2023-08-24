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
        Vector2Int currentPos=c.moveController.standTile.mapPos;
        //Debug.Log(currentPos);
        if (currentPos.x > 0)
            return MapManage.instance.tiles[currentPos.x - 1, currentPos.y];
        else return (MapManage.instance as MapManage_PVZ).roomTile;
    }
    public override void StartMoving()
    {
        base.StartMoving();
    }
    public override void WhenMoving()
    {
        base.WhenMoving();
    }
    public override void EndMoving()
    {
        base.EndMoving();
    }
}
