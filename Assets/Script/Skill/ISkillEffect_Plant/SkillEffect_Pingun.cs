using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Pingun : ISkill
{
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return false;
    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {
        user.stateController.ChangeState(StateName.MoveState);
        user.moveController.standTile.ChessLeave(user);
    }

    public void WhenEnter(Chess user)
    {
         
    }
}

public class SkillEffect_Bowling : ISkill
{
    public CarArmor armor;
    Chess user;
    float ymax, ymin;
    Vector2 targetPos;
    Vector2 moveDir;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return true;
    }

    public void InitSkill(Chess user)
    {
        this.user = user;
        armor.onHit.AddListener(OnHit);
    }

    public void LeaveSkill(Chess user)
    {
        //throw new System.NotImplementedException();
        user.moveController.StopMove();
    }

    public void UseSkill(Chess user)
    {
        //throw new System.NotImplementedException();
        user.moveController.MoveToTarget(targetPos,
            moveDir,OnHit);
        if(user.moveController.standTile!=null)
            user.moveController.standTile.ChessLeave(user);
    }

    /// <summary>
    /// 当碰撞到障碍物的时候
    /// </summary>
    public void OnHit()
    {
        //user.moveController.StopMove();
        //Debug.Log("onhit");
        if (moveDir.y == 0)
        {
            moveDir = Random.Range(-1f, 1f) > 0 ? new Vector3(1, -1, 0) : new Vector3(1, 1, 0);
        }
        if (moveDir.y>0)
        {
            moveDir = new Vector3(1, -1, 0);
            float dis = user.transform.position.y - ymin;
            targetPos = user.transform.position + new Vector3(dis, -dis, 0);
            //Debug.Log(targetPos);
            //Debug.Log("向上转");
        }else if (moveDir.y<0)
        {
            moveDir = new Vector3(1, 1, 0);
            float dis =ymax-user.transform.position.y;
            targetPos = user.transform.position + new Vector3(dis, dis, 0);
            //Debug.Log(targetPos);
            //Debug.Log("向下转");
        }
        //Debug.Log(targetPos);
        user.moveController.MoveToTarget(targetPos,moveDir, OnHit);
    }
     


    public void WhenEnter(Chess user)
    {
        user.transform.right = Vector3.right;
        ymax = MapManage_PVZ.instance.tiles[0, MapManage_PVZ.instance.mapSize.y - 1].transform.position.y;
        ymin = MapManage.instance.tiles[0,0].transform.position.y;
        moveDir = new Vector2(1, 0);
        targetPos = user.transform.position + user.transform.right * 1000;
    }
}
