using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AudioManage :MonoBehaviour
{
    public AudioSource audioSource;
    [Serializable]
    public class AudioName
    {
        public string name;
        public AudioClip clip;
    }
    public List<AudioName> clips;

    public void PlayAudio(string audioname)
    {
        //Debug.Log("play "+audioname);
        for(int i = 0; i < clips.Count; i++)
        {
            if(clips[i].name == audioname)
            {
                audioSource.clip=clips[i].clip;
                audioSource.Play();
            }
        }
    }
    public void Stop()
    {
        audioSource.Stop();
    }
}

