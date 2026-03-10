using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManage:IManager
{
    public static float GameTime { get; private set; }

    /// <summary>
    /// 读档时恢复游戏时间，需在插件创建 Timer 之前调用
    /// </summary>
    public static void SetGameTimeForLoad(float gameTime)
    {
        GameTime = gameTime;
    }
    [Range(0f, 2f)]
    public float timeSpeed;
    List<Timer> timerList ;
    Queue<Timer> availableQueue ;
    public Timer AddTimer(Action onFinished,float delayTime,bool isLoop = false)
    {
        var timer = availableQueue.Count == 0 ? new Timer() : availableQueue.Dequeue();
        timer.Start(onFinished,delayTime,isLoop);
        timerList.Add(timer);
        return timer;
    }

    public void InitManage()
    {
        timerList = new List<Timer>();
        availableQueue = new Queue<Timer>();
        timeSpeed = 1;
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            ClearTime);
    }

    public void OnGameOver()
    {
        //ChangeTimeSpeed(1);
        for(int i = 0; i < timerList.Count; i++)
        {
            timerList[i].Stop();
            availableQueue.Enqueue(timerList[i]);
            timerList.Remove(timerList[i]);
        }

    }

    public void OnGameStart()
    {
        GameTime = 0;
        timerList.Clear();
        availableQueue.Clear();
    }

    public void Update()
    {
        if (LevelManage.instance.IfGameStart)
        {
            GameTime += Time.deltaTime;
            if (timerList.Count == 0) return;
            for (int i = 0; i < timerList.Count; i++)
            {
                if (timerList[i].IsFinish)
                {
                    availableQueue.Enqueue(timerList[i]);
                    timerList.Remove(timerList[i]);
                    continue;
                }
                else
                    timerList[i].Update();
            }
        }
    }
    public void ChangeTimeSpeed(float scale)
    {
        timeSpeed = scale;   
        Time.timeScale = scale;
    }
    public void ClearTime()
    {
        Debug.Log("清理计时器");
        timerList.Clear();
        availableQueue.Clear();
    }
}
public class Timer
{
    private Action OnFinish;
    private float finishTime;
    float delayTime;
    bool ifLoop;
    bool isFinish;
    public bool IsFinish=>isFinish;

    public void Start(Action onFinished,float delayTime,bool isLoop)
    {
        this.OnFinish = onFinished;
        finishTime = TimerManage.GameTime + delayTime;
        //Debug.Log("完成时间" + finishTime);
        
        this.delayTime = delayTime;
        ifLoop = isLoop;
        isFinish=false;
    }
    public void Stop()=>isFinish=true;
    public void Update()
    {
        if (isFinish) return;
        if (TimerManage.GameTime < finishTime) return;
        if (!ifLoop) Stop();
        else finishTime = TimerManage.GameTime + delayTime;
        OnFinish?.Invoke();
    }
    public float LeftTime()
    {
        return  finishTime- TimerManage.GameTime;
    }

    /// <summary>
    /// 供存档系统采集：剩余时间、是否循环
    /// </summary>
    public void GetSaveState(out float remainingTime, out bool isLoop)
    {
        remainingTime = LeftTime();
        isLoop = ifLoop;
    }
    public void ResetTime()
    {
        finishTime = TimerManage.GameTime + delayTime;
    }
    public void ResetTime(float t)
    {
        delayTime = t;
        finishTime =TimerManage.GameTime+t;
    }
    public void ChangeDelayTime(float newDelayTime)
    {
        finishTime = finishTime - delayTime+newDelayTime;
        delayTime = newDelayTime;
        //Debug.Log(newDelayTime);
        //Debug.Log("结束时间" + finishTime);
        //Debug.Log("现在时间" + TimerManage.GameTime);
    }
}
