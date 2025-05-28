using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矿工僵尸那种直接飞到最后排 然后转向
/// 思考一下
/// </summary>
public class SkillEffect_FlyToLast : ISkill
{
    public float moveTime;//固定的水平移动速度
    public float maxHeight;//竖直的最大高度
    float initVSpeed;//初始y速度 
    float gravity;//重力
    float t;
    float timeToApex;
    Vector2 startPosition, targetPosition;
    bool fly;
     
    IEnumerator FlyToLast(Chess user)
    {
        while (Vector2.Distance(user.transform.position, targetPosition) > 0.1f)
        {
            t += Time.deltaTime;
            float horizontalProgress = t / moveTime;
            float verticalProgress = -gravity * Mathf.Pow(t, 2) / 2 + initVSpeed * (t);
            Vector2 nextPos = Vector2.Lerp(startPosition, targetPosition, horizontalProgress);
            nextPos.y = startPosition.y + verticalProgress;
            user.transform.position = nextPos;
            yield return null;
        }
    }
    public bool IfSkillReady(Chess user) 
    {    
        Vector2Int mapsize = MapManage_PVZ.instance.mapSize;
        if (!fly&&user.moveController.standTile.mapPos.x == mapsize.x - 2)
        {
            UseSkill(user);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
         
    }

    public void UseSkill(Chess user)
    {
        fly = true;
        startPosition = user.transform.position;
        int y = user.moveController.standTile.mapPos.y;
        targetPosition = MapManage_PVZ.instance.tiles[0, y].transform.position;
    }

    public void WhenEnter(Chess user)
    {
        fly = false;
        t = 0;
        initVSpeed = 2 * maxHeight / moveTime;
        gravity = initVSpeed / (moveTime / 2);
        timeToApex = moveTime / 2;
 
    }
}
