using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;
public class AudioPlayer : MonoBehaviour
{
    public AudioType autype;
    public AudioSource audioSource;
    public List<GameObject> subAudio;
    public float baseValue;
    private void Awake()
    {
        baseValue= audioSource.volume;
    }
    private void OnEnable()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if(autype==AudioType.SoundEffect)
            audioSource.volume=AudioManage.SoundEffectValue*baseValue;
        else
            audioSource.volume=AudioManage.BgmValue * baseValue;
        AudioManage.AddPlayer(this);
    }
    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(_uniquePlayingName))
        {
            StopUniqueInternal();
        }
        AudioManage.RemovePlayer(this);
    }
    [Serializable]
    public class NameAudioClip
    {
        /// <summary>不要用字段名 <c>name</c>：与 Unity 序列化 / Object.name 冲突，会导致 Inspector 事件等路径下表现异常。</summary>
        [FormerlySerializedAs("name")]
        public string audioKey;
        public AudioClip clip;
    }
    public List<NameAudioClip> clipList;

    [Tooltip("PlayAudioUniqueLimit：同一音效名全局最多允许多少个排队（含正在播的当前持有者）；≤0 视为不限制")]
    [SerializeField]
    int _audioUniqueLimit = 3;

    string _uniquePlayingName; // 当前持有的全局唯一 key，OnDisable/StopUnique 时释放
    int _uniqueSubIndex = -1;  // 子物体播放时为 subAudio 索引，-1 表示本物体
    bool _uniqueIsLoop;        // 是否为循环音效，交接时区分

    /// <param name="audioKey">须与 Clip List 中条目的 <see cref="NameAudioClip.audioKey"/> 一致；参数勿命名为 name，避免与 Unity Object.name 混淆。</param>
    public void PlayAudio(string audioKey)
    {
        if (clipList == null) return;
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i] != null && audioKey == clipList[i].audioKey)
            {
                audioSource.clip = clipList[i].clip;
                Play();
                return;
            }
        }
    }

    /// <summary>运行时设置 <see cref="PlayAudioUniqueLimit"/> 的全局排队上限；≤0 表示不限制（与 int.MaxValue 行为一致）。</summary>
    public void SetAudioUniqueLimit(int n)
    {
        _audioUniqueLimit = n;
    }

    /// <summary>
    /// 与 <see cref="PlayAudioUnique"/> 相同排队交接逻辑，但同一音效名全局最多只保留 <c>_audioUniqueLimit</c> 个排队者，
    /// 超出则本次调用直接忽略（不入队、不播放），避免短时间大量触发时排成长队一直播。
    /// </summary>
    public void PlayAudioUniqueLimit(string audioKey)
    {
        if (!HasClip(audioKey))
        {
            Debug.LogWarning($"[AudioPlayer] PlayAudioUniqueLimit: clipList 中无 audioKey \"{audioKey}\" 的项。请在 Inspector → Clip List 配置 audioKey 并指定 Clip。", this);
            return;
        }
        bool shouldPlay = AudioManage.AcquireUniqueLimited(audioKey, this, _audioUniqueLimit, out bool registered);
        if (!registered) return;
        _uniquePlayingName = audioKey;
        _uniqueIsLoop = false;
        if (shouldPlay)
        {
            PlayAudioNow(audioKey);
            StartCoroutine(ReleaseUniqueWhenDone(audioKey, GetClipLength(audioKey)));
        }
    }

    /// <summary>
    /// 全局唯一播放：同一音效名同一时刻只播放一次，num 引用计数，num==0 时才真正停止
    /// </summary>
    public void PlayAudioUnique(string audioKey)
    {
        if (!HasClip(audioKey)) return;
        bool shouldPlay = AudioManage.AcquireUnique(audioKey, this);
        _uniquePlayingName = audioKey;
        _uniqueIsLoop = false;
        if (shouldPlay)
        {
            PlayAudioNow(audioKey);
            StartCoroutine(ReleaseUniqueWhenDone(audioKey, GetClipLength(audioKey)));
        }
    }

    /// <summary>
    /// 全局唯一随机播放：key 为唯一槽位（可用 GameObject 名、index 等），实际播放随机 clip
    /// </summary>
    public void PlayAudioUniqueRandom(string key)
    {
        if (clipList == null || clipList.Count == 0) return;
        bool shouldPlay = AudioManage.AcquireUnique(key, this);
        _uniquePlayingName = key;
        _uniqueIsLoop = false;
        if (shouldPlay)
        {
            int n = UnityEngine.Random.Range(0, clipList.Count);
            var clip = clipList[n].clip;
            audioSource.clip = clip;
            Play();
            StartCoroutine(ReleaseUniqueWhenDone(key, clip.length));
        }
    }

    /// <summary>
    /// 全局唯一随机循环播放：key 为唯一槽位，随机 clip 循环，停止时调用 StopUnique
    /// </summary>
    public void PlayAudioUniqueRandomLoop(string key)
    {
        if (clipList == null || clipList.Count == 0) return;
        bool shouldPlay = AudioManage.AcquireUnique(key, this);
        _uniquePlayingName = key;
        _uniqueSubIndex = -1;
        _uniqueIsLoop = true;
        if (shouldPlay)
        {
            int n = UnityEngine.Random.Range(0, clipList.Count);
            audioSource.clip = clipList[n].clip;
            SetLoop(true);
            Play();
        }
    }

    /// <summary>
    /// 全局唯一播放（循环音效）：停止时需调用 StopUnique 释放槽位
    /// </summary>
    public void PlayAudioUniqueLoop(string audioKey)
    {
        if (!HasClip(audioKey)) return;
        bool shouldPlay = AudioManage.AcquireUnique(audioKey, this);
        _uniquePlayingName = audioKey;
        _uniqueIsLoop = true;
        if (shouldPlay)
        {
            audioSource.clip = GetClip(audioKey);
            SetLoop(true);
            Play();
        }
    }

    /// <summary>
    /// 停止全局唯一播放并释放槽位；若 num>0 会交接给下一个持有者
    /// </summary>
    public void StopUnique()
    {
        if (string.IsNullOrEmpty(_uniquePlayingName)) return;
        StopUniqueInternal();
    }

    void StopUniqueInternal()
    {
        if (_uniqueSubIndex >= 0 && subAudio != null && _uniqueSubIndex < subAudio.Count)
            subAudio[_uniqueSubIndex].GetComponent<AudioPlayer>()?.Stop();
        else
            Stop();
        var next = AudioManage.ReleaseUnique(_uniquePlayingName, this);
        string key = _uniquePlayingName;
        int subIdx = _uniqueSubIndex;
        bool isLoop = _uniqueIsLoop;
        _uniquePlayingName = null;
        _uniqueSubIndex = -1;
        if (next != null)
        {
            if (subIdx >= 0)
                next.PlaySubHandoff(subIdx, key, isLoop);
            else
                next.PlayHandoff(key, isLoop);
        }
    }

    /// <summary>
    /// 直接播放（用于交接），不经过唯一逻辑
    /// </summary>
    public void PlayAudioNow(string audioKey)
    {
        var clip = GetClip(audioKey);
        if (clip == null) return;
        audioSource.clip = clip;
        Play();
    }

    /// <summary>
    /// 交接播放：key 为 clip 名时用该 clip，否则随机播放
    /// </summary>
    internal void PlayHandoff(string key, bool isLoop)
    {
        var clip = GetClip(key);
        if (clip != null)
        {
            audioSource.clip = clip;
            _uniquePlayingName = key;
            _uniqueIsLoop = isLoop;
            _uniqueSubIndex = -1;
            if (isLoop)
            {
                SetLoop(true);
                Play();
            }
            else
            {
                SetLoop(false);
                Play();
                StartCoroutine(ReleaseUniqueWhenDone(key, clip.length));
            }
        }
        else if (clipList != null && clipList.Count > 0)
        {
            // key 为自定义（如 GameObject 名），随机播放
            int n = UnityEngine.Random.Range(0, clipList.Count);
            clip = clipList[n].clip;
            audioSource.clip = clip;
            _uniquePlayingName = key;
            _uniqueIsLoop = isLoop;
            _uniqueSubIndex = -1;
            if (isLoop)
            {
                SetLoop(true);
                Play();
            }
            else
            {
                SetLoop(false);
                Play();
                StartCoroutine(ReleaseUniqueWhenDone(key, clip.length));
            }
        }
    }

    bool HasClip(string audioKey)
    {
        return GetClip(audioKey) != null;
    }

    AudioClip GetClip(string audioKey)
    {
        if (clipList == null || string.IsNullOrEmpty(audioKey)) return null;
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i] != null && audioKey == clipList[i].audioKey)
                return clipList[i].clip;
        }
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i]?.clip != null && clipList[i].clip.name == audioKey)
                return clipList[i].clip;
        }
        return null;
    }

    float GetClipLength(string audioKey)
    {
        var c = GetClip(audioKey);
        return c != null ? c.length : 0f;
    }

    IEnumerator ReleaseUniqueWhenDone(string key, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_uniquePlayingName == key)
        {
            Stop();
            var next = AudioManage.ReleaseUnique(key, this);
            _uniquePlayingName = null;
            _uniqueSubIndex = -1;
            if (next != null) next.PlayHandoff(key, _uniqueIsLoop);
        }
    }

    IEnumerator ReleaseSubUniqueWhenDone(string key, int subIdx, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_uniquePlayingName == key && _uniqueSubIndex == subIdx)
        {
            if (subAudio != null && subIdx < subAudio.Count)
                subAudio[subIdx].GetComponent<AudioPlayer>()?.Stop();
            var next = AudioManage.ReleaseUnique(key, this);
            _uniquePlayingName = null;
            _uniqueSubIndex = -1;
            if (next != null) next.PlaySubHandoff(subIdx, key, _uniqueIsLoop);
        }
    }

    /// <summary>
    /// 子物体交接播放
    /// </summary>
    internal void PlaySubHandoff(int subIdx, string key, bool isLoop)
    {
        if (subAudio == null || subIdx < 0 || subIdx >= subAudio.Count) return;
        var sub = subAudio[subIdx].GetComponent<AudioPlayer>();
        if (sub == null) return;
        _uniquePlayingName = key;
        _uniqueSubIndex = subIdx;
        _uniqueIsLoop = isLoop;
        sub.RandomPlay();
        if (isLoop)
            sub.SetLoop(true);
        else
        {
            float len = sub.audioSource.clip != null ? sub.audioSource.clip.length : 0f;
            StartCoroutine(ReleaseSubUniqueWhenDone(key, subIdx, len));
        }
    }
    public void RandomPlay()
    {
        int n=UnityEngine.Random.Range(0,clipList.Count);
        audioSource.clip = clipList[n].clip;
        Play();
    }
    public void ChangeAudio(string audioKey)
    {
        if (clipList == null) return;
        for (int i = 0; i < clipList.Count; i++)
        {
            if (clipList[i] != null && audioKey == clipList[i].audioKey)
            {
                audioSource.clip = clipList[i].clip;
                return;
            }
        }
    }
    
    [Button("播放")]
    public void Play()
    {
        if (audioSource == null)
        {
            //Debug.LogError($"{name} : audioSource is null");
            return;
        }

        if (audioSource.clip == null)
        {
            //Debug.LogError($"{name} : audioSource.clip is null");
            return;
        }

        // 关键：每次播放前重置播放位置
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();
    }
    public void PlayWithSub()
    {
        audioSource.Play();
        for(int i=0;i<subAudio.Count;i++)
            subAudio[i].GetComponent<AudioPlayer>().RandomPlay(); 
    }
    public void PlaySub()
    {
        for (int i = 0; i < subAudio.Count; i++)
            subAudio[i].GetComponent<AudioPlayer>().RandomPlay();
    }
    public void PlaySub(int n,string audioName)
    {
        subAudio[n].GetComponent<AudioPlayer>().PlayAudio(audioName);
    }
    public void PlaySubN(int n)
    {
        subAudio[n].GetComponent<AudioPlayer>().RandomPlay();
    }

    /// <summary>
    /// 子物体全局唯一随机播放：key 为空时用 subAudio[n].gameObject.name
    /// </summary>
    public void PlaySubUniqueRandom(int n, string key = null)
    {
        if (subAudio == null || n < 0 || n >= subAudio.Count) return;
        var sub = subAudio[n].GetComponent<AudioPlayer>();
        if (sub == null) return;
        string k = string.IsNullOrEmpty(key) ? subAudio[n].gameObject.name : key;
        bool shouldPlay = AudioManage.AcquireUnique(k, this);
        _uniquePlayingName = k;
        _uniqueSubIndex = n;
        _uniqueIsLoop = false;
        if (shouldPlay)
        {
            sub.RandomPlay();
            float len = sub.audioSource.clip != null ? sub.audioSource.clip.length : 0f;
            StartCoroutine(ReleaseSubUniqueWhenDone(k, n, len));
        }
    }

    /// <summary>
    /// 子物体全局唯一循环播放：key 为空时用 subAudio[n].gameObject.name，停止时调用 StopUnique
    /// </summary>
    public void PlaySubUniqueLoop(int n, string key = null)
    {
        if (subAudio == null || n < 0 || n >= subAudio.Count) return;
        var sub = subAudio[n].GetComponent<AudioPlayer>();
        if (sub == null) return;
        string k = string.IsNullOrEmpty(key) ? subAudio[n].gameObject.name : key;
        bool shouldPlay = AudioManage.AcquireUnique(k, this);
        _uniquePlayingName = k;
        _uniqueSubIndex = n;
        _uniqueIsLoop = true;
        if (shouldPlay)
        {
            sub.RandomPlay();
            sub.SetLoop(true);
        }
    }
    public void StopSubN(int n)
    {
        subAudio[n].GetComponent<AudioPlayer>().Stop();
    }
    public void Stop()
    {
        audioSource.Stop();
    }
    public void Pause()
    {
        audioSource.Pause();
    }
    public void UnPause()
    {
        //if (autype == AudioType.SoundEffect)
        //    audioSource.volume = AudioManage.SoundEffectValue;
        //else
        //    audioSource.volume = AudioManage.BgmValue;
        audioSource.UnPause();
    }
    public void SetLoop(bool loop)
    {
        audioSource.loop = loop;
    }
    /// <summary>设置播放进度（秒），循环时自动取模</summary>
    public void Seek(float time)
    {
        if (audioSource == null || audioSource.clip == null) return;
        if (audioSource.clip.length <= 0f) return;

        float t = time;

        if (audioSource.loop)
        {
            t = Mathf.Repeat(time, audioSource.clip.length);
        }
        else
        {
            t = Mathf.Clamp(time, 0f, audioSource.clip.length - 0.001f);
        }

        audioSource.time = t;
    }
}
