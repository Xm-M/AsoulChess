using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System;
 
 
public class AnimatorController_SpineAnimator : AnimatorController
{
    [Serializable]
    public class SpineAnimationData
    {
        public StateName state;//隶属那个状态机的动画
        public string animationName;//动画名字
        public int track = 0;//轨道默认0
        public bool loop;//是否循环
        public float delay;//延迟播放
        //public float loopContinueTime;//循环持续时间
    }
    [Serializable]
    public class SpineAnimationAudio
    {
        public StateName stateName;
        public AudioPlayer player;
    }


    public SkeletonAnimation sa;
    public List<SpineAnimationData> datas;
    public List<SpineAnimationAudio> audios;
    protected Dictionary<StateName, List<SpineAnimationData>> spineAnimDic;
    protected Dictionary<StateName, List<SpineAnimationAudio>> spineAudioDic;
    protected StateName currentStateName;
    protected bool complete = false;
    private void Start()
    {
        InitController(GetComponent<Chess>());
        PlayIdle();
    }
    public override void InitController(Chess chess)
    {
        base.InitController(chess);
        
        spineAnimDic = new Dictionary<StateName, List<SpineAnimationData>>();
        sa.AnimationState.Complete += OnComplete;
        sa.AnimationState.Event += OnSpineEvent;
        foreach (var data in datas)
        {
            if (!spineAnimDic.TryGetValue(data.state, out var list))
            {
                list = new List<SpineAnimationData>();
                spineAnimDic.Add(data.state, list);
            }
            list.Add(data);
        }
        spineAudioDic = new Dictionary<StateName, List<SpineAnimationAudio>>();
        foreach(var data in audios)
        {
            if(!spineAudioDic.TryGetValue(data.stateName,out var list))
            {
                list = new List<SpineAnimationAudio>();
                spineAudioDic.Add(data.stateName, list);
            }
            list.Add(data);
        }
    }
    void OnComplete(TrackEntry entry)
    {
        if (entry.TrackIndex != 0)
            return;

        // 非loop动画播放完成 
        if (!entry.Loop && entry.Next == null)
        {
            complete = true;
            if (currentStateName == StateName.DeathState) chess.Death();
        }
    }
    Timer looptimer;
    public override void PlayIdle()
    {
        PlaySpineAnim(StateName.IdleState);
    }
    public override void PlayAttack()
    {
        PlaySpineAnim(StateName.AttackState);
    }
    public override void PlaySkill()
    {
        PlaySpineAnim(StateName.SkillState);
    }
    public override void PlayDeath()
    {
        PlaySpineAnim(StateName.DeathState);
    }
    public override void PlayDizzy()
    {
        PlaySpineAnim(StateName.DizzyState);
    }
    //override 
    public virtual void PlaySpineAnim(StateName stateName)
    {
        complete = false;
        currentStateName = stateName;
        if (spineAudioDic.TryGetValue(stateName, out var l))
        {
            foreach (var item in l) item.player.RandomPlay(); 
        }
        if (!spineAnimDic.TryGetValue(stateName, out var list) || list.Count == 0)
            return;
        int track = list[0].track;
        // 关键：清掉当前轨道，防止排队越积越多
        sa.AnimationState.ClearTrack(list[0].track);

        // 关键：第一个用 SetAnimation 立刻切换
        var first = list[0];
        sa.AnimationState.SetAnimation(first.track, first.animationName, first.loop);

        // 后面的才 AddAnimation 作为队列
        for (int i = 1; i < list.Count; i++)
        {
            var anim = list[i];
            sa.AnimationState.AddAnimation(anim.track, anim.animationName, anim.loop, anim.delay);
        }

    }
   
    public override bool IfAnimPlayOver()
    {
        var entry = sa.AnimationState.GetCurrent(0);

        if (entry == null)
            return true;

        bool hasNext = entry.Next != null;

        // 情况1：当前是loop
        if (entry.Loop)
        {
            // 如果loop后面还有动画（说明是 Start->Loop->End）
            if (hasNext)
                return false;

            // 如果loop后面没有动画（Idle / AttackLoop）
            return true;
        }

        // 情况2：非loop动画
        return complete;   // complete由Complete回调设置
    }

    void OnSpineEvent(TrackEntry entry, Spine.Event e)
    {
        string eventName = e.Data.Name;

        switch (eventName)
        {
            case "OnAttack":
                if (currentStateName==StateName.AttackState)
                {
                    chess.TakeDamage();
                }
                else if(currentStateName==StateName.SkillState)
                {
                    chess.UseSkill();
                }
                break;

            case "OnStart":
                 
                break;
        }
    }


    //void Start() //测试用代码
    //{
    //    sa.AnimationState.Event += OnSpineEvent;
    //    //var data = sa.Skeleton.Data;

    //    //foreach (var ev in data.Events)
    //    //{
    //    //    Debug.Log("Event Defined: " + ev.Name);
    //    //}
    //    var data = sa.Skeleton.Data;
    //    var state = sa.AnimationState;

    //    if (data.Animations.Count == 0)
    //    {
    //        Debug.LogWarning("No animations found in this skeleton.");
    //        return;
    //    }

    //    // 清掉队列，避免叠加上一次的播放计划
    //    state.ClearTracks();

    //    // 先播放第一个
    //    var first = data.Animations.Items[0];
    //    state.SetAnimation(0, first.Name, false);

    //    // 把后续动画排队
    //    for (int i = 1; i < data.Animations.Count; i++)
    //    {
    //        var anim = data.Animations.Items[i];
    //        state.AddAnimation(0, anim.Name, false, 0f); // 0f = 紧接上一个
    //    }

    //    //// 最后回到 Idle（如果存在）
    //    //if (includeIdleAtEnd && data.FindAnimation("Idle") != null)
    //    //{
    //    //    state.AddAnimation(0, "Idle", true, 0f);
    //    //}
    //}
    //private void OnSpineEvent(Spine.TrackEntry entry,Spine.Event e)
    //{
    //    Debug.Log($"[SpineEvent] anim={entry.Animation.Name}, event={e.Data.Name}");
    //}

}
