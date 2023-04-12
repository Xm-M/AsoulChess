using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CantMoveTile : Tile
{
    public Vector2 Pos;
    private void Awake()
    {
        IfMoveable = false;
    }
}
