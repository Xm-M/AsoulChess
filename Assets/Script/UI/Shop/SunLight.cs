using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 阳光 就是天上掉的 向日葵产的那种阳光
/// </summary>
public class SunLight : MonoBehaviour
{
    public int SunLightNum;//阳光数值
    public float fallSpeed;//掉落速度
    public float disapearTime;//消失时间
    public AudioSource au;//为什么你也有音效 
    bool ifPick;//是否被捡起来
    Vector2 target;
    Vector2 recyclePos;
    float t;
    public void InitSunLight(Tile target)
    {
        this.target = target.transform.position;
        recyclePos = (MapManage.instance as MapManage_PVZ).sunLightRecyclePos.position;
        ifPick = false;
        t = 0;
    }
    private void FixedUpdate()
    {
        if (!ifPick)
        {
            if (Vector2.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, fallSpeed * Time.fixedDeltaTime);
            }
            t += Time.fixedDeltaTime;
            if (t > disapearTime)
            {
                ObjectPool.instance.Recycle(gameObject);
            }
        }
    }
    private void OnMouseDown()
    {
        if (!ifPick)
        {
            PlantsShop.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycle());
            ifPick = true;
        }
    }
    public IEnumerator Recycle()
    {
        au.Play();
        while (Vector2.Distance(transform.position,recyclePos) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, recyclePos, fallSpeed * Time.fixedDeltaTime);
            yield return null;
        }
        ObjectPool.instance.Recycle(gameObject);
        //Debug.Log("回收");
    }
    private void OnMouseEnter()
    {
        if (!ifPick)
        {
            PlantsShop.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycle());
            ifPick = true;
        }
    }
}
