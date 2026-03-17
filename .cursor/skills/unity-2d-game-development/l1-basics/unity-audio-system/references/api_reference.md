# Unity Audio System API 完整参考文档

## 概述

Unity Audio系统是游戏音频播放的核心系统，包括AudioSource、AudioClip、AudioListener等组件。本文档提供完整的API参考，包括性能优化建议和白名单分类。

---

## 核心组件

### 1. AudioSource（音频源）

AudioSource是播放音频的核心组件。

#### ✅ 推荐使用的API

**基础播放方法：**

```csharp
// ✅ 推荐：播放音频
AudioSource audioSource = GetComponent<AudioSource>();

// 播放已分配的AudioClip
audioSource.Play();

// 播放一次性音效（不受AudioSource状态影响）
audioSource.PlayOneShot(audioClip);

// 播放指定延迟
audioSource.PlayDelayed(0.5f); // 延迟0.5秒播放

// 在指定时间播放
audioSource.PlayScheduled(AudioSettings.dspTime + 0.5);
```

**停止和暂停：**

```csharp
// ✅ 推荐：停止播放
audioSource.Stop();

// ✅ 推荐：暂停播放
audioSource.Pause();

// ✅ 推荐：取消暂停
audioSource.UnPause();
```

**音量控制：**

```csharp
// ✅ 推荐：设置音量（0-1线性值）
audioSource.volume = 0.5f;

// ✅ 推荐：线性音量转分贝
float linearVolume = 0.5f;
float dB = 20f * Mathf.Log10(linearVolume);
Debug.Log($"音量 {linearVolume} = {dB} dB");

// ✅ 推荐：分贝转线性音量
float dBValue = -6f;
float linear = Mathf.Pow(10f, dBValue / 20f);
audioSource.volume = linear;
```

**音量转换公式：**
```
dB = 20 × log₁₀(linear)
linear = 10^(dB/20)
```

**常用音量对照表：**

| 线性音量 | 分贝值(dB) | 说明 |
|---------|-----------|------|
| 1.0 | 0 dB | 最大音量 |
| 0.5 | -6 dB | 半音量 |
| 0.25 | -12 dB | 1/4音量 |
| 0.1 | -20 dB | 1/10音量 |
| 0.01 | -40 dB | 1/100音量 |
| 0.0 | -∞ dB | 静音 |

**循环和速度：**

```csharp
// ✅ 推荐：循环播放
audioSource.loop = true;

// ✅ 推荐：播放速度（1.0为正常速度）
audioSource.pitch = 1.0f;  // 正常
audioSource.pitch = 0.5f;  // 半速
audioSource.pitch = 2.0f;  // 两倍速
```

**播放状态：**

```csharp
// ✅ 推荐：检查播放状态
if (audioSource.isPlaying) {
    Debug.Log("正在播放");
}

// ✅ 推荐：获取当前播放时间
float currentTime = audioSource.time;
float totalTime = audioSource.clip.length;
float progress = currentTime / totalTime;

// ✅ 推荐：设置播放时间（跳转）
audioSource.time = 10f; // 跳转到第10秒
```

**空间混合（2D/3D音效）：**

```csharp
// ✅ 推荐：2D音效（不受距离影响）
audioSource.spatialBlend = 0f;

// ✅ 推荐：3D音效（受距离影响）
audioSource.spatialBlend = 1f;

// ✅ 推荐：混合（2D和3D混合）
audioSource.spatialBlend = 0.5f;
```

#### ⚠️ 性能警告的API

**PlayOneShot注意事项：**

```csharp
// ⚠️ 警告：PlayOneShot不受Stop影响
audioSource.PlayOneShot(clip);
audioSource.Stop(); // ❌ 不会停止PlayOneShot的声音

// ⚠️ 警告：同时播放大量PlayOneShot会影响性能
for (int i = 0; i < 100; i++) {
    audioSource.PlayOneShot(clip); // ❌ 性能问题
}

// ✅ 推荐：限制同时播放的音效数量
// 使用音频池管理AudioSource数量
```

#### ❌ 禁止使用的API

```csharp
// ❌ 禁止：每帧实例化AudioSource
void Update() {
    GameObject go = new GameObject("TempAudio");
    AudioSource temp = go.AddComponent<AudioSource>();
    temp.PlayOneShot(clip);
    Destroy(go, clip.length); // 性能灾难
}

// ✅ 正确：使用音频池
AudioPool.Instance.PlaySound(clip);
```

---

### 2. AudioClip（音频片段）

AudioClip是音频数据容器。

#### ✅ 推荐使用的属性

```csharp
AudioClip clip = audioSource.clip;

// ✅ 推荐：获取音频信息
string name = clip.name;
int lengthSamples = clip.samples;      // 总采样数
int frequency = clip.frequency;        // 采样率（Hz）
int channels = clip.channels;          // 声道数（1=单声道，2=立体声）
float length = clip.length;            // 长度（秒）

Debug.Log($"音频: {name}, 长度: {length}秒, 采样率: {frequency}Hz");
```

#### ⚠️ 性能警告

**音频加载设置：**

```csharp
// ⚠️ 警告：Decompress on Load（默认）- 加载时解压，占用内存大
// 适用于：短音效（< 1秒）

// ✅ 推荐：Compressed in Memory - 压缩存储，内存占用小
// 适用于：中等长度音效（1-10秒）

// ✅ 推荐：Streaming - 流式加载，内存占用最小
// 适用于：长音频（BGM、语音）
```

**音频格式建议：**

| 音频类型 | 推荐格式 | 加载方式 | 说明 |
|---------|---------|---------|------|
| 短音效 | WAV/MP3 | Decompress on Load | 快速播放 |
| 中等音效 | OGG Vorbis | Compressed in Memory | 平衡性能 |
| BGM | OGG Vorbis | Streaming | 内存优化 |
| 语音 | OGG Vorbis | Streaming | 内存优化 |

---

### 3. AudioListener（音频监听器）

AudioListener接收场景中的音频，通常绑定在主相机上。

#### ✅ 推荐使用的API

```csharp
// ✅ 推荐：全局音量控制
AudioListener.volume = 0.5f; // 0-1范围

// ✅ 推荐：暂停所有音频
AudioListener.pause = true;

// ✅ 推荐：获取音频设置
int outputSampleRate = AudioSettings.outputSampleRate;
int speakerMode = AudioSettings.speakerMode;
```

**重要：** 场景中只能有一个AudioListener！

---

### 4. 3D音频设置

#### ✅ 推荐使用的API

```csharp
AudioSource audioSource = GetComponent<AudioSource>();

// ✅ 推荐：启用3D音效
audioSource.spatialBlend = 1f;

// ✅ 推荐：设置距离衰减
audioSource.minDistance = 1f;   // 最小距离（音量最大）
audioSource.maxDistance = 50f;  // 最大距离（音量为0）

// ✅ 推荐：音量衰减曲线
audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // 对数衰减（推荐）
audioSource.rolloffMode = AudioRolloffMode.Linear;      // 线性衰减
audioSource.rolloffMode = AudioRolloffMode.Custom;      // 自定义曲线

// ✅ 推荐：多普勒效果
audioSource.dopplerLevel = 1f; // 0=禁用，1=正常

// ✅ 推荐：扩散角度
audioSource.spread = 0f; // 0-360度

// ✅ 推荐：优先级
audioSource.priority = 128; // 0=最高，256=最低
```

**距离衰减曲线说明：**

```csharp
// 对数衰减（推荐，符合真实听觉）
audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
// 音量 = 1 / (1 + rolloffFactor * (distance - minDistance))

// 线性衰减
audioSource.rolloffMode = AudioRolloffMode.Linear;
// 音量 = 1 - (distance - minDistance) / (maxDistance - minDistance)

// 自定义曲线（完全控制）
audioSource.rolloffMode = AudioRolloffMode.Custom;
audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animationCurve);
```

---

### 5. 音频混合器（AudioMixer）

#### ✅ 推荐使用的API

```csharp
using UnityEngine.Audio;

[SerializeField] private AudioMixer audioMixer;

// ✅ 推荐：设置混音器参数
audioMixer.SetFloat("MasterVolume", 0f); // 0 dB（最大音量）
audioMixer.SetFloat("MusicVolume", -10f); // -10 dB
audioMixer.SetFloat("SFXVolume", -5f); // -5 dB

// ✅ 推荐：获取混音器参数
float volume;
audioMixer.GetFloat("MasterVolume", out volume);

// ✅ 推荐：快照切换（平滑过渡）
AudioMixerSnapshot snapshot = audioMixer.FindSnapshot("Paused");
snapshot.TransitionTo(1f); // 1秒过渡到Paused快照
```

**音量转换（混音器使用分贝）：**

```csharp
// ✅ 推荐：线性音量转分贝（混音器专用）
float LinearToDecibel(float linear)
{
    if (linear <= 0f)
        return -80f; // 最小分贝值
    
    return 20f * Mathf.Log10(linear);
}

// ✅ 推荐：分贝转线性音量
float DecibelToLinear(float dB)
{
    return Mathf.Pow(10f, dB / 20f);
}

// 使用示例
float linearVolume = 0.5f;
float dB = LinearToDecibel(linearVolume); // ≈ -6 dB
audioMixer.SetFloat("MasterVolume", dB);
```

---

### 6. 音频效果（Audio Effects）

#### ✅ 推荐使用的效果

```csharp
// ✅ 推荐：混响（Reverb）
AudioReverbFilter reverb = gameObject.AddComponent<AudioReverbFilter>();
reverb.reverbLevel = 0f;      // 混响电平（dB）
reverb.reverbDelay = 0.04f;   // 混响延迟
reverb.room = -1000f;         // 房间大小

// ✅ 推荐：低通滤波器（Low Pass Filter）
AudioLowPassFilter lowPass = gameObject.AddComponent<AudioLowPassFilter>();
lowPass.cutoffFrequency = 5000f; // 截止频率（Hz）
lowPass.resonanceQ = 1f;         // 共振

// ✅ 推荐：高通滤波器（High Pass Filter）
AudioHighPassFilter highPass = gameObject.AddComponent<AudioHighPassFilter>();
highPass.cutoffFrequency = 500f; // 截止频率（Hz）

// ✅ 推荐：回声（Echo）
AudioEchoFilter echo = gameObject.AddComponent<AudioEchoFilter>();
echo.delay = 500f;     // 延迟（ms）
echo.decayRatio = 0.5f; // 衰减比例
echo.dryMix = 1f;      // 原声混合
echo.wetMix = 0.5f;    // 回声混合
```

---

## 性能优化清单

### ✅ 推荐做法

1. **使用音频池**
   ```csharp
   // 预创建AudioSource，避免运行时实例化
   // 限制同时播放AudioSource数量（建议< 32个）
   ```

2. **合理的音频加载方式**
   ```csharp
   // 短音效：Decompress on Load
   // 中等音效：Compressed in Memory
   // 长音频：Streaming
   ```

3. **使用AudioMixer**
   ```csharp
   // 分组管理音频（Master、Music、SFX）
   // 使用快照实现音频状态切换
   ```

4. **合理的音频格式**
   ```csharp
   // 短音效：WAV/MP3
   // BGM/语音：OGG Vorbis
   ```

5. **限制并发音效**
   ```csharp
   // 同时播放AudioSource < 32个
   // 使用优先级管理重要音效
   ```

### ❌ 禁止做法

1. **禁止运行时实例化AudioSource**
   ```csharp
   // ❌ 禁止
   AudioSource temp = gameObject.AddComponent<AudioSource>();
   temp.PlayOneShot(clip);
   Destroy(temp, clip.length);
   ```

2. **禁止同时播放大量AudioSource**
   ```csharp
   // ❌ 禁止（> 32个同时播放）
   ```

3. **禁止加载未压缩的大型音频**
   ```csharp
   // ❌ 禁止：3分钟BGM使用Decompress on Load
   // ✅ 正确：使用Streaming加载方式
   ```

---

## 常见问题

### Q1: 音效播放不出来？
**A:** 检查以下几点：
1. AudioSource是否添加
2. AudioClip是否分配
3. AudioListener是否存在
4. AudioListener.volume是否为0
5. AudioSource.volume是否为0

### Q2: 音效有延迟？
**A:** 解决方案：
1. 使用WAV格式（无压缩解码延迟）
2. 减小音频文件大小
3. 使用PlayScheduled代替PlayDelayed

### Q3: 3D音效听不到？
**A:** 检查以下几点：
1. spatialBlend是否为1
2. AudioListener和AudioSource距离
3. minDistance和maxDistance设置
4. 音频导入设置是否为3D

### Q4: 音频性能差？
**A:** 性能优化：
1. 使用音频池管理AudioSource
2. 合理设置加载方式（Streaming for BGM）
3. 限制并发音效数量（< 32）
4. 使用AudioMixer管理音量

---

## 相关资源

- [Unity Audio官方文档](https://docs.unity3d.com/Manual/AudioOverview.html)
- [AudioSource脚本API](https://docs.unity3d.com/ScriptReference/AudioSource.html)
- [AudioMixer教程](https://learn.unity.com/tutorial/introduction-to-audio-mixing)

---

## 版本说明

- 文档版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
