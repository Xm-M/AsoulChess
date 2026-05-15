using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 0:高度 1：跳跃速度  [2] 世界空间水平飞行距离、[3] 落地后占格的横向偏移（格数）
/// </summary>
public class ISkillEffect_Zombie_JumpToTagret : ISkillEffect
{
    //public float dis;
    //public int nextTile = 1;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
         if (user?.moveController?.standTile == null) return;

        var startPos = (Vector2)user.transform.position;
        //var endPos = startPos + (Vector2)user.transform.right * config.baseDamage[2];
        var endPos = new Vector2(startPos.x + ((Vector2)user.transform.right * config.baseDamage[2]).x, user.moveController.standTile.transform.position.y  );
        //Debug.Log(endPos);
        var maxHeight = config.baseDamage[0];
        var speed = config.baseDamage[1];
        var map = MapManage.instance;
        var tile = user.moveController.standTile;
        int dx = Mathf.RoundToInt(Mathf.Sign(user.transform.right.x)) * (int)config.baseDamage[3];
        int newX = Mathf.Clamp(tile.mapPos.x + dx, 0, map.mapSize.x - 1);
        int newY = Mathf.Clamp(tile.mapPos.y, 0, map.mapSize.y - 1);
        // 若目标超出地图则取边缘格
        var targetTile = map.tiles[newX, newY];
        user.moveController.standTile = targetTile;
        user.moveController.JumpToTarget(user, user.transform, startPos, endPos, maxHeight, speed, () =>
        {
            if (user == null || user.IfDeath || tile == null || targetTile == null)
                return;
            //tile.ChessLeave(user);
            //targetTile.ChessEnter(user);
        });
    }

    
}
