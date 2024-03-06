using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class MoveController:Controller
{
    public Chess chess;
    public Tile standTile;
    public Tile nextTile;
    [SerializeReference]
    public FindTileMethod tileMethod;
    bool ifMove;
    public virtual void InitController(Chess chess)
    {
        this.chess = chess;
        
        //tileMethod.FindNextTile(chess);
    }
    public void WhenControllerEnterWar()
    {
    }

    public void WhenControllerLeaveWar()
    {
        standTile.ChessLeave(chess);
        standTile = null;
        nextTile = null;
    }
    
    public virtual void StartMoving(string anim="run")
    {
        chess.animator.Play(anim);
        nextTile=tileMethod.FindNextTile(chess);
        tileMethod.StartMoving();
    }
    public virtual void WhenMoving()
    {
        tileMethod.WhenMoving();
        if (nextTile != null && Vector2.Distance(chess.transform.position, nextTile.transform.position) > 0.01)
        {
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, nextTile.transform.position,
                chess.propertyController.GetMoveSpeed()*Time.deltaTime);
        }
        else if (nextTile != null)
        {
            if (Vector2.Distance(chess.transform.position, nextTile.transform.position) <= 0.01)
            {
                standTile = nextTile;
                nextTile = tileMethod.FindNextTile(chess);
            }
        }
        else 
        {
            nextTile = tileMethod.FindNextTile(chess);
        }
    }
    public virtual void MoveToTarget(Vector2 targetPos)
    {
        chess.transform.position = Vector2.MoveTowards(chess.transform.position, targetPos,
                chess.propertyController.GetMoveSpeed());
    }


    public virtual void EndMoving()
    {
        tileMethod.EndMoving();
    }

   
}
