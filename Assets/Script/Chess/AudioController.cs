using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public List<AudioPlayer> audioPlayers;
    public Dictionary<string, AudioPlayer> playerMap;
    AudioPlayer currentPlay;
    public void InitController()
    {
        playerMap = new Dictionary<string, AudioPlayer>();
        for(int i = 0; i < audioPlayers.Count; i++)
        {
            playerMap.Add(audioPlayers[i].playerName, audioPlayers[i]);
        }
    }
    public void SetCurrentPlayer(string name)
    {
        currentPlay = playerMap[name];
    }
    public void PlayAudio(string name)
    {
        currentPlay.PlayAudio(name);
    }
    public void PlayAudio()
    {
        currentPlay.audioSource.Play();
    }
}
[Serializable]
public class AudioPlayer
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
}
