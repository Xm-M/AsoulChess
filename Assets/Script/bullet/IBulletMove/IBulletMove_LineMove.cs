using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBulletMove_LineMove : IBulletMove
{
    public LineRenderer line;
    float force;
    public float startMoveSpeed;
    float currentMoveSpeed;
    Vector2 startpos;
    //Bullet bullet1;

    public void InitMove(Bullet bullet)
    {
        //bullet1 = bullet;
        force = startMoveSpeed * startMoveSpeed / (2 * Vector2.Distance(bullet.transform.position,bullet.targetPos));
        startpos = bullet.transform.position;
        currentMoveSpeed = startMoveSpeed;
    }

    public void MoveBullet(Bullet bullet)
    {
        //bullet.transform.position = Vector2.MoveTowards(bullet.transform.position, bullet.targetPos, currentMoveSpeed * Time.deltaTime);
        bullet.transform.position = bullet.transform.position += (Vector3)bullet.transform.right*currentMoveSpeed * Time.deltaTime;
        currentMoveSpeed-=force*Time.deltaTime;
        if (line != null)
        {
            line.SetPosition(0,bullet.startPos);
            line.SetPosition(1,bullet.transform.position);
        }
        //Debug.Log(currentMoveSpeed);
        if (currentMoveSpeed < 0 && (Vector2.Distance(bullet.transform.position, startpos) < 0.25f||startpos.x- bullet.transform.position.x>bullet.transform.right.x))
        {
            bullet.RecycleBullet();
        }
    }
}
