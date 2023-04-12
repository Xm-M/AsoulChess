using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletEffectType
{
    AOE,
    Single,
}
public class Bullet : MonoBehaviour
{
    public Chess shooter;
    public float damage;
    
    public float moveSpeed = 10;
    public GameObject effect;
    Vector2 startPos;
    bool haveTakeDamage;
    BulletEffectType effectType;
    private void Awake()
    {
        EventController.Instance.AddListener(EventName.GameOver.ToString(), RecycleBullet);
    }
    private void OnEnable()
    {
        startPos = transform.position;
        haveTakeDamage = false;
    }
    public void ShootTo(float damage, Vector2 target, BulletEffectType bulletEffectType)
    {
        this.damage = damage;
        effectType = bulletEffectType;
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        StartCoroutine(Move(target));
    }
    public void ShootTo(float damage, Vector2 dir, float maxDis, BulletEffectType bulletEffectType)
    {
        this.damage = damage;
        effectType = bulletEffectType;
        Vector2 target= (Vector2)transform.position+dir.normalized *maxDis;
        StartCoroutine(Move(target));
    }
    IEnumerator Move(Vector2 target)
    {
        while (Vector2.Distance(transform.position, target) >= 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        RecycleBullet();
    }
    public virtual void HitTarget(Chess c)
    {
        if (effectType != BulletEffectType.Single || !haveTakeDamage)
        {
            //Debug.Log(name+"造成伤害"+c.name+damage);
            //shooter.TakeDamage(c, damage);
            WhenBulletTakeDamage(c);
        }
        if (effect != null) ObjectPool.instance.Create(effect).transform.position=c.transform.position;
        if (effectType == BulletEffectType.Single)
        {
            //StopAllCoroutines();
            RecycleBullet();
        }
    }
    public virtual void WhenBulletTakeDamage(Chess c)
    {

    }
    public void RecycleBullet() {
        StopAllCoroutines();
        if (gameObject.activeSelf)
            ObjectPool.instance.Recycle(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.LogWarning(collision.name+"碰到了");
        
        Chess t = collision.GetComponent<Chess>();
        //Debug.Log(collision.name + "碰到了" + damage+t.name);
        if (t!= null&&!collision.CompareTag(shooter.tag))
        {
            //Debug.Log("hit"+collision.name);
            HitTarget(t);
        }
    }
    private void OnDestroy()
    {
        EventController.Instance.RemoveListener(EventName.GameOver.ToString(), RecycleBullet);
    }
}
