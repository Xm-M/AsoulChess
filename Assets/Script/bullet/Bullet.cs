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
    public Chess shooter;
    public float moveSpeed = 10;
    public float maxHeight, maxLength;
    public int MaxHitNum;
    public UnityEvent<Bullet> WhenBulletHit;
    public AnimationCurve moveCurve;
    public Chess hitChess;
    protected Vector2 startPos;
    protected int current;
    protected LayerMask targerLayer;

    public virtual void InitBullet(Chess chess)
    {
        this.shooter = chess;
        startPos = transform.position;
        this.tag = chess.tag;

        current = 0;
    } 
    protected virtual void Update()
    {

    }
    protected virtual void FixedUpdate()
    {
        
        
    }
    public virtual void RecycleBullet() {
        ObjectPool.instance.Recycle(gameObject);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        if (!CompareTag(collision.tag))
        {
            shooter.equipWeapon.TakeDamage(collision.GetComponent<Chess>());
        }
        RecycleBullet();
    }
}
