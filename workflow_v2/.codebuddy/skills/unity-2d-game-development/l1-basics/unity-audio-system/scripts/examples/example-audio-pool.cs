using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 示例：音频池管理
/// 演示如何使用对象池优化音频播放性能
/// 
/// 性能要点：
/// - ✅ 预创建AudioSource，避免运行时实例化
/// - ✅ 限制并发AudioSource数量
/// - ✅ 复用AudioSource对象
/// - ❌ 避免每帧Instantiate/Destroy
/// </summary>
public class ExampleAudioPool : MonoBehaviour
{
    public static ExampleAudioPool Instance { get; private set; }

    [Header("音频池配置")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 30;
    
    [Header("AudioSource设置")]
    [SerializeField] private GameObject audioSourcePrefab;
    
    private Queue<AudioSource> pool = new Queue<AudioSource>();
    private List<AudioSource> activeSources = new List<AudioSource>();
    private Transform poolContainer;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 创建容器
        poolContainer = new GameObject("AudioPool").transform;
        poolContainer.SetParent(transform);
        
        // 初始化池
        InitializePool();
    }

    /// <summary>
    /// 初始化音频池
    /// </summary>
    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            AudioSource source = CreateAudioSource();
            pool.Enqueue(source);
        }
        
        Debug.Log($"音频池初始化完成，数量: {pool.Count}");
    }

    /// <summary>
    /// 创建新的AudioSource
    /// </summary>
    AudioSource CreateAudioSource()
    {
        GameObject go;
        
        if (audioSourcePrefab != null)
        {
            go = Instantiate(audioSourcePrefab, poolContainer);
        }
        else
        {
            go = new GameObject("PooledAudioSource");
            go.transform.SetParent(poolContainer);
            go.AddComponent<AudioSource>();
        }
        
        AudioSource source = go.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.gameObject.SetActive(false);
        
        return source;
    }

    // ========== 公开API ==========

    /// <summary>
    /// 播放音效（使用音频池）
    /// </summary>
    public AudioSource PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip为null");
            return null;
        }

        // 从池中获取AudioSource
        AudioSource source = GetFromPool();
        
        if (source == null)
        {
            Debug.LogWarning("音频池已满，无法播放音效");
            return null;
        }

        // 配置AudioSource
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.gameObject.SetActive(true);
        
        // 添加到活跃列表
        activeSources.Add(source);
        
        // 播放
        source.Play();
        
        // 如果不是循环，自动返回池
        if (!loop)
        {
            StartCoroutine(ReturnToPoolAfterPlay(source, clip.length / pitch));
        }
        
        return source;
    }

    /// <summary>
    /// 播放3D音效
    /// </summary>
    public AudioSource Play3DSound(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 1f, float maxDistance = 50f)
    {
        AudioSource source = PlaySound(clip, volume);
        
        if (source != null)
        {
            source.transform.position = position;
            source.spatialBlend = 1f; // 纯3D
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
        }
        
        return source;
    }

    /// <summary>
    /// 停止特定音效
    /// </summary>
    public void StopSound(AudioSource source)
    {
        if (source != null && activeSources.Contains(source))
        {
            source.Stop();
            ReturnToPool(source);
        }
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        // 停止所有活跃的AudioSource
        foreach (var source in activeSources)
        {
            source.Stop();
            ReturnToPool(source);
        }
        
        activeSources.Clear();
    }

    /// <summary>
    /// 设置全局音量
    /// </summary>
    public void SetGlobalVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        
        // 设置所有活跃的AudioSource
        foreach (var source in activeSources)
        {
            source.volume = volume;
        }
    }

    // ========== 池管理 ==========

    /// <summary>
    /// 从池中获取AudioSource
    /// </summary>
    AudioSource GetFromPool()
    {
        // 如果池中有可用对象
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        
        // 如果池为空，检查是否可以创建新对象
        if (activeSources.Count + pool.Count < maxPoolSize)
        {
            return CreateAudioSource();
        }
        
        return null; // 池已满
    }

    /// <summary>
    /// 将AudioSource返回池中
    /// </summary>
    void ReturnToPool(AudioSource source)
    {
        if (source == null) return;
        
        // 停止播放
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        
        // 从活跃列表移除
        activeSources.Remove(source);
        
        // 返回池中
        pool.Enqueue(source);
    }

    /// <summary>
    /// 播放完成后返回池
    /// </summary>
    System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (source != null && activeSources.Contains(source))
        {
            ReturnToPool(source);
        }
    }

    // ========== 调试和统计 ==========

    /// <summary>
    /// 获取池统计信息
    /// </summary>
    [ContextMenu("Print Pool Stats")]
    public void PrintPoolStats()
    {
        Debug.Log("=== 音频池统计 ===");
        Debug.Log($"池中对象: {pool.Count}");
        Debug.Log($"活跃对象: {activeSources.Count}");
        Debug.Log($"总对象: {pool.Count + activeSources.Count}");
        Debug.Log($"最大容量: {maxPoolSize}");
        Debug.Log($"使用率: {(float)activeSources.Count / maxPoolSize * 100:F1}%");
    }

    /// <summary>
    /// 清空池
    /// </summary>
    [ContextMenu("Clear Pool")]
    public void ClearPool()
    {
        // 停止所有活跃的AudioSource
        StopAllSounds();
        
        // 销毁池中的对象
        foreach (var source in pool)
        {
            if (source != null)
            {
                Destroy(source.gameObject);
            }
        }
        
        pool.Clear();
        
        Debug.Log("音频池已清空");
    }

    /// <summary>
    /// 预热池（创建指定数量的AudioSource）
    /// </summary>
    public void WarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (pool.Count + activeSources.Count < maxPoolSize)
            {
                AudioSource source = CreateAudioSource();
                pool.Enqueue(source);
            }
            else
            {
                break;
            }
        }
        
        Debug.Log($"池预热完成，当前池大小: {pool.Count}");
    }

    // ========== 编辑器工具 ==========

    void OnDrawGizmosSelected()
    {
        // 绘制活跃AudioSource的位置（3D音效）
        if (activeSources != null)
        {
            Gizmos.color = Color.green;
            foreach (var source in activeSources)
            {
                if (source != null && source.spatialBlend > 0f)
                {
                    Gizmos.DrawWireSphere(source.transform.position, source.minDistance);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(source.transform.position, source.maxDistance);
                    Gizmos.color = Color.green;
                }
            }
        }
    }
}

/// <summary>
/// 示例：使用音频池
/// </summary>
public class ExampleUseAudioPool : MonoBehaviour
{
    [Header("音频资源")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip explosionSound;
    
    void Update()
    {
        // 按空格键播放跳跃音效
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExampleAudioPool.Instance.PlaySound(jumpSound);
        }
        
        // 按C键播放金币音效
        if (Input.GetKeyDown(KeyCode.C))
        {
            ExampleAudioPool.Instance.PlaySound(coinSound);
        }
        
        // 按E键播放爆炸3D音效
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 explosionPos = transform.position + Vector3.forward * 5f;
            ExampleAudioPool.Instance.Play3DSound(
                explosionSound,
                explosionPos,
                volume: 1f,
                minDistance: 1f,
                maxDistance: 30f
            );
        }
    }

    void OnDestroy()
    {
        // 清理
        if (ExampleAudioPool.Instance != null)
        {
            ExampleAudioPool.Instance.StopAllSounds();
        }
    }
}
