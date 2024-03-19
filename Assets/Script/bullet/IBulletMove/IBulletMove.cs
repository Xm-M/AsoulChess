using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBulletMove 
{
    public void InitMove(Bullet bullet);
    public void MoveBullet(Bullet bullet);
}
