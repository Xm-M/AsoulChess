using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunLight : MonoBehaviour
{
    public int SunLightNum;
    public float fallSpeed;
    public float disapearTime;
    public AudioSource au;
    bool ifPick;
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
            UIManage.GetView<PlantsShop>().ChangeSunLight(SunLightNum);
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
        //Debug.Log("ªÿ ’");
    }
    private void OnMouseEnter()
    {
        if (!ifPick)
        {
            UIManage.GetView<PlantsShop>().ChangeSunLight(SunLightNum);
            StartCoroutine(Recycle());
            ifPick = true;
        }
    }
}
