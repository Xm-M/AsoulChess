using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 阳光 就是天上掉的 向日葵产的那种阳光
/// </summary>
public class SunLight : UIItem, IPointerEnterHandler
{
    public int SunLightNum;//阳光数值
    public float fallSpeed;//掉落速度
    public float disapearTime;//消失时间
    //public AudioSource au;//为什么你也有音效 
    bool ifPick;//是否被捡起来
    Vector2 target;
    Vector2 recyclePos;
    float t;
    public void InitSunLight(Tile target)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        this.target = target.transform.position;
        transform.position= screenPos;
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        t = 0;
    }
    public void InitSunLight(Tile target,int num)
    {
        InitSunLight(target);
        SunLightNum = num;//这里其实还有一个根据num大小改变阳光大小的函数
    }
    public IEnumerator Recycles()
    {
        UIManage.GetView<PlantsShop>().shopAudio.PlayAudio("SunLight");
        while (Vector2.Distance(transform.position,recyclePos) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, recyclePos, fallSpeed * Time.fixedDeltaTime);
            yield return null;
        }
        Recycle();
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!ifPick)
        {
            SunLightPanel.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycles());
            ifPick = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ifPick)
        {
            SunLightPanel.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycles());
            ifPick = true;
        }
    }

    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<SunLight>(this);
    }
}
