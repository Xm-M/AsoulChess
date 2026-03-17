---
name: unity-audio-system
description: Unity音频系统，包括AudioSource、AudioClip、AudioMixer等组件。提供音频池实现（避免运行时实例化）、音量转换公式（dB=20×log₁₀(linear)）、3D音效配置、性能优化建议。限制并发AudioSource数量<32。
---

# Unity Audio System Skill

## 技能描述
Unity Audio系统是游戏音频播放的核心系统，包括AudioSource、AudioClip、AudioListener等组件。本技能提供音频开发最佳实践、性能优化建议和API白名单。

---

## API白名单

### ✅ 推荐使用的API

**AudioSource核心方法：**
- `Play()` - 播放音频
- `PlayOneShot()` - 播放一次性音效
- `Stop()` - 停止播放
- `Pause()` - 暂停播放

**音量控制：**
```csharp
// ✅ 推荐：线性音量转分贝
float linearVolume = 0.5f;
float dB = 20f * Mathf.Log10(linearVolume);
audioSource.volume = linearVolume;
```

### ⚠️ 性能警告的API

**避免频繁创建AudioSource：**
- 使用音频池管理AudioSource
- 预加载常用音频资源

### ❌ 禁止使用的API

**性能陷阱：**
- ❌ 每帧实例化AudioSource
- ❌ 同时播放大量AudioSource（建议< 32个）
- ❌ 加载未压缩的大型音频文件

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ AudioSource核心API（Play、Stop、Pause、PlayOneShot）
- ✅ AudioClip音频资源管理
- ✅ AudioMixer混音器系统
- ✅ AudioListener音频监听器
- ✅ 音量控制与转换（线性↔分贝）
- ✅ 3D音效配置
- ✅ 音频池实现
- ✅ BGM/SFX分层管理

### 不在本Skill范围内

- ❌ 音频DSP效果器 → 不涉及
- ❌ 音频可视化 → 不涉及
- ❌ 语音聊天系统 → 本项目不涉及
- ❌ 音频中间件（FMOD、Wwise）→ 使用Unity原生系统
- ❌ 自定义音频解码器 → 不涉及

### 跨Skill功能依赖

**完整音频系统需要**：
- unity-audio-system（音频系统）← 当前Skill
- unity-singleton-manager（AudioManager）
- unity-scriptableobject-config（音频数据配置）

**音频与游戏系统集成**：
- unity-audio-system（音频播放）← 当前Skill
- unity-audio-system（AudioMixer）
- 游戏事件系统

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 同时播放AudioSource | ≤ 32 | 避免CPU过载 |
| AudioClip文件大小 | ≤ 5MB（SFX） | 长音频使用压缩格式 |
| AudioMixer暴露参数 | ≤ 20 | 控制混音复杂度 |
| 音频池大小 | ≤ 50 | 平衡内存与性能 |

---

## 音量转换公式

**线性 → 分贝：**
```
dB = 20 × log₁₀(linear)
```

**分贝 → 线性：**
```
linear = 10^(dB/20)
```

**示例：**
```csharp
// 线性音量0.5对应的分贝值
float linearVolume = 0.5f;
float dB = 20f * Mathf.Log10(linearVolume); // ≈ -6.02 dB

// 分贝值转线性音量
float dBValue = -6.02f;
float linear = Mathf.Pow(10f, dBValue / 20f); // ≈ 0.5
```

---

## 音频池实现

```csharp
using UnityEngine;
using System.Collections.Generic;

public class AudioPool : MonoBehaviour
{
    [SerializeField] private int poolSize = 10;
    private Queue<AudioSource> pool = new Queue<AudioSource>();
    
    void Start()
    {
        // 预创建AudioSource
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("AudioSource_Pooled");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            pool.Enqueue(source);
        }
    }
    
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (pool.Count > 0)
        {
            AudioSource source = pool.Dequeue();
            source.PlayOneShot(clip, volume);
            StartCoroutine(ReturnToPool(source, clip.length));
        }
    }
    
    System.Collections.IEnumerator ReturnToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        pool.Enqueue(source);
    }
}
```

---

## 版本说明
- 技能版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
