using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� �������ϵ��� ���տ�������������
/// </summary>
public class SunLight : MonoBehaviour
{
    public int SunLightNum;//������ֵ
    public float fallSpeed;//�����ٶ�
    public float disapearTime;//��ʧʱ��
    public AudioSource au;//Ϊʲô��Ҳ����Ч 
    bool ifPick;//�Ƿ񱻼�����
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
        //Debug.Log("����");
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
