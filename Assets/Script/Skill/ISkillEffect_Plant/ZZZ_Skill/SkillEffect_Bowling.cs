using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Bowling_ : ISkillEffect
{
    //public CarArmor armor;
    //Chess user;
    //float ymax, ymin;
    //Vector2 targetPos;
    //Vector2 moveDir;
    //public void LeaveSkill(Chess user)
    //{
    //    //throw new System.NotImplementedException();
    //    user.moveController.StopMove();
    //}

    //public void UseSkill(Chess user)
    //{
    //    //throw new System.NotImplementedException();
    //    user.moveController.MoveToTarget(targetPos,
    //        moveDir, OnHit);
    //    if (user.moveController.standTile != null)
    //        user.moveController.standTile.ChessLeave(user);
    //}

    ///// <summary>
    ///// 当碰撞到障碍物的时候
    ///// </summary>
    //public void OnHit()
    //{
    //    //user.moveController.StopMove();
    //    //Debug.Log("onhit");
    //    if (moveDir.y == 0)
    //    {
    //        moveDir = Random.Range(-1f, 1f) > 0 ? new Vector3(1, -1, 0) : new Vector3(1, 1, 0);
    //    }
    //    if (moveDir.y > 0)
    //    {
    //        moveDir = new Vector3(1, -1, 0);
    //        float dis = user.transform.position.y - ymin;
    //        targetPos = user.transform.position + new Vector3(dis, -dis, 0);
    //    }
    //    else if (moveDir.y < 0)
    //    {
    //        moveDir = new Vector3(1, 1, 0);
    //        float dis = ymax - user.transform.position.y;
    //        targetPos = user.transform.position + new Vector3(dis, dis, 0);
    //    }
    //    //Debug.Log(targetPos);
    //    user.moveController.MoveToTarget(targetPos, moveDir, OnHit);
 
    //public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    //{
    //    WhenEnter(user);
    //    UseSkill(user);
    //}
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Tile standTile = user.moveController.standTile;
        user.moveController.standTile.ChessLeave(user);
        user.moveController.standTile = standTile;
    }
}
public class Passive_hitExplode : ISkillEffect
{
    public CarArmor armor;
    public float checkRange;
    Chess user;
    
    List<Chess> targets;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        //throw new System.NotImplementedException();
        this.user = user;
        armor.onHit.AddListener(OnHit);
        
    }
    public void OnHit() {
        targets = new List<Chess>();
        SearchTarget(user, targets);
        DamageMessege dm = user.skillController.DM;
        float damage = user.propertyController.GetAttack() * 10;
        dm.damageFrom = user;
        foreach (var chess in targets)
        {
            dm.damage = damage;
            dm.damageTo = chess;
            user.propertyController.TakeDamage(dm);
        }
        user.Death();
    }
    public void SearchTarget(Chess user, List<Chess> targets)
    {
        targets.Clear();
        Collider2D[] cols = CheckObjectPoolManage.GetColArray((int)(1000  ));
        LayerMask layer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int i = Physics2D.OverlapCircleNonAlloc(user.transform.position , checkRange, cols, layer);
        //Debug.Log("找到了" + i);
        for (int j = 0; j < i; j++)
        {
            Chess enemy = cols[j].GetComponent<Chess>();
            targets.Add(enemy);
        }
        CheckObjectPoolManage.ReleaseColArray(1000, cols);
    }
}
