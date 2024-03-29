using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 最大的问题是怎么设计啊
/// </summary>
public class Bullet : MonoBehaviour
{
    public int MaxHitNum;
    public UnityEvent<Bullet> WhenBulletHit;
    [SerializeReference]
    public IBulletMove bulletMove;   
    public Chess shooter;
    public Vector2 startPos;
    public Vector2 targetPos;
    public Chess lockTarget;
    public DamageMessege Dm;
    protected Chess hitChess;
    protected int current;

    public virtual void InitBullet(Chess shooter,Vector3 position,Chess target,Vector2 moveDir)
    {
        this.shooter = shooter;
        startPos = position;
        targetPos = target.transform.position;
        lockTarget = target;
        this.tag = shooter.tag;
        current = MaxHitNum;
        transform.position = startPos;
        transform.right=moveDir;
        Dm.damage = shooter.propertyController.GetAttack();
        Dm.damageFrom = shooter;
        bulletMove.InitMove(this);
    } 
    protected virtual void Update()
    {
        bulletMove.MoveBullet(this);    
    }
    public virtual void RecycleBullet() {
        ObjectPool.instance.Recycle(gameObject);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("bulllet hit "+collision.name);
        
        if (!CompareTag(collision.tag) )
        {
            Chess c = collision.GetComponent<Chess>();
            if(c==null)return;
            Dm.damageTo = c; 
            if(current>0)
                shooter.propertyController.TakeDamage(Dm);
            current--;
            if (current == 0)
            {
                RecycleBullet();
            }
        }
        
    }
}
