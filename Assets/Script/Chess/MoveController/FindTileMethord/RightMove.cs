using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightMove : FindTileMethod
{
    Chess c;
    public Vector2 moveDir;
    public override Tile FindNextTile(Chess c)
    {
        c.moveController.standTile.ChessLeave(c);
        this.c = c;
        return null;
    }
    public override void StartMoving()
    {
        //base.StartMoving();
    }
    public override void WhenMoving()
    {
 
        c.transform.position = Vector2.MoveTowards(c.transform.position, (Vector2)c.transform.position +moveDir,
            Time.deltaTime * c.propertyController.GetMoveSpeed());
 
    }
    public override void EndMoving()
    {
        //base.EndMoving();
    }
}
