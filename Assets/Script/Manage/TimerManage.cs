using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManage
{
     List<Timer> timerList=new List<Timer> ();
     Queue<Timer> availableQueue=new Queue<Timer> ();
    public Timer AddTimer(Action onFinished,float delayTime,bool isLoop = false)
    {
        var timer = availableQueue.Count == 0 ? new Timer() : availableQueue.Dequeue();
        timer.Start(onFinished,delayTime,isLoop);
        timerList.Add(timer);
        return timer;
    }
    
    public void Update()
    {
        if(timerList.Count==0)return;

        for(int i = 0; i < timerList.Count; i++)
        {
            if (timerList[i].IsFinish)
            {
                availableQueue.Enqueue(timerList[i]);
                timerList.Remove(timerList[i]);
                continue;
            }else
                timerList[i].Update();
        }
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
        finishTime = Time.time + delayTime;
        //Debug.Log("完成时间" + finishTime);
        this.delayTime = delayTime;
        ifLoop = isLoop;
        isFinish=false;
    }
    public void Stop()=>isFinish=true;
    public void Update()
    {
        if (isFinish) return;
        if (Time.time < finishTime) return;
        if (!ifLoop) Stop();
        else finishTime = Time.time + delayTime;
        OnFinish?.Invoke();
    }
    public void ResetTime()
    {
        finishTime = Time.time + delayTime;
    }
}
