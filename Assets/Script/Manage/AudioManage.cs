using System;
using System.Collections.Generic;
using UnityEngine;
using AsoulChess.Game.Core.Audio;
using AsoulChess.Game.Core.Services;

public class AudioManage
{
    static IAudioService Service
    {
        get
        {
            if (GameServices.Audio == null)
                GameServices.RegisterDefaults();
            return GameServices.Audio;
        }
    }

    static readonly Dictionary<string, int> uniqueCount = new Dictionary<string, int>();
    static readonly Dictionary<string, AudioPlayer> uniqueCurrentPlayer = new Dictionary<string, AudioPlayer>();
    static readonly Dictionary<string, List<AudioPlayer>> uniqueHolders = new Dictionary<string, List<AudioPlayer>>();

    public static float SoundEffectValue => Service.SoundEffectVolume;
    public static float BgmValue => Service.BgmVolume;

    public AudioManage()
    {
        EventController.Instance.AddListener(EventName.PauseGame.ToString(), Pause);
        EventController.Instance.AddListener(EventName.ResumeGame.ToString(), Resume);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Stop);
    }

    public static void ChangeSoundEffect(float value) => Service.ChangeSoundEffect(value);

    public static void ChangeBGMValue(float value) => Service.ChangeBgm(value);

    public void Pause() => Service.PauseAll();

    public void Resume() => Service.ResumeAll();

    public void Stop() => Service.StopAll();

    public static void AddPlayer(AudioPlayer play) => Service.RegisterPlayer(play);

    public static void RemovePlayer(AudioPlayer audioPlayer) => Service.UnregisterPlayer(audioPlayer);

    public static bool AcquireUniqueLimited(string clipName, AudioPlayer player, int maxHolders, out bool registered)
        => Service.AcquireUniqueLimited(clipName, player, maxHolders, out registered);

    public static bool AcquireUnique(string clipName, AudioPlayer player)
    {
        if (!uniqueHolders.ContainsKey(clipName))
        {
            uniqueHolders[clipName] = new List<AudioPlayer>();
            uniqueCount[clipName] = 0;
        }

        var list = uniqueHolders[clipName];
        if (list.Contains(player)) return false;
        list.Add(player);
        uniqueCount[clipName]++;
        if (uniqueCount[clipName] == 1)
        {
            uniqueCurrentPlayer[clipName] = player;
            return true;
        }

        return false;
    }

    public static AudioPlayer ReleaseUnique(string clipName, AudioPlayer player)
    {
        var serviceNext = Service.ReleaseUnique(clipName, player) as AudioPlayer;
        if (serviceNext != null) return serviceNext;

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

        if (uniqueCurrentPlayer.TryGetValue(clipName, out var current) && current == player)
        {
            var next = list[0];
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
