using UnityEngine;

/// <summary>
/// 示例：基础音频播放
/// 演示如何使用AudioSource播放音频
/// 
/// 性能要点：
/// - ✅ 使用Play播放已分配的clip
/// - ✅ 使用PlayOneShot播放一次性音效
/// - ❌ 避免每帧实例化AudioSource
/// </summary>
public class ExampleAudioBasic : MonoBehaviour
{
    [Header("AudioSource引用")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("音频资源")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip hurtSound;

    void Start()
    {
        // 开始播放背景音乐
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true; // 循环播放
            musicSource.Play();
        }
    }

    // ========== 音乐控制 ==========

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.Unpause();
        }
    }

    /// <summary>
    /// 跳转到指定时间
    /// </summary>
    public void SeekMusic(float timeInSeconds)
    {
        if (musicSource != null)
        {
            musicSource.time = timeInSeconds;
        }
    }

    /// <summary>
    /// 获取音乐播放进度（0-1）
    /// </summary>
    public float GetMusicProgress()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            return musicSource.time / musicSource.clip.length;
        }
        return 0f;
    }

    // ========== 音效播放 ==========

    /// <summary>
    /// 播放跳跃音效
    /// </summary>
    public void PlayJumpSound()
    {
        if (sfxSource != null && jumpSound != null)
        {
            // ✅ 推荐：使用PlayOneShot播放一次性音效
            sfxSource.PlayOneShot(jumpSound);
        }
    }

    /// <summary>
    /// 播放金币音效
    /// </summary>
    public void PlayCoinSound()
    {
        if (sfxSource != null && coinSound != null)
        {
            sfxSource.PlayOneShot(coinSound);
        }
    }

    /// <summary>
    /// 播放受伤音效
    /// </summary>
    public void PlayHurtSound()
    {
        if (sfxSource != null && hurtSound != null)
        {
            sfxSource.PlayOneShot(hurtSound);
        }
    }

    /// <summary>
    /// 播放指定音效
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // ========== 音量控制 ==========

    /// <summary>
    /// 设置音乐音量（线性0-1）
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// 设置音效音量（线性0-1）
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// 线性音量转分贝
    /// </summary>
    public float LinearToDecibel(float linear)
    {
        if (linear <= 0f)
            return -80f; // 最小分贝值
        return 20f * Mathf.Log10(linear);
    }

    /// <summary>
    /// 分贝转线性音量
    /// </summary>
    public float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }

    // ========== 音频状态查询 ==========

    /// <summary>
    /// 检查音乐是否在播放
    /// </summary>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    /// <summary>
    /// 获取当前音乐播放时间（秒）
    /// </summary>
    public float GetMusicTime()
    {
        return musicSource != null ? musicSource.time : 0f;
    }

    /// <summary>
    /// 获取音乐总时长（秒）
    /// </summary>
    public float GetMusicLength()
    {
        return musicSource != null && musicSource.clip != null ? musicSource.clip.length : 0f;
    }

    // ========== 播放速度控制 ==========

    /// <summary>
    /// 设置音乐播放速度
    /// </summary>
    public void SetMusicPitch(float pitch)
    {
        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        }
    }

    /// <summary>
    /// 加速音乐
    /// </summary>
    public void SpeedUpMusic(float amount = 0.1f)
    {
        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Clamp(musicSource.pitch + amount, 0.1f, 3f);
        }
    }

    /// <summary>
    /// 减速音乐
    /// </summary>
    public void SlowDownMusic(float amount = 0.1f)
    {
        if (musicSource != null)
        {
            musicSource.pitch = Mathf.Clamp(musicSource.pitch - amount, 0.1f, 3f);
        }
    }

    // ========== 调试方法 ==========

    /// <summary>
    /// 打印音频信息
    /// </summary>
    [ContextMenu("Print Audio Info")]
    public void PrintAudioInfo()
    {
        if (musicSource != null)
        {
            Debug.Log("=== 音乐信息 ===");
            Debug.Log($"Clip: {musicSource.clip?.name}");
            Debug.Log($"Is Playing: {musicSource.isPlaying}");
            Debug.Log($"Volume: {musicSource.volume}");
            Debug.Log($"Pitch: {musicSource.pitch}");
            Debug.Log($"Time: {musicSource.time}/{musicSource.clip?.length}");
            Debug.Log($"Loop: {musicSource.loop}");
        }

        if (sfxSource != null)
        {
            Debug.Log("\n=== 音效信息 ===");
            Debug.Log($"Volume: {sfxSource.volume}");
            Debug.Log($"Priority: {sfxSource.priority}");
        }
    }

    /// <summary>
    /// 测试所有音效
    /// </summary>
    [ContextMenu("Test All Sounds")]
    public void TestAllSounds()
    {
        StartCoroutine(TestSoundsCoroutine());
    }

    private System.Collections.IEnumerator TestSoundsCoroutine()
    {
        Debug.Log("测试开始...");
        
        yield return new WaitForSeconds(1f);
        PlayJumpSound();
        yield return new WaitForSeconds(0.5f);
        
        PlayCoinSound();
        yield return new WaitForSeconds(0.5f);
        
        PlayHurtSound();
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("测试完成");
    }
}
