using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class TileEffect_WineZone : TileEffect
{
    [LabelText("持续时间")]
    public float continueTime;
    [SerializeReference]
    [LabelText("酒Buff")]
    public Buff wineBuff;
    Timer timer;
    Tile tile;
    public override void EnterTile(Tile tile)
    {
        if(!tile.AddObjectToTile<TileEffect_WineZone>(this))
        {
            ObjectPool.instance.Recycle(gameObject);
        }
        else
        {
            this.tile = tile;
            timer = GameManage.instance.timerManage.AddTimer(LeaveTile,continueTime);
        }

    }
    public void LeaveTile()
    {
        LeaveTile(tile);
    }
    public override void LeaveTile(Tile tile)
    {
        tile.RemoveObjectFromTile<TileEffect_WineZone>();
        ObjectPool.instance.Recycle(gameObject);
    }

    public override void ResetTileEffect()
    {
        timer.ResetTime();    
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        Chess chess = collision.GetComponent<Chess>();
        if (chess != null)
        {
            chess.buffController.AddBuff(wineBuff);
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        Chess chess = collision.GetComponent<Chess>();
        if (chess != null)
        {
            chess.buffController.AddBuff(wineBuff);
        }
    }
}
