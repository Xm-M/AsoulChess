using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 抛物线移动方式
/// </summary>
public class BulletMoveParabola : IBulletMove
{
    public float moveTime;//固定的水平移动速度
    public float maxHeight;//竖直的最大高度
    Vector2 startPos;
    Vector2 endPos;
    float initVSpeed;//初始y速度
    float gravity;//重力
    float t;
    float timeToApex;
    Vector2 startPosition,targetPosition;  
    public void InitMove(Bullet bullet)
    {
        t = 0;
        initVSpeed=2*maxHeight/moveTime;
        gravity = initVSpeed / (moveTime / 2);
        timeToApex = moveTime / 2;
        startPosition = bullet.startPos;
        Debug.Log(startPosition);
        targetPosition = bullet.targetPos;
    }

    public void MoveBullet(Bullet bullet)
    {
        t+=Time.deltaTime;
        if (t<moveTime)
        {
            float horizontalProgress = t / moveTime;
            float verticalProgress = -gravity * Mathf.Pow(t, 2)/2 + initVSpeed* (t);
            Vector2 nextPos = Vector2.Lerp(startPosition, targetPosition, horizontalProgress);
            nextPos.y = startPosition.y + verticalProgress;
            bullet.transform.position = nextPos;
        }
        else
        {
            bullet.RecycleBullet();
        }
    }
}
