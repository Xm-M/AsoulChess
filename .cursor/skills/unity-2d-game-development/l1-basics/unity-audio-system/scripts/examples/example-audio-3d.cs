using UnityEngine;

/// <summary>
/// 示例：3D音效配置
/// 演示如何创建和配置3D空间音效
/// 
/// 性能要点：
/// - ✅ 使用spatialBlend控制2D/3D混合
/// - ✅ 合理设置minDistance和maxDistance
/// - ✅ 使用AudioRolloffMode优化距离衰减
/// </summary>
public class ExampleAudio3D : MonoBehaviour
{
    [Header("AudioSource")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("3D音频设置")]
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 50f;
    
    [Header("多普勒效果")]
    [SerializeField] private float dopplerLevel = 1f;
    
    [Header("扩散角度")]
    [Range(0f, 360f)]
    [SerializeField] private float spread = 0f;
    
    [Header("优先级")]
    [Range(0, 256)]
    [SerializeField] private int priority = 128;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        Apply3DSettings();
    }

    /// <summary>
    /// 应用3D音频设置
    /// </summary>
    public void Apply3DSettings()
    {
        if (audioSource == null) return;
        
        // 启用3D音效
        audioSource.spatialBlend = 1f;
        
        // 设置距离衰减
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;
        
        // 设置多普勒效果
        audioSource.dopplerLevel = dopplerLevel;
        
        // 设置扩散角度
        audioSource.spread = spread;
        
        // 设置优先级
        audioSource.priority = priority;
        
        Debug.Log("3D音频设置已应用");
    }

    // ========== 距离衰减模式 ==========

    /// <summary>
    /// 设置对数衰减（推荐，符合真实听觉）
    /// </summary>
    public void SetLogarithmicRolloff()
    {
        if (audioSource != null)
        {
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            Debug.Log("对数衰减模式");
        }
    }

    /// <summary>
    /// 设置线性衰减
    /// </summary>
    public void SetLinearRolloff()
    {
        if (audioSource != null)
        {
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            Debug.Log("线性衰减模式");
        }
    }

    /// <summary>
    /// 设置自定义衰减曲线
    /// </summary>
    public void SetCustomRolloff(AnimationCurve curve)
    {
        if (audioSource != null)
        {
            audioSource.rolloffMode = AudioRolloffMode.Custom;
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            Debug.Log("自定义衰减模式");
        }
    }

    // ========== 2D/3D混合 ==========

    /// <summary>
    /// 设置2D/3D混合（0=纯2D，1=纯3D）
    /// </summary>
    public void SetSpatialBlend(float blend)
    {
        if (audioSource != null)
        {
            audioSource.spatialBlend = Mathf.Clamp01(blend);
            Debug.Log($"SpatialBlend: {audioSource.spatialBlend}");
        }
    }

    /// <summary>
    /// 设置纯2D音效
    /// </summary>
    public void Set2DAudio()
    {
        SetSpatialBlend(0f);
    }

    /// <summary>
    /// 设置纯3D音效
    /// </summary>
    public void Set3DAudio()
    {
        SetSpatialBlend(1f);
    }

    /// <summary>
    /// 设置混合模式
    /// </summary>
    public void SetMixedAudio()
    {
        SetSpatialBlend(0.5f);
    }

    // ========== 音频效果示例 ==========

    /// <summary>
    /// 播放脚步声（3D）
    /// </summary>
    public void PlayFootstepSound(AudioClip footstepClip, float volume = 1f)
    {
        if (audioSource == null || footstepClip == null) return;
        
        audioSource.PlayOneShot(footstepClip, volume);
    }

    /// <summary>
    /// 播放环境音（3D，循环）
    /// </summary>
    public void PlayAmbientSound(AudioClip ambientClip, float volume = 0.5f)
    {
        if (audioSource == null || ambientClip == null) return;
        
        audioSource.clip = ambientClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 20f;
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// 播放爆炸声（3D，大范围）
    /// </summary>
    public void PlayExplosionSound(AudioClip explosionClip, float volume = 1f)
    {
        if (audioSource == null || explosionClip == null) return;
        
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 100f;
        audioSource.spatialBlend = 1f;
        
        audioSource.PlayOneShot(explosionClip, volume);
    }

    /// <summary>
    /// 播放语音（3D，小范围）
    /// </summary>
    public void PlayVoiceSound(AudioClip voiceClip, float volume = 1f)
    {
        if (audioSource == null || voiceClip == null) return;
        
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 0f; // 语音通常不需要多普勒效果
        
        audioSource.PlayOneShot(voiceClip, volume);
    }

    // ========== 距离检测 ==========

    /// <summary>
    /// 获取到AudioListener的距离
    /// </summary>
    public float GetDistanceToListener()
    {
        if (audioSource == null) return float.MaxValue;
        
        // AudioListener通常在主相机上
        var listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            return Vector3.Distance(transform.position, listener.transform.position);
        }
        
        return float.MaxValue;
    }

    /// <summary>
    /// 获取当前音量（基于距离）
    /// </summary>
    public float GetAttenuatedVolume()
    {
        if (audioSource == null) return 0f;
        
        float distance = GetDistanceToListener();
        
        // 简单的线性衰减计算
        if (distance <= minDistance)
        {
            return audioSource.volume;
        }
        else if (distance >= maxDistance)
        {
            return 0f;
        }
        else
        {
            float t = (distance - minDistance) / (maxDistance - minDistance);
            return audioSource.volume * (1f - t);
        }
    }

    // ========== 调试方法 ==========

    /// <summary>
    /// 打印3D音频信息
    /// </summary>
    [ContextMenu("Print 3D Audio Info")]
    public void Print3DAudioInfo()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource未设置");
            return;
        }
        
        Debug.Log("=== 3D音频信息 ===");
        Debug.Log($"SpatialBlend: {audioSource.spatialBlend} (0=2D, 1=3D)");
        Debug.Log($"MinDistance: {audioSource.minDistance}m");
        Debug.Log($"MaxDistance: {audioSource.maxDistance}m");
        Debug.Log($"RolloffMode: {audioSource.rolloffMode}");
        Debug.Log($"DopplerLevel: {audioSource.dopplerLevel}");
        Debug.Log($"Spread: {audioSource.spread}°");
        Debug.Log($"Priority: {audioSource.priority}");
        Debug.Log($"距离Listener: {GetDistanceToListener()}m");
        Debug.Log($"衰减后音量: {GetAttenuatedVolume():F2}");
    }

    /// <summary>
    /// 测试音量衰减（移动测试）
    /// </summary>
    [ContextMenu("Test Volume Attenuation")]
    public void TestVolumeAttenuation()
    {
        StartCoroutine(TestAttenuationCoroutine());
    }

    private System.Collections.IEnumerator TestAttenuationCoroutine()
    {
        float startX = transform.position.z - maxDistance;
        float endX = transform.position.z + maxDistance;
        float step = 2f;
        
        var listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            Debug.LogWarning("找不到AudioListener");
            yield break;
        }
        
        Debug.Log("开始音量衰减测试...");
        
        for (float x = startX; x <= endX; x += step)
        {
            listener.transform.position = new Vector3(0, 0, x);
            float distance = GetDistanceToListener();
            float volume = GetAttenuatedVolume();
            
            Debug.Log($"距离: {distance:F1}m, 音量: {volume:F2}");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("测试完成");
    }

    // ========== 视觉调试 ==========

    void OnDrawGizmosSelected()
    {
        if (audioSource == null) return;
        
        // 绘制最小距离（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, audioSource.minDistance);
        
        // 绘制最大距离（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, audioSource.maxDistance);
        
        // 绘制到AudioListener的连线
        var listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, listener.transform.position);
            
            float distance = Vector3.Distance(transform.position, listener.transform.position);
            float volume = GetAttenuatedVolume();
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"距离: {distance:F1}m\n音量: {volume:F2}");
            #endif
        }
    }
}

/// <summary>
/// 示例：移动音源，测试3D音效
/// </summary>
public class ExampleMovingAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ambientSound;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float movementRange = 20f;
    
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        
        // 播放环境音
        if (audioSource != null && ambientSound != null)
        {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 15f;
            audioSource.Play();
        }
    }

    void Update()
    {
        // 来回移动
        float offset = Mathf.Sin(Time.time * moveSpeed) * movementRange;
        transform.position = startPos + Vector3.right * offset;
    }
}
