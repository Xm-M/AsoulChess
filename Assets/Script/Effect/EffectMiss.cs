using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMiss : MonoBehaviour
{
    public GameObject Effect;

    [Header("圆形持续刷特效 CreateEffectInCircle")]
    public float radius = 1f;
    public float continueTime = 3f;
    public float interval = 0.5f;
    public int createNumOnce = 3;
    public Transform center;

    Coroutine circleSpawnRoutine;

    public void CreateEffect()
    {
        var obj = ObjectPool.instance.Create(Effect);
        obj.transform.position = transform.position;
        ConfigureSpawnedEffect(obj);
    }
    public void CreateEffects(GameObject Effect)
    {
        var obj = ObjectPool.instance.Create(Effect);
        obj.transform.position = transform.position;
        ConfigureSpawnedEffect(obj);
    }

    /// <summary>对象池取出后、落位后可覆写（如僵王口气 Blend）。</summary>
    protected virtual void ConfigureSpawnedEffect(GameObject obj) { }

    /// <summary>
    /// 在圆盘内持续刷特效：t=0 立即一批，之后每 <see cref="interval"/> 秒再刷 <see cref="createNumOnce"/> 个，共持续 <see cref="continueTime"/> 秒。
    /// 圆心为 <see cref="center"/>，为空则用自身 <see cref="Transform"/>。
    /// </summary>
    public void CreateEffectInCircle()
    {
        StopCircleSpawn();
        if (Effect == null || continueTime <= 0f || createNumOnce <= 0)
            return;
        circleSpawnRoutine = StartCoroutine(CircleSpawnRoutine());
    }

    IEnumerator CircleSpawnRoutine()
    {
        float elapsed = 0f;
        while (elapsed < continueTime)
        {
            SpawnEffectBatchInCircle();
            if (interval <= 0f)
                yield break;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        circleSpawnRoutine = null;
    }

    void SpawnEffectBatchInCircle()
    {
        if (Effect == null || createNumOnce <= 0 || radius < 0f)
            return;

        Vector3 origin = (center != null ? center : transform).position;
        for (int i = 0; i < createNumOnce; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            var pos = origin + new Vector3(offset.x, offset.y, 0f);
            GameObject obj = ObjectPool.instance.Create(Effect);
            obj.transform.position = pos;
            Transform facing = center != null ? center : transform;
            obj.transform.right = facing.right;
            ConfigureSpawnedEffect(obj);
        }
    }

    void StopCircleSpawn()
    {
        if (circleSpawnRoutine != null)
        {
            StopCoroutine(circleSpawnRoutine);
            circleSpawnRoutine = null;
        }
    }

    private void Awake()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Miss);
    }
    protected virtual void Start()
    {
        //if(GetComponent<SpriteRenderer>()!=null)
        //GetComponent<SpriteRenderer>().sortingLayerName = "Effect";
        
    }
    public void Miss()
    {
        StopCircleSpawn();
        if(gameObject.activeSelf)
            ObjectPool.instance.Recycle(gameObject);
    }
    public void DestroySelf()
    {
        Debug.Log("调用");
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        StopCircleSpawn();
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), Miss);
    }
    //public void PlayAudio()=>GetComponent<AudioSource>().Play();
    public void Off()=>gameObject.SetActive(false);
}
