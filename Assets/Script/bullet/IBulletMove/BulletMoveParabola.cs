using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 抛物线移动方式
/// 抛物线子弹在移动过程中 是没有碰撞体积的 只有在快到目的地的时候才有（或者击中目标的时候）
/// 我真的是日你哥了
/// 如果抛物线没有目标的话 就会直线飞行
/// 还要写一个可以不同行的抛物线
/// 
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
        //Debug.Log(startPosition);       
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
public class BulletMoveParabola_Continue : IBulletMove
{
    public float continueTime;//持续时间
    public float moveTime;
    public float height;//竖直的最大高度
    Vector2 startPos;
    Vector2 endPos;
    float progress;
    Vector2 startPosition;
    float t;
    float speed;//固定的水平移动速度
    public void InitMove(Bullet bullet)
    {
        startPos = bullet.startPos;
        endPos = bullet.targetPos;
        //targetPosition = bullet.targetPos;
        progress = 0;
        t = 0;
        speed = Mathf.Abs(endPos.x - startPos.x) / moveTime;
    }
    
    public void MoveBullet(Bullet bullet)
    {
        if (progress < 1)
        {
            progress += speed * Time.deltaTime / Vector2.Distance(startPos, endPos);
            if (progress > 1f) progress = 1f; // 确保进度不超出 1

            // 根据进度计算当前位置
            Vector2 position = CalculateParabolicPosition(startPos, endPos, height, progress);
            bullet.transform.position = position;
            SpriteRenderer render = bullet.GetComponent<SpriteRenderer>();
            if (render != null)
                render.material.renderQueue = 3000 - (int)(bullet.transform.position.y * 15);
        }
        else
        {
            // 结束条件
            t += Time.deltaTime;
            if (t > continueTime)
            {
                t = 0;
                bullet.WhenBulletHit?.Invoke(bullet);
                bullet.RecycleBullet();
            }
        }
    }
    /// <summary>
    /// 计算抛物线上的位置
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="height">抛物线最高点的高度</param>
    /// <param name="t">当前进度 (0-1)</param>
    /// <returns>抛物线上的点</returns>
    private Vector2 CalculateParabolicPosition(Vector2 start, Vector2 end, float height, float t)
    {
        // 水平线性插值
        Vector2 horizontalPosition = Vector2.Lerp(start, end, t);

        // 抛物线高度计算
        float parabolicHeight = 4 * height * t * (1 - t); // 顶点在 t=0.5

        // 返回最终位置
        return new Vector2(horizontalPosition.x, horizontalPosition.y + parabolicHeight);
    }
}