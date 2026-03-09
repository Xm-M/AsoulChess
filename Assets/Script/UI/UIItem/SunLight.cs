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
    public AnimationCurve curve;
    public float fallSpeed;//掉落速度
    public float disapearTime;//消失时间
    public float jumpHeight = 80f;
    //public AudioSource au;//为什么你也有音效 
    bool ifPick;//是否被捡起来
    Vector2 target;
    Vector2 recyclePos;
    //float t;
    public void InitSunLight(Tile target)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        this.target = target.transform.position;
        transform.position= screenPos;
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        //t = 0;
    }
    public void InitSunLight(Tile target,int num)
    {
        InitSunLight(target);
        SunLightNum = num;//这里其实还有一个根据num大小改变阳光大小的函数
    }
    public void InitSunLight(Tile target,int num,Vector3 startPos)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        Vector2 screenstartPos=Camera.main.WorldToScreenPoint(startPos);
        this.target = target.transform.position;
        //transform.position = screenPos;
        SunLightNum = num;
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        //t = 0;
        StartCoroutine(Move(screenstartPos,screenPos));
    }
    public void InitSunLight(int num, Vector3 startPos, float totalTime, float x0, float height,float moveSpeed,float timeSpeed)
    {
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        SunLightNum = num;
        StartCoroutine(CurveMove(startPos,totalTime,x0,height,moveSpeed,timeSpeed));
    }

    IEnumerator CurveMove(Vector3 startPos,float totalTime, float x0, float height, float moveSpeed,float timeSpeed)
    {

        transform.position = Camera.main.WorldToScreenPoint(startPos);
        float elapsed = 0f;
        float t=x0;
        float y0 = curve.Evaluate(t);
        //Debug.Log(x0 + " " + y0);
        float starty = startPos.y;
        while (elapsed < totalTime)
        {
            //t(curve的x参数)=elspsed(实际经过的时间)/总时间
            t = elapsed + x0;
            float yOffset = (curve.Evaluate(t)-y0);
            startPos= new Vector2(startPos.x + moveSpeed * Time.deltaTime, starty + (yOffset * height));
            transform.position = Camera.main.WorldToScreenPoint(startPos);
            elapsed += Time.deltaTime*timeSpeed;
            
            yield return null;
        }
        
        // 最终归位
        //rectTransform.anchoredPosition = targetPos;
    }
    IEnumerator Move(Vector2 startPos,Vector2 endPos)
    {
        transform.position = startPos;
        while (!ifPick &&Vector2.Distance(transform.position,endPos)>0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position,endPos,fallSpeed*Time.deltaTime);
            yield return null;
        }
    }
    public IEnumerator Recycles()
    {
        //UIManage.GetView<PlantsShop>().shopAudio.PlayAudio("SunLight");
        UIManage.GetView<ItemPanel>().player.PlayAudio("SunLight");
        while (Vector2.Distance(transform.position,recyclePos) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, recyclePos, fallSpeed * Time.deltaTime*3);
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
        //Debug.Log("回收阳光");
        UIManage.GetView<ItemPanel>().Recycle<SunLight>(this);
    }
}
