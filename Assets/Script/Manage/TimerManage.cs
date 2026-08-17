using System;
using System.Collections.Generic;
using UnityEngine;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.Timing;

public class TimerManage : IManager
{
    static readonly float[] SpeedPresets = { 1f, 2f, 3f };

    IGameplayTimerService Service
    {
        get
        {
            if (GameServices.GameTimers == null)
                GameServices.RegisterDefaults();
            return GameServices.GameTimers;
        }
    }

    [Range(0f, 2f)]
    public float timeSpeed;

    int currentSpeedIndex;
    float lastNonZeroSpeed = 1f;

    public static float GameTime => GameServices.GameTimers?.GameTime ?? 0f;

    public static void SetGameTimeForLoad(float gameTime)
    {
        if (GameServices.GameTimers == null)
            GameServices.RegisterDefaults();
        GameServices.GameTimers.SetGameTimeForLoad(gameTime);
    }

    public Timer AddTimer(Action onFinished, float delayTime, bool isLoop = false)
    {
        return (Timer)Service.AddTimer(onFinished, delayTime, isLoop);
    }

    public void InitManage()
    {
        timeSpeed = 1;
        if (Service is GameplayTimerService gameplay)
            gameplay.SetTimerFactory(() => new Timer());
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), ClearTime);
    }

    public void OnGameOver() => ClearTime();

    public void OnGameStart()
    {
        Service.ClearAll();
        Service.SetGameTimeForLoad(0f);
    }

    public void Update()
    {
        if (LevelManage.instance != null)
            Service.IsSimulationRunning = LevelManage.instance.IfGameStart;

        Service.Update();
    }

    public void ChangeTimeSpeed(float scale)
    {
        timeSpeed = scale;
        Service.TimeSpeed = scale;
        Time.timeScale = scale;
        if (scale > 0) lastNonZeroSpeed = scale;
    }

    public void CycleSpeed()
    {
        if (LevelManage.instance == null || !LevelManage.instance.IfGameStart)
            return;
        currentSpeedIndex = (currentSpeedIndex + 1) % SpeedPresets.Length;
        float s = SpeedPresets[currentSpeedIndex];
        lastNonZeroSpeed = s;
        ChangeTimeSpeed(s);
    }

    public float GetResumeSpeed() => lastNonZeroSpeed;

    public int GetCurrentSpeedIndex() => currentSpeedIndex;

    public void ClearTime()
    {
        Debug.Log("清理计时器");
        Service.ClearAll();
        currentSpeedIndex = 0;
        lastNonZeroSpeed = 1f;
        ChangeTimeSpeed(1f);
    }
}

public class Timer : GameTimer { }
