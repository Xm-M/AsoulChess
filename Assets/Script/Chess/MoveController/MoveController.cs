using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 感觉moveController也是可有可无的东西 要不然就直接删了吧
/// 但是standTile还是有用的 这就很烦了
/// 所有跟chess有关的startcorutin都不要放在外面写
/// </summary>
[Serializable]
public class MoveController:Controller
{
    public Chess chess;
    public Tile standTile;
    public Tile nextTile;
    [SerializeReference]
    public FindTileMethod tileMethod;
    bool ifMove;
    Vector2 targetPos;
    public virtual void InitController(Chess chess)
    {
        this.chess = chess;
        //tileMethod.FindNextTile(chess);
    }
    public void WhenControllerEnterWar()
    {
        ifMove=false;
    }

    public void WhenControllerLeaveWar()
    {
        standTile?.ChessLeave(chess);
        standTile = null;
        nextTile = null;
        ifMove = false;
    }
    
    public virtual void StartMoving(string anim="run")
    {
        chess.animatorController.PlayMove();
        nextTile=tileMethod.FindNextTile(chess);
        tileMethod.StartMoving();
    }
    public virtual void WhenMoving()
    {
        tileMethod.WhenMoving();
        if (ifMove) return;
        if (nextTile != null && Vector2.Distance(chess.transform.position, nextTile.transform.position) > 0.01)
        {
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, nextTile.transform.position,
                chess.propertyController.GetMoveSpeed()*Time.deltaTime);
            if(Vector2.Distance(chess.transform.position, nextTile.transform.position) < 1.25)
                standTile = nextTile;
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
    }//
    public virtual void MoveToTarget(Vector2 targetPos, float moveSpeed=-1, UnityAction moveOver = null)
    {
        //chess.transform.position = Vector2.MoveTowards(chess.transform.position, targetPos,
        //        chess.propertyController.GetMoveSpeed());
        //Debug.Log("开始移动");
        if (!ifMove)
        {
            
            Vector2 dir = targetPos - (Vector2)chess.transform.position;
            int dx = (int)(dir.x / 2.5);
            int dy = (int)(dir.y / 2.5);

            Tile targetTile = MapManage_PVZ.instance.tiles[standTile.mapPos.x + dx, standTile.mapPos.y + dy];
            chess.StartCoroutine(MoveToTile(targetPos, targetTile, moveSpeed));
        }
    }
    public virtual void MoveToTarget(Vector2 targetPos,Vector2 moveDir,UnityAction moveOver = null)
    {
        if (!ifMove)
        {
            this.targetPos = targetPos;
            ifMove = true;
            Debug.Log("开始移动");
            chess.StartCoroutine(MoveToTile(moveOver));
        }
        else
        {
            this.targetPos = targetPos;
        }
    }



    public virtual void MoveToTarget(Tile tile,float moveSpeed=-1,UnityAction moveOver=null)
    {
        if(!ifMove)
            chess.StartCoroutine(MoveToTile(tile.transform.position,tile,moveSpeed,moveOver));
    }
    public void StopMove()
    {
        ifMove = false;
        
    }
    IEnumerator MoveToTile(Vector2 targetPos,Tile newStandTile,float movespeed, UnityAction moveOver = null)
    {
        if (movespeed == -1)
        {
            movespeed = chess.propertyController.GetMoveSpeed();
        }
        ifMove = true;
        while(ifMove&&Vector2.Distance(chess.transform.position, newStandTile.transform.position) > 0.01&&!chess.IfDeath&&LevelManage.instance.IfGameStart)
        {
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, newStandTile.transform.position,
                movespeed * Time.deltaTime);
            yield return null;
            //Debug.Log("移动中");
        }
        standTile = newStandTile;
        ifMove = false;
        moveOver?.Invoke();
    }
    IEnumerator MoveToTile( UnityAction moveOver = null)
    {
        float movespeed = chess.propertyController.GetMoveSpeed();
        ifMove = true;
        Debug.Log("移动中");
        while (ifMove && !chess.IfDeath && LevelManage.instance.IfGameStart)
        {
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, targetPos,
                movespeed * Time.deltaTime);
            if (Vector2.Distance(chess.transform.position, targetPos)< 0.01)
            {
                Debug.Log("移动");
                moveOver?.Invoke();
            }

            yield return null;
            //
        }
        //standTile = newStandTile;
        ifMove = false;
        
    }



    public virtual void EndMoving()
    {
        tileMethod.EndMoving();
    }

   
}
