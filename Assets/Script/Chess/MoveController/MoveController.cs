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
    [HideInInspector]
    public UnityEvent<Chess, Tile> OnReachTile;
    public virtual void InitController(Chess chess)
    {
        this.chess = chess;
        //tileMethod.FindNextTile(chess);
    }
    public void WhenControllerEnterWar()
    {
        ifMove=false;
        OnReachTile.RemoveAllListeners();
    }

    public void WhenControllerLeaveWar()
    {
        standTile?.ChessLeave(chess);
        standTile = null;
        nextTile = null;
        ifMove = false;
        OnReachTile.RemoveAllListeners();
    }
    
    public virtual void StartMoving(string anim="run")
    {
        chess.animatorController.PlayMove();
        nextTile=tileMethod.FindNextTile(chess);
        tileMethod.StartMoving(chess);
    }
    public virtual void WhenMoving()
    {
        if (ifMove) return;
        tileMethod.WhenMoving(chess);
        if (nextTile != null && Vector2.Distance(chess.transform.position, nextTile.transform.position) > 0.01)
        {
            //chess.transform.position = Vector2.MoveTowards(chess.transform.position, nextTile.transform.position,
            //    chess.propertyController.GetMoveSpeed()*Time.deltaTime);
            Vector3 pos = chess.transform.position;
            Vector3 target = nextTile.transform.position;

            float speed = chess.propertyController.GetMoveSpeed();

            float newX = Mathf.MoveTowards(pos.x, target.x, speed * Time.deltaTime);
            float newY = Mathf.MoveTowards(pos.y, target.y, speed * 2f * Time.deltaTime);

            chess.transform.position = new Vector3(newX, newY, pos.z);
            //我当时写这一句的目的是 哪个wineTile可以准确的放到下一格 或者说脚下的那一格对吧 不然没什么用这个的道理啊
            if (Vector2.Distance(chess.transform.position, nextTile.transform.position) < 1.25)
                standTile = nextTile;
        }
        else if (nextTile != null)
        {
            if (Vector2.Distance(chess.transform.position, nextTile.transform.position) <= 0.01)
            {
                standTile = nextTile;
                OnReachTile?.Invoke(chess,standTile);
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
        //Debug.Log("移动中");
        while (ifMove && !chess.IfDeath && LevelManage.instance.IfGameStart)
        {
            chess.transform.position = Vector2.MoveTowards(chess.transform.position, targetPos,
                movespeed * Time.deltaTime);
            if (Vector2.Distance(chess.transform.position, targetPos)< 0.01)
            {
                //Debug.Log("移动");
                moveOver?.Invoke();
            }

            yield return null;
            //
        }
        //standTile = newStandTile;
        ifMove = false;
        
    }

    public virtual void Turn()
    {
        chess.ForceFlip();
        nextTile = tileMethod?.FindNextTile(chess);
    }

    public virtual void EndMoving()
    {
        tileMethod.EndMoving(chess);
    }
     
    public void ContinuMove() => ifMove = true;
    public void JumpToTarget(Chess chess, Transform transform, Vector2 startPos, Vector2 endPos, float maxHeight, float moveSpeed)
    {
        chess.StartCoroutine(Jump(chess,transform,startPos,endPos,maxHeight,moveSpeed));
    }

    public IEnumerator Jump(Chess chess, Transform transform, Vector2 startPos, Vector2 endPos, float maxHeight, float moveSpeed)
    {
        ifMove = true;
        // 初始化位置
        transform.position = startPos;

        float dx = Mathf.Abs(endPos.x - startPos.x);
        float dy = Mathf.Abs(endPos.y - startPos.y);

        // 水平距离几乎为0时，避免除0；退化成按总距离估算时间
        float distForTime = dx > 0.0001f ? dx : Vector2.Distance(startPos, endPos);

        // moveSpeed 非法则直接瞬移到终点
        if (moveSpeed <= 0.0001f || distForTime <= 0.0001f)
        {
            transform.position = endPos;
            yield break;
        }

        float duration = distForTime / moveSpeed; // 跳跃总时长
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration); // 0~1

            // x 线性插值（整体水平匀速）
            float x = Mathf.Lerp(startPos.x, endPos.x, t);

            // y = 起终点线性插值 + 抛物线额外高度
            // 4*t*(1-t) 在 t=0/1 为0，在 t=0.5 为1
            float baseY = Mathf.Lerp(startPos.y, endPos.y, t);
            float y = baseY + (4f * maxHeight * t * (1f - t));

            transform.position = new Vector2(x, y);
            yield return null;
        }
        // 结束时强制落到终点，避免浮点误差
        transform.position = endPos;
        chess.animatorController.animator.Play("land");
        ifMove = false;
    }

}
