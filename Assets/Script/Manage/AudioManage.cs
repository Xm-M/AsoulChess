using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AudioManage
{
    public static float SoundEffectValue { get; private set; }
    public static float BgmValue { get; private set; }

    static List<AudioPlayer> players;
    public AudioManage()
    {
        Debug.Log("≥ı ºªØ");
        players = new List<AudioPlayer>();
        EventController.Instance.AddListener(EventName.PauseGame.ToString(), Pause);
        EventController.Instance.AddListener(EventName.ResumeGame.ToString(), Resume);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Stop);
        SoundEffectValue = 1;
        BgmValue = 1;
    }
    public static void ChangeSoundEffect(float value)
    {
        SoundEffectValue = value;
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].autype == AudioType.SoundEffect)
            {
                players[i].audioSource.volume = SoundEffectValue;
            }
        }
    }
    public static void ChangeBGMValue(float value)
    {
        BgmValue = value;
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].autype == AudioType.BGM)
            {
                players[i].audioSource.volume = BgmValue;
            }
        }
    }
    public void Pause()
    {
        for(int i = 0; i < players.Count; i++)
        {
            players[i].Pause();
        }
    }
    public void Resume()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].UnPause();
        }
    }
    public void Stop()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Stop();
        }
    }
    public static void AddPlayer(AudioPlayer play)
    {
       
        if (!players.Contains(play))
        {
            players.Add(play);
        }
    }
    public static void RemovePlayer(AudioPlayer audioPlayer)
    {
        if (players.Contains(audioPlayer))
        {
            players.Remove(audioPlayer);
        }
    }
}
public enum AudioType
{
    SoundEffect,
    BGM,
}

