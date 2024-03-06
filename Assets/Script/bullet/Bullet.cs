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

    public virtual void InitBullet(Chess chess,Vector3 position)
    {
        this.shooter = chess;
        startPos = position;
        this.tag = chess.tag;
        current = MaxHitNum;
        transform.position = startPos;
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
        if (!CompareTag(collision.tag))
        {
            current--;
            if (current >= 0)
            {
                shooter.equipWeapon.TakeDamages();
            }else if(current == -1)
            {
                RecycleBullet();
            }
        }
        
    }
}
