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
    protected Chess shooter;
    protected Chess hitChess;
    protected Vector2 startPos;
    protected int current;

    public virtual void InitBullet(Chess shooter,Vector3 position)
    {
        this.shooter = shooter;
        startPos = position;
        this.tag = shooter.tag;
        current = MaxHitNum;
        transform.position = startPos;
        transform.right=shooter.transform.right;
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
        //Debug.Log(collision.name);
        
        if (!CompareTag(collision.tag) )
        {
            Chess c = collision.GetComponent<Chess>();
            if(c==null)return;
            DamageMessege damage=new DamageMessege(shooter,c ,
                    shooter.propertyController.GetAttack());
            if(current>0)
                shooter.propertyController.TakeDamage( damage);
            current--;
            if (current == 0)
            {
                RecycleBullet();
            }
        }
        
    }
}
