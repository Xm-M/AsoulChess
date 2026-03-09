using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMiss : MonoBehaviour
{
    public GameObject Effect;
    public void CreateEffect()
    {
        ObjectPool.instance.Create(Effect).transform.position = transform.position;
    }
    public void CreateEffects(GameObject Effect)
    {
        ObjectPool.instance.Create(Effect).transform.position = transform.position;
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
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), Miss);
    }
    //public void PlayAudio()=>GetComponent<AudioSource>().Play();
    public void Off()=>gameObject.SetActive(false);
}
