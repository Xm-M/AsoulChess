using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// Timeline控制器，提供播放控制、时间跳转、事件监听等功能
/// </summary>
public class TimelineController : MonoBehaviour
{
    [Header("Timeline配置")]
    [SerializeField] private PlayableDirector director;
    [SerializeField] private TimelineAsset timelineAsset;
    
    [Header("播放设置")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private DirectorWrapMode wrapMode = DirectorWrapMode.Hold;
    
    // 事件
    public event System.Action OnTimelinePlayed;
    public event System.Action OnTimelinePaused;
    public event System.Action OnTimelineStopped;
    public event System.Action<float> OnTimelineTimeChanged;
    
    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
    }
    
    void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }
    
    void OnEnable()
    {
        if (director != null)
        {
            director.played += HandlePlayed;
            director.paused += HandlePaused;
            director.stopped += HandleStopped;
        }
    }
    
    void OnDisable()
    {
        if (director != null)
        {
            director.played -= HandlePlayed;
            director.paused -= HandlePaused;
            director.stopped -= HandleStopped;
        }
    }
    
    #region 播放控制
    
    /// <summary>
    /// 开始播放Timeline
    /// </summary>
    public void Play()
    {
        if (director != null && timelineAsset != null)
        {
            director.playableAsset = timelineAsset;
            director.wrapMode = wrapMode;
            director.Play();
        }
    }
    
    /// <summary>
    /// 暂停播放
    /// </summary>
    public void Pause()
    {
        director?.Pause();
    }
    
    /// <summary>
    /// 停止播放并重置
    /// </summary>
    public void Stop()
    {
        director?.Stop();
    }
    
    /// <summary>
    /// 从指定时间开始播放
    /// </summary>
    /// <param name="startTime">起始时间（秒）</param>
    public void PlayFromTime(float startTime)
    {
        if (director != null)
        {
            director.time = startTime;
            director.Evaluate();
            director.Play();
        }
    }
    
    /// <summary>
    /// 播放指定时间范围
    /// </summary>
    /// <param name="startTime">起始时间</param>
    /// <param name="duration">持续时间</param>
    public void PlayTimeRange(float startTime, float duration)
    {
        if (director != null)
        {
            director.time = startTime;
            director.Evaluate();
            
            // 使用协程控制播放范围
            StartCoroutine(PlayTimeRangeCoroutine(startTime, duration));
        }
    }
    
    private System.Collections.IEnumerator PlayTimeRangeCoroutine(float startTime, float duration)
    {
        director.Play();
        
        while (director.time < startTime + duration)
        {
            yield return null;
        }
        
        director.Stop();
    }
    
    #endregion
    
    #region 时间控制
    
    /// <summary>
    /// 跳转到指定时间点
    /// </summary>
    /// <param name="targetTime">目标时间（秒）</param>
    public void JumpToTime(float targetTime)
    {
        if (director != null)
        {
            director.time = targetTime;
            director.Evaluate();
            OnTimelineTimeChanged?.Invoke(targetTime);
        }
    }
    
    /// <summary>
    /// 获取当前播放时间
    /// </summary>
    public float GetCurrentTime()
    {
        return director != null ? (float)director.time : 0f;
    }
    
    /// <summary>
    /// 获取Timeline总时长
    /// </summary>
    public float GetDuration()
    {
        return director != null ? (float)director.duration : 0f;
    }
    
    /// <summary>
    /// 获取播放进度（0-1）
    /// </summary>
    public float GetProgress()
    {
        float duration = GetDuration();
        if (duration <= 0) return 0f;
        
        return GetCurrentTime() / duration;
    }
    
    /// <summary>
    /// 设置播放进度（0-1）
    /// </summary>
    /// <param name="progress">进度值</param>
    public void SetProgress(float progress)
    {
        float duration = GetDuration();
        JumpToTime(duration * progress);
    }
    
    #endregion
    
    #region 速度控制
    
    /// <summary>
    /// 设置播放速度
    /// </summary>
    /// <param name="speed">速度值（1.0=正常，2.0=两倍速，0.5=半速）</param>
    public void SetPlaybackSpeed(float speed)
    {
        if (director != null && director.playableGraph.IsValid())
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
    }
    
    /// <summary>
    /// 倒放Timeline
    /// </summary>
    public void PlayBackwards()
    {
        if (director != null)
        {
            // 先跳转到结尾
            director.time = director.duration;
            director.Evaluate();
            
            // 设置负速度
            SetPlaybackSpeed(-1f);
            director.Play();
        }
    }
    
    #endregion
    
    #region 轨道绑定
    
    /// <summary>
    /// 动态绑定轨道
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    /// <param name="bindingObject">绑定对象</param>
    public void SetTrackBinding(string trackName, Object bindingObject)
    {
        if (director == null || timelineAsset == null) return;
        
        var tracks = timelineAsset.GetOutputTracks();
        foreach (var track in tracks)
        {
            if (track.name == trackName)
            {
                director.SetGenericBinding(track, bindingObject);
                break;
            }
        }
    }
    
    /// <summary>
    /// 获取轨道绑定对象
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    public Object GetTrackBinding(string trackName)
    {
        if (director == null || timelineAsset == null) return null;
        
        var tracks = timelineAsset.GetOutputTracks();
        foreach (var track in tracks)
        {
            if (track.name == trackName)
            {
                return director.GetGenericBinding(track);
            }
        }
        
        return null;
    }
    
    #endregion
    
    #region 事件处理
    
    private void HandlePlayed(PlayableDirector director)
    {
        OnTimelinePlayed?.Invoke();
    }
    
    private void HandlePaused(PlayableDirector director)
    {
        OnTimelinePaused?.Invoke();
    }
    
    private void HandleStopped(PlayableDirector director)
    {
        OnTimelineStopped?.Invoke();
    }
    
    #endregion
}
