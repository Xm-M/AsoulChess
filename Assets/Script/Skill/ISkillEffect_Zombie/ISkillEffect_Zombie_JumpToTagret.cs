using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        user.moveController.JumpToTarget(user, user.transform, startPos, endPos, maxHeight, speed);
        var map = MapManage.instance;
        var tile = user.moveController.standTile;
        int dx = Mathf.RoundToInt(Mathf.Sign(user.transform.right.x)) * (int)config.baseDamage[3];
        int newX = Mathf.Clamp(tile.mapPos.x + dx, 0, map.mapSize.x - 1);
        int newY = Mathf.Clamp(tile.mapPos.y, 0, map.mapSize.y - 1);
        // 若目标超出地图则取边缘格
        var targetTile = map.tiles[newX, newY];
        tile.ChessLeave(user);
        targetTile.ChessEnter(user);
    }

    
}
