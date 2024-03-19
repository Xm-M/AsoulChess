using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMiss : MonoBehaviour
{
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
    public void Play()=>GetComponent<AudioSource>().Play();
    public void Off()=>gameObject.SetActive(false);
}
