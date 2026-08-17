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

    [Header("奖励式弹出（与 Item_Reward / RewardItemBase.SetRewardPos 同曲线）")]
    [Tooltip("与 RewardItemBase.totalTime 一致")]
    public float rewardPopTotalTime = 1f;
    [Tooltip("与 RewardItemBase.height 一致")]
    public float rewardPopHeight = 1.25f;
    [Tooltip("与 RewardItemBase.moveSpeed 一致")]
    public float rewardPopMoveSpeed = 1f;
    [Tooltip("与 RewardItemBase.timeSpeed 一致")]
    public float rewardPopTimeSpeed = 2f;

    //public AudioSource au;//为什么你也有音效 
    bool ifPick;//是否被捡起来
    Vector2 target;
    Vector2 recyclePos;
    Timer timer;
    public void InitSunLight(Tile target)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        this.target = target.transform.position;
        transform.position= screenPos;
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        timer = GameManage.instance.timerManage.AddTimer(PickSunlight, 3);
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
        timer = GameManage.instance.timerManage.AddTimer(PickSunlight, 3);
        StartCoroutine(Move(screenstartPos,screenPos));
    }
    public void InitSunLight(int num, Vector3 startPos, float totalTime, float x0, float height,float moveSpeed,float timeSpeed)
    {
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        SunLightNum = num;
        StartCoroutine(CurveMove(startPos,totalTime,x0,height,moveSpeed,timeSpeed));
        timer = GameManage.instance.timerManage.AddTimer(PickSunlight, 3);
    }

    /// <summary>
    /// 与 <see cref="RewardItemBase.SetRewardPos"/> 相同的曲线弹出，再停留待拾取（不暂停游戏、无自动回收计时器）。
    /// </summary>
    public void InitSunLightDropPop(Tile targetTile, int num, Vector3 worldStartPos)
    {
        if (targetTile == null || SunLightPanel.instance == null || Camera.main == null) return;
        SunLightNum = num;
        recyclePos = SunLightPanel.instance.sunLightText.transform.position;
        ifPick = false;
        target = targetTile.transform.position;

        worldStartPos = RewardItemBase.ClampRewardPosToVisible(worldStartPos);
        float speed = rewardPopMoveSpeed;
        var mapPvz = MapManage_PVZ.instance;
        if (mapPvz != null && mapPvz.tiles != null && MapManage.instance != null)
        {
            var size = MapManage.instance.mapSize;
            Vector2 center = mapPvz.tiles[size.x / 2, size.y / 2].transform.position;
            if (worldStartPos.x > center.x) speed *= -1f;
        }
        timer = GameManage.instance.timerManage.AddTimer(PickSunlight, 3);
        StartCoroutine(CurveMove(worldStartPos, rewardPopTotalTime, 0f, rewardPopHeight, speed, rewardPopTimeSpeed));
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
            StopPickTimer();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ifPick)
        {
            SunLightPanel.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycles());
            ifPick = true;
            StopPickTimer();
        }
    }
    public void PickSunlight()
    {
        // Destroy 后 Unity 伪 null：避免清场后残留 Timer 回调再 StartCoroutine
        if (this == null) return;
        if (!ifPick)
        {
            
            SunLightPanel.instance.ChangeSunLight(SunLightNum);
            StartCoroutine(Recycles());
            ifPick = true;
            StopPickTimer();
        }
    }

    void OnDisable()
    {
        StopPickTimer();
    }

    void StopPickTimer()
    {
        if (timer == null) return;
        timer.Stop();
        timer = null;
    }

    public override void Recycle()
    {
        StopPickTimer();
        //Debug.Log("回收阳光");
        UIManage.GetView<ItemPanel>().Recycle<SunLight>(this);
    }
}
