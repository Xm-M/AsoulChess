using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBulletMoveRight : IBulletMove
{
    public float speed;

    public void InitMove(Bullet bullet)
    {
         
    }

    public void MoveBullet(Bullet bullet)
    {
        Vector2 pos=bullet.transform.position;
        bullet.transform.position = Vector2.MoveTowards(
            pos, pos + (Vector2)bullet.transform.right, speed * Time.deltaTime
            );
    }
}
