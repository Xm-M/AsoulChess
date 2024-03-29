using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotTile : Tile
{
    public Tile downTile;
    protected override void Awake()
    {
        
    }
    public void SetTile(Tile t)
    {
        downTile = t;
        chessesIntile = t.chessesIntile;
        //cubePos = t.cubePos;
        mapPos = t.mapPos;
    }
    public void ClearTile()
    {
        downTile = null;
        chessesIntile = null;
    }
    //protected override void OnMouseDown()
    //{
    //    base.OnMouseDown();
    //    Debug.Log("Pot");
    //}

}
