# Unity Audio System 基础教程

## 快速开始

### 1. 添加AudioSource组件

```
GameObject → Add Component → Audio Source
```

AudioSource是播放音频的核心组件，必须添加到GameObject上。

### 2. 准备音频文件

将音频文件导入Unity项目：
```
Project窗口右键 → Import New Asset → 选择音频文件
```

**支持的音频格式：**
- WAV - 无压缩，快速加载
- MP3 - 压缩，小文件
- OGG Vorbis - 压缩，平衡性能
- AAC - 压缩，移动端优化

### 3. 播放音频

```csharp
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    void Start()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
```

---

## 核心概念

### AudioSource（音频源）

AudioSource负责播放音频，类似于"播放器"。

**关键属性：**
- `clip` - 要播放的AudioClip
- `volume` - 音量（0-1）
- `pitch` - 播放速度（0.1-3）
- `loop` - 是否循环
- `spatialBlend` - 2D/3D音效混合（0=2D，1=3D）

### AudioClip（音频片段）

AudioClip是音频数据容器，类似于"音乐文件"。

**关键属性：**
- `length` - 长度（秒）
- `samples` - 采样数
- `frequency` - 采样率（Hz）
- `channels` - 声道数（1=单声道，2=立体声）

### AudioListener（音频监听器）

AudioListener接收音频，类似于"耳朵"。

**重要：** 场景中只能有一个AudioListener！

通常绑定在主相机上：
```
Main Camera → Add Component → Audio Listener
```

---

## 音频加载方式

### 三种加载方式

**1. Decompress on Load（默认）**
- 加载时解压
- 占用内存大
- 播放速度快
- 适用：短音效（< 1秒）

**2. Compressed in Memory**
- 内存中保持压缩
- 占用内存小
- 播放需要解压
- 适用：中等音效（1-10秒）

**3. Streaming（流式加载）**
- 边播放边加载
- 内存占用最小
- 播放有轻微延迟
- 适用：长音频（BGM、语音）

**设置方法：**
```
选择AudioClip → Inspector → Load In Background / Compressed In Memory / Streaming
```

---

## 音量控制

### 线性音量（0-1）

```csharp
audioSource.volume = 0.5f; // 50%音量
audioSource.volume = 1.0f;   // 100%音量
audioSource.volume = 0.0f;   // 静音
```

### 分贝音量（dB）

Unity混音器使用分贝（dB）。

**转换公式：**
```
dB = 20 × log₁₀(linear)
linear = 10^(dB/20)
```

**转换代码：**
```csharp
// 线性 → 分贝
float LinearToDecibel(float linear)
{
    if (linear <= 0f)
        return -80f; // 最小分贝值
    return 20f * Mathf.Log10(linear);
}

// 分贝 → 线性
float DecibelToLinear(float dB)
{
    return Mathf.Pow(10f, dB / 20f);
}
```

**常用音量对照：**

| 线性音量 | 分贝值 | 说明 |
|---------|-------|------|
| 1.0 | 0 dB | 最大音量 |
| 0.5 | -6 dB | 半音量 |
| 0.25 | -12 dB | 1/4音量 |
| 0.1 | -20 dB | 1/10音量 |
| 0.01 | -40 dB | 1/100音量 |

---

## 2D vs 3D音效

### 2D音效（不受距离影响）

```csharp
audioSource.spatialBlend = 0f; // 纯2D音效
```

适用场景：
- UI音效
- 背景音乐
- 全局提示音

### 3D音效（受距离影响）

```csharp
audioSource.spatialBlend = 1f; // 纯3D音效

// 设置距离衰减
audioSource.minDistance = 1f;  // 最小距离（音量最大）
audioSource.maxDistance = 50f; // 最大距离（音量为0）
```

适用场景：
- 角色语音
- 环境音效
- 爆炸、枪声等

### 混合模式

```csharp
audioSource.spatialBlend = 0.5f; // 2D和3D混合
```

---

## 常用AudioSource方法

### 播放音频

```csharp
// 播放已分配的clip
audioSource.Play();

// 播放一次性音效
audioSource.PlayOneShot(audioClip);

// 延迟播放
audioSource.PlayDelayed(0.5f); // 延迟0.5秒

// 在指定时间播放（用于精确同步）
audioSource.PlayScheduled(AudioSettings.dspTime + 0.5);
```

### 停止和暂停

```csharp
// 停止播放（从头开始）
audioSource.Stop();

// 暂停播放
audioSource.Pause();

// 恢复播放
audioSource.Unpause();
```

### 检查状态

```csharp
// 检查是否正在播放
if (audioSource.isPlaying)
{
    Debug.Log("正在播放");
}

// 获取当前播放时间
float currentTime = audioSource.time;
float totalTime = audioSource.clip.length;
float progress = currentTime / totalTime;

// 跳转到指定时间
audioSource.time = 10f; // 跳转到第10秒
```

---

## AudioMixer（音频混音器）

### 创建AudioMixer

```
Project右键 → Create → Audio Mixer
命名为"MainMixer"
```

### 添加Group（分组）

1. 打开AudioMixer编辑器
2. 点击"+"添加Group
3. 命名为"Master"、"Music"、"SFX"

### 设置音量

```csharp
using UnityEngine.Audio;

[SerializeField] private AudioMixer audioMixer;

void Start()
{
    // 设置主音量（0 dB = 最大）
    audioMixer.SetFloat("MasterVolume", 0f);

    // 设置音乐音量（-10 dB）
    audioMixer.SetFloat("MusicVolume", -10f);

    // 设置音效音量（-5 dB）
    audioMixer.SetFloat("SFXVolume", -5f);
}

// 线性音量转分贝
void SetVolume(string parameterName, float linearVolume)
{
    float dB = linearVolume > 0 ? 20f * Mathf.Log10(linearVolume) : -80f;
    audioMixer.SetFloat(parameterName, dB);
}
```

### AudioMixer快照

快照用于保存不同的音效配置，方便切换。

```
AudioMixer编辑器 → 点击"+"添加快照
命名为"Normal"、"Paused"、"Battle"
```

```csharp
// 切换快照
AudioMixerSnapshot normalSnapshot = audioMixer.FindSnapshot("Normal");
normalSnapshot.TransitionTo(1f); // 1秒过渡

AudioMixerSnapshot pausedSnapshot = audioMixer.FindSnapshot("Paused");
pausedSnapshot.TransitionTo(0.5f); // 0.5秒过渡
```

---

## 音频效果（Audio Effects）

### 混响（Reverb）

```csharp
AudioReverbFilter reverb = gameObject.AddComponent<AudioReverbFilter>();
reverb.reverbLevel = -10f;     // 混响电平（dB）
reverb.reverbDelay = 0.04f;    // 混响延迟
reverb.room = -1000f;          // 房间大小
```

### 低通滤波器（Low Pass Filter）

```csharp
AudioLowPassFilter lowPass = gameObject.AddComponent<AudioLowPassFilter>();
lowPass.cutoffFrequency = 5000f; // 截止频率（Hz）
lowPass.resonanceQ = 1f;          // 共振
```

用途：制作"在水下"或"隔墙听"的效果

### 高通滤波器（High Pass Filter）

```csharp
AudioHighPassFilter highPass = gameObject.AddComponent<AudioHighPassFilter>();
highPass.cutoffFrequency = 500f; // 截止频率（Hz）
```

用途：移除低频噪音

### 回声（Echo）

```csharp
AudioEchoFilter echo = gameObject.AddComponent<AudioEchoFilter>();
echo.delay = 500f;     // 延迟（ms）
echo.decayRatio = 0.5f; // 衰减比例
echo.dryMix = 1f;      // 原声混合
echo.wetMix = 0.5f;    // 回声混合
```

---

## 常见问题

### Q1: 音频播放不出来？
**A:** 检查以下几点：
1. AudioSource是否添加
2. AudioClip是否分配
3. AudioListener是否存在
4. AudioListener.volume是否为0
5. AudioSource.volume是否为0

### Q2: 音频有延迟？
**A:** 解决方案：
1. 使用WAV格式（无压缩延迟）
2. 减小音频文件大小
3. 使用PlayScheduled代替PlayDelayed

### Q3: 3D音效听不到？
**A:** 检查以下几点：
1. spatialBlend是否为1
2. AudioListener和AudioSource距离
3. minDistance和maxDistance设置

### Q4: 音频内存占用大？
**A:** 解决方案：
1. 使用Streaming加载方式（BGM）
2. 降低采样率（44100Hz → 22050Hz）
3. 使用OGG格式压缩

---

## 最佳实践

1. **使用音频池** - 预创建AudioSource，避免运行时实例化
2. **合理加载方式** - 短音效用Decompress，BGM用Streaming
3. **使用AudioMixer** - 分组管理音频，方便控制
4. **限制并发音效** - 同时播放AudioSource数量 < 32
5. **合理使用音频效果** - 避免过度使用效果器

---

## 版本说明

- 教程版本：1.0
- Unity版本：2021.3 LTS+
- 最后更新：2024年
