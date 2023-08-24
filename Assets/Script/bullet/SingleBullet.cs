using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBullet : Bullet
{
    protected Chess target;
    protected Vector3 moveDir;
    public override void InitBullet(Chess chess)
    {
        base.InitBullet(chess);
        target = chess.equipWeapon.target;
        moveDir = chess.transform.right ;
    }
    //protected override void FixedUpdate()
    //{
    //    if (shooter == null || shooter.equipWeapon.target == null)
    //    {
    //        RecycleBullet();
    //        return;
    //    }
    //    float x = (transform.position.x + Time.fixedDeltaTime * moveSpeed - startPos.x) / (shooter.equipWeapon.target.transform.position.x - shooter.transform.position.x);
    //    float y = moveCurve.Evaluate(x) * maxHeight;
    //    if(CompareTag("Player"))
    //        transform.position = new Vector3(transform.position.x + Time.fixedDeltaTime * moveSpeed, startPos.y + y);
    //    else
    //    {
    //        transform.position = new Vector3(transform.position.x - Time.fixedDeltaTime * moveSpeed, startPos.y + y);
    //    }
    //}
    protected override void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, transform.position + moveDir, moveSpeed * Time.deltaTime);
        maxLength = Mathf.Abs(target.transform.position.x - startPos.x);
        float currentx = Mathf.Abs(transform.position.x - startPos.x);
        if (currentx < maxLength)
        {
            float y = moveCurve.Evaluate(currentx / maxLength) * maxHeight+startPos.y;
            transform.position = new Vector2(transform.position.x, y);
        }
        else
        {
            RecycleBullet();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CompareTag(collision.tag)&&current<MaxHitNum)
        {
            hitChess = collision.GetComponent<Chess>();
            if(hitChess.moveController.standTile.mapPos.y==shooter.moveController.standTile.mapPos.y){
                current++;
                shooter.equipWeapon.TakeDamage(hitChess);
                WhenBulletHit?.Invoke(this);
            }
        }
    }
    public override void RecycleBullet()
    {
        ObjectPool.instance.Recycle(gameObject);
    }
}

