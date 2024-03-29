using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ���� �������ϵ��� ���տ�������������
/// </summary>
public class SunLight : UIItem, IPointerEnterHandler
{
    public int SunLightNum;//������ֵ
    public float fallSpeed;//�����ٶ�
    public float disapearTime;//��ʧʱ��
    //public AudioSource au;//Ϊʲô��Ҳ����Ч 
    bool ifPick;//�Ƿ񱻼�����
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
        SunLightNum = num;//������ʵ����һ������num��С�ı������С�ĺ���
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
