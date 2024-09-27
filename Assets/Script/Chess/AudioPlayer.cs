using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioType autype;
    public AudioSource audioSource;
    private void OnEnable()
    {
        if(autype==AudioType.SoundEffect)
            audioSource.volume=AudioManage.SoundEffectValue;
        else
            audioSource.volume=AudioManage.BgmValue;
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
    public void Play()
    {
        //if(autype==AudioType.SoundEffect)
        //    audioSource.volume=AudioManage.SoundEffectValue;
        //else
        //    audioSource.volume=AudioManage.BgmValue;
        audioSource.Play();
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
}
