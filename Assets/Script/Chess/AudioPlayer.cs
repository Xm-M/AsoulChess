using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioController : Controller
{
    public List<AudioPlayer> audioPlayers;
    public Dictionary<string, AudioPlayer> playerMap;
 
    public void InitController(Chess chess)
    {
        playerMap = new Dictionary<string, AudioPlayer>();
        for(int i = 0; i < audioPlayers.Count; i++)
        {
            playerMap.Add(audioPlayers[i].playerName, audioPlayers[i]);
        }
    }
    public void PlayAudio(string name)
    {
        playerMap[name].PlayAudio();
    }
   

    public void WhenControllerEnterWar()
    {
         
            
    }

    public void WhenControllerLeaveWar()
    {
        throw new NotImplementedException();
    }
}


public class AudioPlayer : MonoBehaviour
{
    public string playerName;
    public AudioSource audioSource;
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
                audioSource.Play();
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
    public void PlayAudio()
    {
        audioSource.Play();
    }
}
