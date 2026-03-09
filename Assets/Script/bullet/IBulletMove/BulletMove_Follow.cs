using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMove_Follow : IBulletMove
{
    public Transform target;           // 跟踪目标
    public float moveSpeed = 10f;      // 子弹前进速度
    public float turnSpeed = 10f;     // 最大转向角速度（度/秒）

    private Vector3 currentDirection;  // 当前方向（单位向量）
    public void InitMove(Bullet bullet)
    {
        currentDirection=bullet.transform.right;
        //target = bullet.Dm.damageTo.transform;
    }
    /// <summary>
    /// 这个我下午来重写一下吧 这写的是个篮子
    /// </summary>
    /// <param name="bullet"></param>
    public void MoveBullet(Bullet bullet)
    {
        Chess c = bullet.Dm.damageTo;
        
        if (c == null||c.IfDeath)
        {
            bullet.Dm.damageTo = null;
            bullet.transform.position = Vector2.MoveTowards(bullet.transform.position, bullet.transform.position + bullet.transform.right
                , moveSpeed * Time.deltaTime);
            return;
        }
        else
        {
            target = bullet.Dm.damageTo.transform;
        }

        Vector2 toTarget = (target.position - bullet. transform.position).normalized;

        // 计算当前方向与目标方向之间的角度
        float angleToTarget = Vector2.SignedAngle(currentDirection, toTarget);

        // 限制转向角度
        float maxRotation = turnSpeed * Time.deltaTime;
        float rotation = Mathf.Clamp(angleToTarget, -maxRotation, maxRotation);

        // 应用旋转
        currentDirection = Quaternion.Euler(0, 0, rotation) * currentDirection;
        currentDirection.Normalize();

        // 更新位置
        bullet.transform.position += (Vector3)(currentDirection * moveSpeed * Time.deltaTime);

        // 设置物体旋转角度（让子弹朝向当前方向）
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
