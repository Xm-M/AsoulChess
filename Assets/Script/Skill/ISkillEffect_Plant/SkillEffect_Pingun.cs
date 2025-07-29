using Sirenix.OdinInspector;
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
        Tile t = user.moveController.standTile;
        user.moveController.standTile.ChessLeave(user);
        user.moveController.standTile = t;
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
    /// µ±Åö×²µ½ÕÏ°­ÎïµÄÊ±ºò
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
        }else if (moveDir.y<0)
        {
            moveDir = new Vector3(1, 1, 0);
            float dis =ymax-user.transform.position.y;
            targetPos = user.transform.position + new Vector3(dis, dis, 0);
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
public class PassiveSkill_HitExplode : ISkill
{

    [SerializeReference]
    [LabelText("Ñ°µÐ·½Ê½")]
    public IFindTarget findTarget;
    [LabelText("±¬Õ¨ÉËº¦±¶ÂÊ")]
    public float explodeRate;
    public CarArmor armor;
    List<Chess> targets;
    float t;
    Chess user;
    public bool IfSkillReady(Chess user)
    {
        return false;
    }
    public void InitSkill(Chess user)
    {
        t = 0;
        targets = new List<Chess>();
        this.user = user;
        armor.onHit.AddListener(Explode);
        
    }

    public void LeaveSkill(Chess user)
    {
        t = 0;
    }
    public void UseSkill(Chess user)
    {
        
    }
    public void Explode()
    {
        
        findTarget.FindTarget(user, targets);
        for (int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
            user.skillController.DM.damageElementType = ElementType.Explode;
            user.skillController.DM.damage =
                user.propertyController.GetAttack() * explodeRate;
            user.propertyController.TakeDamage(user.skillController.DM);
        }
        user.Death();
    }
    public void WhenEnter(Chess user)
    {
        t = 0;
        targets.Clear();
    }
}
public class PassiveSkill_Uik : ISkill
{
    Chess user;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return false;
    }

    public void InitSkill(Chess user)
    {
        //throw new System.NotImplementedException();
        this.user = user;
    }

    public void LeaveSkill(Chess user)
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), WhenSakiPlant);
    }

    public void UseSkill(Chess user)
    {
        //throw new System.NotImplementedException();
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), WhenSakiPlant);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
    }

    public void WhenSakiPlant(Chess saki)
    {
        if (saki.propertyController.creator.chessName.Contains("·á´¨Ïé×Ó")&&
            saki.moveController.standTile.mapPos.y==user.moveController.standTile.mapPos.y
            && user.stateController.currentState.state.stateName != StateName.MoveState)
        {
            user.stateController.ChangeState(StateName.MoveState);
            Tile t = user.moveController.standTile;
            user.moveController.standTile.ChessLeave(user);
            user.moveController.standTile = t;
        }
    }
}