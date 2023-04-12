using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBullet : MonoBehaviour
{
    public float damage;
    public float continueTime;
    public float damageTime = 0.5f;
    float dtimes;
    float times;
    private void Update()
    {
        times+=Time.deltaTime;
        dtimes+=Time.deltaTime;
        if(times > continueTime)
        {
            times = 0;
            ObjectPool.instance.Recycle(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.GetComponent<Chess>() != null && !CompareTag(collision.tag)&&dtimes>damageTime)
        {
            dtimes = 0;
            //collision.GetComponent<Chess>().GetDamage(damage*damageTime,null);
        }
    }
}
