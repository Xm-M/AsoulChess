---
name: unity-timeline
description: Unity Timeline时间轴系统，提供可视化序列编排能力，用于创建过场动画、游戏事件序列、Boss战斗阶段切换等场景，支持动画、音频、事件、镜头等多轨道协调。
---

# Unity Timeline 时间轴系统

## 概述

Unity Timeline是强大的可视化时间轴编辑器，通过Timeline Asset和Playable Director组件，实现过场动画、游戏事件序列编排、Boss战斗阶段切换等复杂时序控制。支持动画、音频、事件、特效、镜头等多轨道协调工作。

## 核心概念

### Timeline Asset与Playable Director

- **Timeline Asset**：时间轴资源文件，存储轨道、剪辑和动画数据
- **Playable Director**：控制Timeline播放的核心组件，提供播放控制API
- **PlayableGraph**：底层播放架构，支持动态节点管理

### 六大内置轨道

| 轨道类型 | 功能 | 使用场景 |
|---------|------|---------|
| **Activation Track** | 控制GameObject显示/隐藏 | 角色出现、特效显示 |
| **Animation Track** | 播放和混合动画片段 | 角色动作序列、属性动画 |
| **Audio Track** | 控制音频播放和混合 | 背景音乐、音效同步 |
| **Signal Track** | 触发自定义事件和游戏逻辑 | 剧情对话、游戏事件触发 |
| **Control Track** | 控制粒子、Prefab、子Timeline | 粒子特效、动态创建对象 |
| **Cinemachine Track** | 控制虚拟相机切换 | 过场动画镜头切换 |

### Signal事件系统

**核心组件**：
- **Signal Asset**：信号资源，作为发射器和接收器的唯一标识符
- **Signal Emitter**：信号发射器标记，放置在Timeline特定时间点
- **Signal Receiver**：信号接收器组件，挂载在GameObject上监听信号

**工作原理**：
```
Timeline播放 → 到达Signal Emitter时间点 → 发射Signal Asset 
→ Signal Receiver接收 → 匹配Signal Asset → 触发UnityEvent
```

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 说明 |
|-----|------|------|
| `PlayableDirector` | 播放控制 | 核心组件 |
| `PlayableDirector.Play()` | 开始播放 | 标准播放方法 |
| `PlayableDirector.Pause()` | 暂停播放 | 保持当前时间 |
| `PlayableDirector.Stop()` | 停止播放 | 重置到初始状态 |
| `PlayableDirector.time` | 时间属性 | 获取/设置当前时间（秒） |
| `PlayableDirector.playableAsset` | Timeline资源 | 绑定TimelineAsset |
| `PlayableDirector.wrapMode` | 循环模式 | Hold、Loop、None |
| `TimelineAsset` | Timeline资源类 | 轨道管理 |

### ⚠️ 警告使用的API

| API | 风险 | 替代方案 |
|-----|------|---------|
| `director.time = value` | 可能导致跳帧 | 使用Evaluate()强制刷新 |
| 运行时动态添加轨道 | 可能内存泄漏 | 预先创建轨道 |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Timeline Asset创建和配置
- ✅ Playable Director基础使用
- ✅ 六大内置轨道详解
- ✅ Signal事件系统
- ✅ 动画录制与混合
- ✅ 运行时播放控制
- ✅ 2D游戏应用场景（过场动画、教程、Boss战斗、对话系统）

### 不在本Skill范围内

- ❌ Cinemachine详细配置 → 独立Skill
- ❌ 自定义Track开发 → 高级主题
- ❌ PlayableGraph底层API → 高级主题
- ❌ Timeline性能优化 → 高级主题

---

## 渐进式学习路径

### 阶段1：基础播放控制

```csharp
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    public PlayableDirector director;
    
    void Start()
    {
        director.Play();
    }
    
    public void PauseTimeline()
    {
        director.Pause();
    }
    
    public void StopTimeline()
    {
        director.Stop();
    }
    
    public void JumpToTime(float targetTime)
    {
        director.time = targetTime;
        director.Evaluate();
    }
}
```

### 阶段2：轨道绑定与Signal事件

```csharp
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineBinding : MonoBehaviour
{
    public PlayableDirector director;
    public TimelineAsset timeline;
    public GameObject targetObject;
    
    void SetupTimeline()
    {
        director.playableAsset = timeline;
        
        // 动态绑定轨道
        var tracks = timeline.GetOutputTracks();
        foreach (var track in tracks)
        {
            if (track.name == "Animation_Player")
            {
                director.SetGenericBinding(track, targetObject);
            }
        }
    }
}
```

### 阶段3：Signal事件触发

```csharp
// Signal Receiver脚本
public class TimelineEventReceiver : MonoBehaviour
{
    public void OnDialogueTrigger()
    {
        DialogueSystem.Instance.StartDialogue();
    }
    
    public void OnEnemySpawn()
    {
        EnemySpawner.Instance.SpawnEnemy();
    }
}
```

---

## 2D游戏应用场景

### 场景一：过场动画制作

**实现步骤**：
1. 创建Timeline Asset，配置多个虚拟相机
2. 使用Animation Track控制2D角色Sprite Animation
3. 使用Cinemachine Track实现镜头切换
4. 使用Audio Track同步背景音乐和音效
5. 使用Signal Track触发剧情事件

**关键配置**：
- Animation Track绑定带有Animator的角色
- Cinemachine Track绑定Main Camera的CinemachineBrain
- 镜头融合通过Timeline剪辑重叠实现

### 场景二：教程引导序列

**实现方案**：
1. 使用Signal Track在特定时间点触发UI提示
2. 使用Activation Track控制引导高亮物体
3. 使用Animation Track制作UI动画
4. 教程进度通过PlayerPrefs保存

**示例代码**：
```csharp
public class TutorialSystem : MonoBehaviour
{
    public PlayableDirector director;
    private int currentStep = 0;
    
    public void NextStep()
    {
        currentStep++;
        director.time = GetStepStartTime(currentStep);
        director.Play();
    }
}
```

### 场景三：Boss战斗阶段切换

**实现方案**：
```csharp
public class BossController : MonoBehaviour
{
    public PlayableDirector director;
    public TimelineAsset phase1Timeline;
    public TimelineAsset phase2Timeline;
    
    void Update()
    {
        if (health <= maxHealth * 0.5f)
        {
            TransitionToPhase2();
        }
    }
    
    void TransitionToPhase2()
    {
        director.playableAsset = phase2Timeline;
        director.Play();
    }
}
```

### 场景四：对话系统事件触发

**实现方案**：
1. 每句对话开始处放置Signal Emitter
2. Signal Receiver绑定对话管理器
3. 通过PlayableDirector.time控制对话跳转

---

## 最佳实践

### 1. 轨道组织原则

```
Timeline Asset
├── [Camera] Cinemachine Track
├── [Characters] Track Group
│   ├── Player Animation Track
│   └── NPC Animation Track
├── [Audio] Track Group
│   ├── BGM Audio Track
│   └── SFX Audio Track
└── [Events] Signal Track
```

### 2. 命名规范

```
Timeline命名：场景名_类型_序号.playable
例如：Level01_Cutscene_01.playable

轨道命名：类型_对象名
例如：Animation_Player, Audio_BGM
```

### 3. 事件监听

```csharp
public class TimelineEventListener : MonoBehaviour
{
    public PlayableDirector director;
    
    void OnEnable()
    {
        director.played += OnTimelinePlayed;
        director.stopped += OnTimelineStopped;
    }
    
    void OnDisable()
    {
        director.played -= OnTimelinePlayed;
        director.stopped -= OnTimelineStopped;
    }
}
```

---

## 常见问题

### Q1: Timeline不播放？

**检查清单**：
- [ ] Playable Director是否绑定Timeline Asset
- [ ] Timeline中是否有轨道和剪辑
- [ ] 是否调用了Play()方法

### Q2: Signal不触发？

**解决方案**：
1. 检查Signal Asset是否匹配
2. 检查Signal Receiver配置
3. 确认UnityEvent绑定正确

### Q3: 动画轨道不播放？

**检查**：
- Animator组件是否存在
- Animation Clip是否正确
- Clip是否设置Legacy模式（如需要）

---

## References

- [Unity官方文档 - Playables API](https://docs.unity3d.com/Manual/Playables.html)
- [Timeline实战案例](references/timeline-examples.md)
