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
    public int MaxHitNum=1;
    public float rate = 1;
    public UnityEvent<Bullet> WhenBulletHit;
    [SerializeReference]
    public IBulletEffect effect;
    [SerializeReference]
    public IBulletMove bulletMove;   
    public Chess shooter;
    public Vector2 startPos;
    public Vector2 targetPos;
    //public Chess lockTarget;
    public DamageMessege Dm;
    protected float damage;
    protected Chess hitChess;
    protected int current;
    private void Awake()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), RecycleBullet);
    }
    public virtual void InitBullet(Chess shooter,Vector3 position,Chess target,Vector2 moveDir,float damage=-1,float rate=1)
    {
        this.shooter = shooter;
        startPos = position;
        if(target!=null)
            targetPos = target.transform.position;
        //lockTarget = target;
        this.tag = shooter.tag;
        current = MaxHitNum;
        transform.position = startPos;
        transform.right=moveDir;
        Dm.damageFrom = shooter;
        this.rate = rate;
        if (damage == -1)
        {
            this.damage = shooter.propertyController.GetAttack() * rate;
           
        }
        else
        {
            this.damage = damage;
        }
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
            Dm.damage= damage * rate;
            Dm.damageTo = c;
            if (current > 0)
            {
                shooter.propertyController.TakeDamage(Dm);
                WhenBulletHit?.Invoke(this);
                effect?.OnBulletHit(this);
                current--;
            }
            
            if (current == 0)
            {
                RecycleBullet();
            }
        }
        
    }
    private void OnDestroy()
    {
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), RecycleBullet);
    }
}
public interface IBulletEffect
{
    public void OnBulletHit(Bullet bullet);
}