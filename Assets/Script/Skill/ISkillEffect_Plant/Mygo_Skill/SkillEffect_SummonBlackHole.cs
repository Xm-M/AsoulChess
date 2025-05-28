using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_SummonBlackHole : ISkill
{
    //public float startTime;
    public float coldDown;//CD
    public float loopTime;
    public int FontNum=4;
    public Transform sunLightPos;
    public GameObject blackHole;
    public MouseDownSkill ifMouseDown;
    float t;
    Timer timer;
    GameObject black;
    Chess user;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;
        
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            user.animatorController.ChangeFlash(-1);
            if (ifMouseDown.IfDown)
            {
                t = 0;
                user.animatorController.ChangeFlash(1);
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
         this.user = user;
    }

    public void LeaveSkill(Chess user)
    {
        if (timer != null)
        {
            timer.Stop();
            timer = null;
        }
    }

    public void UseSkill(Chess user)
    {
        black = ObjectPool.instance.Create(blackHole);
        black.tag = user.tag;
        Tile tile;
        Vector2Int standPos = user.moveController.standTile.mapPos;
        if (standPos.x + FontNum < MapManage.instance.mapSize.x)
        {
            tile = MapManage.instance.tiles[standPos.x + FontNum,standPos.y];
        }
        else
        {
            tile=MapManage.instance.tiles[MapManage.instance.mapSize.x-1, standPos.y];
        }
        black.transform.position = tile.transform.position;
        black.GetComponent<BlackHole>().Init(tile,loopTime);
        timer = GameManage.instance.timerManage.AddTimer(ChangeState, loopTime, false);
        
    }
    public void ChangeState()
    {
        //ObjectPool.instance.ReycleObject(black);
        user.stateController.ChangeState(StateName.IdleState);
        user.animatorController.ChangeFlash(0);
    }

    public void WhenEnter(Chess user)
    {
        t =0;
        user.animatorController.ChangeFlash(0);
    }
}
