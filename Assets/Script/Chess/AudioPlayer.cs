using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class AudioPlayer : MonoBehaviour
{
    public AudioType autype;
    public AudioSource audioSource;
    public List<GameObject> subAudio;
    public float baseValue;
    private void Awake()
    {
        baseValue= audioSource.volume;
    }
    private void OnEnable()
    {
       
        if(autype==AudioType.SoundEffect)
            audioSource.volume=AudioManage.SoundEffectValue*baseValue;
        else
            audioSource.volume=AudioManage.BgmValue * baseValue;
        AudioManage.AddPlayer(this);
    }
    private void OnDisable()
    {
        AudioManage.RemovePlayer(this);
    }
    [Serializable]
    public class NameAudioClip
    {
        public string name;
        public AudioClip clip;
    }
    public List<NameAudioClip> clipList;
     public void PlayAudio(string name)
    {
        for(int i = 0; i < clipList.Count; i++)
        {
            if (name == clipList[i].name)
            {
                audioSource.clip = clipList[i].clip;
                Play();
                return;
            }
        }
    }
    public void RandomPlay()
    {
        int n=UnityEngine.Random.Range(0,clipList.Count);
        audioSource.clip = clipList[n].clip;
        Play();
    }
    public void ChangeAudio(string name)
    {
        for (int i = 0; i < clipList.Count; i++)
        {
            if (name == clipList[i].name)
            {
                audioSource.clip = clipList[i].clip;
                return;
            }
        }
    }
    [Button("播放")]
    public void Play()
    {
        audioSource.Play();
    }
    public void PlayWithSub()
    {
        audioSource.Play();
        for(int i=0;i<subAudio.Count;i++)
            subAudio[i].GetComponent<AudioPlayer>().RandomPlay(); 
    }
    public void PlaySub()
    {
        for (int i = 0; i < subAudio.Count; i++)
            subAudio[i].GetComponent<AudioPlayer>().RandomPlay();
    }
    public void PlaySub(int n,string audioName)
    {
        subAudio[n].GetComponent<AudioPlayer>().PlayAudio(audioName);
    }
    public void PlaySubN(int n)
    {
        subAudio[n].GetComponent<AudioPlayer>().RandomPlay();
    }
    public void Stop()
    {
        audioSource.Stop();
    }
    public void Pause()
    {
        audioSource.Pause();
    }
    public void UnPause()
    {
        //if (autype == AudioType.SoundEffect)
        //    audioSource.volume = AudioManage.SoundEffectValue;
        //else
        //    audioSource.volume = AudioManage.BgmValue;
        audioSource.UnPause();
    }
    public void SetLoop(bool loop)
    {
        audioSource.loop = loop;
    }
}
