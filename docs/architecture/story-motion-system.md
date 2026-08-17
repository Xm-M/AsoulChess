# 架构分析: StoryMotion

## 系统定位
- **所属模块**: Story（剧情演出）
- **上游**: Timeline Signal、`EnterMapPlugin_StoryStage`
- **下游**: `StoryMotionPlayer` Transform / 可选 Animator
- **横向**: 与 Dialogue Signal 并列，同属 StoryStage 编排

## 设计决策
| 决策 | 选择 | 理由 |
|------|------|------|
| Tween 库 | 自研 Curve + 协程 | 对齐对话自研、无 DOTween |
| 配置 | Preset SO 共用/特例 | 同状态机 SO 习惯 |
| Binding 位置 | stage 上 `StoryMotionBinder` | 引用实例化后的 Player/Anchor |
| Motion 时 Timeline | Pause | 与对话一致，避免抢位移 |

## 数据流
```text
Timeline Signal
  → StoryTimelineBridge
  → EnterMapPlugin_StoryStage.OnMotionSignal
  → Pause Director
  → StoryMotionPlayer.Play(preset, anchor)
  → onComplete → Resume
```

## 风险等级
低～中（仅改 StoryStage 插件与新增 Story 脚本）
