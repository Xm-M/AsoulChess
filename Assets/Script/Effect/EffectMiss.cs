using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMiss : MonoBehaviour
{
    public GameObject Effect;
    public void CreateEffect()
    {
        ObjectPool.instance.Create(Effect).transform.position=transform.position;
    }
    private void Start()
    {
        if(GetComponent<SpriteRenderer>()!=null)
        GetComponent<SpriteRenderer>().sortingLayerName = "Effect";
    }
    public void Miss()
    {
        if(gameObject.activeSelf)
            ObjectPool.instance.Recycle(gameObject);
    }
    //public void PlayAudio()=>GetComponent<AudioSource>().Play();
    public void Off()=>gameObject.SetActive(false);
}
