using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProperty", menuName = "Message/Planter")]
public class Planter_PropertyCreator : PropertyCreator
{
    public TileType newTileType;
    public override void WhenChessEnterWar(Chess chess)
    {
        chess.moveController.standTile.typeStack.Push(newTileType);
        base.WhenChessEnterWar(chess);
    }
    public override void WhenChessLeaveWar(Chess chess)
    {
        chess.moveController.standTile.typeStack.Pop();
        base.WhenChessLeaveWar(chess);
    }
}
