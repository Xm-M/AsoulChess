using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManage
{
    public static float SoundEffectValue { get; private set; }
    public static float BgmValue { get; private set; }

    static List<AudioPlayer> players;
    /// <summary>全局唯一播放：每个音效名对应引用计数，num>0 时保持播放，num==0 时停止；停止时若有其他持有者则交接给下一个</summary>
    static Dictionary<string, int> uniqueCount = new Dictionary<string, int>();
    static Dictionary<string, AudioPlayer> uniqueCurrentPlayer = new Dictionary<string, AudioPlayer>();
    static Dictionary<string, List<AudioPlayer>> uniqueHolders = new Dictionary<string, List<AudioPlayer>>();
    public AudioManage()
    {
        Debug.Log("初始化");
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
        uniqueCount.Clear();
        uniqueCurrentPlayer.Clear();
        uniqueHolders.Clear();
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

    /// <summary>
    /// 占用全局唯一播放：num++。若 num 从 0 变为 1 返回 true（需播放），否则返回 false（仅登记）
    /// </summary>
    public static bool AcquireUnique(string clipName, AudioPlayer player)
    {
        if (!uniqueHolders.ContainsKey(clipName))
        {
            uniqueHolders[clipName] = new List<AudioPlayer>();
            uniqueCount[clipName] = 0;
        }
        var list = uniqueHolders[clipName];
        if (list.Contains(player)) return false; // 已登记过，不再重复
        list.Add(player);
        uniqueCount[clipName]++;
        if (uniqueCount[clipName] == 1)
        {
            uniqueCurrentPlayer[clipName] = player;
            return true; // 第一个，需要播放
        }
        return false;
    }

    /// <summary>
    /// 释放全局唯一播放：num--。若 num==0 停止；若 num>0 且当前播放者是自己，则交接给下一个持有者
    /// 返回应接替播放的 AudioPlayer，无则返回 null
    /// </summary>
    public static AudioPlayer ReleaseUnique(string clipName, AudioPlayer player)
    {
        if (!uniqueHolders.ContainsKey(clipName)) return null;
        var list = uniqueHolders[clipName];
        list.Remove(player);
        uniqueCount[clipName]--;

        if (uniqueCount[clipName] <= 0)
        {
            uniqueHolders.Remove(clipName);
            uniqueCount.Remove(clipName);
            uniqueCurrentPlayer.Remove(clipName);
            return null;
        }

        // 若当前是自己播的，交接给下一个
        if (uniqueCurrentPlayer.TryGetValue(clipName, out var current) && current == player)
        {
            var next = list[0]; // 取第一个仍在列表中的
            uniqueCurrentPlayer[clipName] = next;
            return next;
        }
        return null;
    }
}
public enum AudioType
{
    SoundEffect,
    BGM,
}

