# 功能需求卡片: StoryMotion 剧情通用位移

## 基本信息
- **功能名称**: StoryMotion（剧情关公式化位移）
- **所属模块**: Story / LevelSystem（RG-002-A 扩展）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 4～6 小时
- **提出日期**: 2026-07-17

## 功能描述
### 详细描述
在现有肉鸽剧情关（Timeline + Signal + Dialogue）上，增加薄层 **StoryMotion**：用 `AnimationCurve` + 协程执行可复用的跳跃/移动/冲刺等位移，由 Timeline Signal 触发；**不引入 DOTween**。日常走位仍用 Timeline Animation Track。

### 用户故事
作为策划，我希望用 Signal 触发「跳到锚点 / 冲到锚点」，手感用可复用 Preset SO 配置，以便少手调 Timeline Y 曲线。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `EnterMapPlugin_StoryStage` | LevelSystem | 扩展 | 注册 Motion Signal，Pause/Resume |
| `StoryTimelineBridge` | Story | 复用 | Signal → C# |
| `StoryDialogueRangeBinding` | Story | 相似 | Signal → 对话区间 |
| `Item_Coin` 曲线 | UI | 参考 | AnimationCurve + 协程 |

### 需求类型判定理由
在已有剧情编排上增加位移子能力，非新 Run 子系统。

### 集成点
- **调用**: `StoryMotionPlayer.Play`、`PlayableDirector.Pause/Resume`
- **触发**: Timeline Signal → Binding 查表
- **新接口**: Preset SO、Player、Binder（stage 上）

### 对现有功能的影响
- **接口变更**: `EnterMapPlugin_StoryStage` 读取 stage 上 `StoryMotionBinder` 并注册
- **行为变更**: Motion 期间 Pause Timeline（与对话一致）
- **数据变更**: 无存档变更

## 技术要求
- **依赖模块**: Story、LevelSystem、Timeline
- **性能要求**: 单事件同时 Motion ≤ 4
- **兼容性要求**: 无 Binder 的旧 stage 行为不变
- **平台支持**: 与主游戏一致

## 功能清单
### 核心功能（必须）
- [ ] `StoryMotionKind` / `StoryMotionPreset`（MoveTo、JumpTo、DashTo）
- [ ] `StoryMotionPlayer`（协程插值 + 可选朝向）
- [ ] `StoryMotionSignalBinding` + `StoryMotionBinder`（挂 stage）
- [ ] `EnterMapPlugin_StoryStage` 注册 Motion、Pause/Resume、离关 Stop
- [ ] `StorySignalNames` 命名约定

### 扩展功能（可选）
- [ ] Knockback
- [ ] Preset 内 Animator 状态名

## 验收标准
- [ ] Signal 触发后角色按 Preset 跳到 / 移到 Anchor
- [ ] Motion 期间 Timeline 暂停，完成后 Resume
- [ ] 同 Signal 多 Binding 并行，全部完成再 Resume
- [ ] 无 Binder 的示例剧情关仍可跑通
- [ ] 不引入第三方 Tween 库

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| Timeline 位移轨与脚本抢 Transform | 中 | 中 | 文档约定分段互斥 |
| Binding 引用预制体未实例化 | 低 | 高 | Binding 挂在 stage 实例侧 Binder |

## 关联 Context
- 涉及模块: LevelSystem、Story
- 参考: `docs/requirements/roguelike-event-story-nodes.md`

## 策划使用流程

1. **Create → Story → Motion Preset**（如 `Jump_Default`），填 kind / duration / 曲线  
2. stage 角色挂 **StoryMotionPlayer**（可与 StoryActor 同物体）  
3. stage 内摆空物体作 **Anchor**（落点）  
4. stage 根挂 **StoryMotionBinder**，添加 Binding：Signal + Player + Preset + Anchor  
5. Timeline Signal Track 在对应时刻放同一 **SignalAsset**  
6. 该时段 **不要**再录该角色 Position 轨；动作轨可同时放 Jump/Dash Clip  
7. 通用手感共用 Preset；特例复制一份改数值（同状态机 SO）
