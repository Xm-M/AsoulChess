# 功能需求卡片：肉鸽事件节点 — 剧情关（RG-002-A）

## 基本信息

- **需求 ID**：RG-002-A
- **功能名称**：Roguelike Event Story — 地图「？」节点剧情事件
- **所属模块**：Roguelike / LevelSystem / Dialogue / UI
- **需求类型**：功能扩展
- **优先级**：P1（肉鸽事件系统一期；小游戏战斗另开 RG-002-B）
- **预估复杂度**：L2（接近 A 级：新场景 + Controller + Timeline 编排 + Run 集成）
- **预估耗时**：3～5 天（含 1 条示例事件 + 美术资源占位）
- **提出日期**：2026-05-20
- **最后修订**：2026-05-20（stage 预制体一体化方案定稿）

## 功能描述

### 详细描述

实现肉鸽地图 `MapRoomType.Event` 的 **非战斗剧情路径**：点击事件节点 → 进入专用剧情场景 → 播放 Timeline 驱动的像素角色演出 + 对话框叙事 → 结算道具奖励 → 回到地图并标记节点通关。

**一期范围（用户确认）**

| 包含 | 不包含 |
|------|--------|
| 剧情事件（StoryActor + Dialogue + Timeline） | 小游戏 `eventLevelPool` 战斗（RG-002-B） |
| 道具奖励 MVP | StS 式地图内图文抉择 UI（RG-002-C） |
| 通用场景 + **stage 预制体一体化**（见下） | Chess 预制体直接当演员 |
| | 运行时生成演员 / `StoryActorSlotBinding` |
| `LevelMode.StoryMode` + `LevelController_Story` | 事件节点内嵌战斗 |

**动画与编排原则（用户确认 2026-05-20）**

- **StoryActor 的移动、离开、所有动画播放**：由 **Timeline**（Animation Track / Transform 等）控制。
- **节拍顺序、对话插入、奖励时机**：由 **`EnterMapPlugin_StoryStage`** 编排。
- **对话 UI**：复用现有 `DialoguePanel` + `DialogueData`；Timeline 通过 **Signal** 通知 StoryStage 暂停并弹出对话段。

**奖励触发原则（用户确认 2026-05-20）**

- **不在 Timeline 里直接写经济逻辑**（不 Signal → `AddProp`）。
- Timeline **最后一帧**（或整段演出结束 Signal）→ **`EnterMapPlugin_StoryStage` / `LevelOutCome_RoguelikeEventStory`** 统一发道具、清场、回地图。
- 与战斗关 `LevelOutCome_Roguelike` 对称：Outcome 负责「演出结束后的游戏状态变更」，Timeline 只负责「演出」。

### 用户故事

作为玩家，我希望在肉鸽地图点击「？」事件后观看一段像素角色剧情并获得道具，以便 Run 有叙事节奏且事件节点与战斗/商店/休息区分开。

作为策划，我希望用 **LevelData + stage 预制体 + Timeline + DialogueData** 配置一条完整事件（进门→对话→巡视→对话→离开），而不进入战斗插件链路。

### Stage 预制体一体化（用户定稿 2026-05-20）⭐

**一条事件 = 一个 stage 预制体**，在 Editor 内打包完成，运行时 **只 Instantiate，不生成/不绑轨**：

```text
StagePrefab_Event_XXX
├── 场景美术（背景、灯光等）
├── StoryActor_A（Sprite + Animator，Timeline 已绑）
├── StoryActor_B（可选）
├── PlayableDirector
│     playableAsset → masterTimeline（进门/巡视/离开 + Signal 标点）
│     绑定 → 已在预制体里连到各 StoryActor
└── SignalReceiver（可选，也可由 StoryStage 运行时挂）
```

| 决策 | 选择 | 说明 |
|------|------|------|
| 演员来源 | **预摆在 stage 内** | 不用 `List<DialogueCharacter>` 运行时 Instantiate |
| Timeline 绑定 | **Editor 内完成** | 不用 `SetGenericBinding` 运行时绑轨 |
| 站位/动画 | **Timeline K 轴** | 不用锚点、`StoryActorSlotBinding` |
| 对话头像/名字 | **`DialogueData` + `DialogueCharacter`** | 不单独做 `StoryActorDefinition` SO |
| `DialogueCharacter` 扩展 | **可选、非必须** | 仅对话框用现有字段即可；stage 内演员与 speaker 由策划对齐 |
| `StoryActor` 组件 | **保留、极简** | 可选挂 `DialogueCharacter` 引用方便 Editor 对照；无逻辑 |

**换事件**：复制 stage 预制体 → 改 Timeline / 演员 / 背景 → LevelData 只改 `stagePrefab` 引用。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `RoguelikeRunService.EnterShopNode` / `EnterRestNode` | Roguelike | 相似（高） | 非战斗节点范式 |
| `RoguelikeMapPanel.OnNodeClicked` | UI | 修改 | Event 当前走 `EnterCombatNode`，需分支 |
| `EnterMapPlugin_Dialogue` | LevelSystem | 复用 | 对话展示；StoryStage 内调用 |
| `DialoguePanel` / `DialogueData` | Dialogue | 复用 | 逐字、eventOnEnter、waitForEvent |
| `MapManage_PVZ.dir` + Timeline Signal | Map | 参考 | 战斗关流程节点；剧情关复用 Signal 模式 |
| `LevelController_TestArena` | LevelSystem | 参考 | 无波次、无战斗 Update 的 Controller 裁剪 |
| `LevelOutCome_Roguelike` | LevelSystem | 对称 | 战斗胜利回地图；剧情用 EventStory Outcome |
| `RoguelikeRunPropPool.AddProp` | Roguelike | 依赖 | 道具奖励 MVP |

### 需求类型判定理由

地图已有 Event 节点与 `eventLevelPool`，但点击等同战斗且无叙事 UI。本需求在 **现有 Run + LevelData 插件体系** 上扩展非战斗剧情路径，属于 **功能扩展**，非全新 Run 子系统。

### 集成点

- **调用**：
  - `RoguelikeRunService.EnterStoryEventNode` / `LeaveEventNode` / `ReturnToMapUI`
  - `LevelManage.ChangeLevel(storyLevelData)`
  - `DialoguePanel.ShowDialogue`（支持按 entry 区间播放）
  - `PlayableDirector.Play` / `Pause` / `time` / Signal Receiver
  - `RoguelikeRunPropPool.AddProp`（经 Outcome 或 StoryStage 封装）
- **触发**：
  - Timeline Signal：`Story.Dialogue.Start` / `Story.Dialogue.End`（命名可配置）
  - Timeline Signal：`Story.Sequence.Complete`（整段演出结束 → StoryStage 结算）
  - `EventController`：对话 entry 的 `eventOnEnter`（可选，用于 UI 次要效果；**角色动画以 Timeline 为准**）
  - `OnNodeResolved`（节点通关，与 Shop/Rest 一致）
- **新接口**：
  - `LevelController_Story`
  - `EnterMapPlugin_StoryStage`（编排核心：Instantiate stage、Signal、对话、Complete）
  - `LevelOutCome_RoguelikeEventStory`
  - `StoryActor`（可选标记组件，预制体上）
  - `RoguelikeEventDefinition` + `ActMapConfig.eventStoryPool`
  - `RoguelikeEventFlow.PickStoryEvent` / `ResolveStoryLevelForPendingNode`
- **明确不做**：
  - ~~`StoryActorDefinition`~~（合并进可选 `DialogueCharacter` 扩展，stage 预摆方案下甚至不需要 prefab 字段）
  - ~~`StoryActorSlotBinding`~~
  - ~~运行时 `SetGenericBinding`~~

### 对现有功能的影响

- **行为变更**：一期 Event 节点 **不再** `EnterCombatNode`；`eventLevelPool` 保留给 RG-002-B。
- **接口变更**：`LevelMode` 新增 `StoryMode`；`MapManage` 或 Story 场景 `EnsureLevelController` 识别 StoryMode 挂载 `LevelController_Story`。
- **数据变更**：`ActMapConfig` 增加 `eventStoryPool`；Run 存档无需关卡内快照（与 Rest 类似，PendingNodeId 保留即可）。

## 技术要求

- **Unity 版本**：与项目一致
- **依赖模块**：Roguelike、LevelSystem、Dialogue、UI、Event（EventController）
- **性能要求**：单事件 Timeline 同时绑定 Actor 数 ≤ 4；场景切换与 Rest/Shop 同级
- **兼容性要求**：旧 Act 配置无 `eventStoryPool` 时 Event 节点应 LogWarning 而非崩溃
- **平台支持**：与主游戏一致

## 架构设计

### 职责分层

```text
┌─────────────────────────────────────────────────────────┐
│ EnterMapPlugin_StoryStage（编排器 / 唯一流程大脑）        │
│  - Instantiate(stagePrefab)  // 演员+Director+TL 已在内  │
│  - 取预制体上 PlayableDirector → Play()                  │
│  - 监听 Signal → Pause/Resume → ShowDialogueRange        │
│  - Story.Sequence.Complete → Outcome / 离关              │
└─────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────┐
│ stagePrefab（Editor 内已配好）                             │
│  PlayableDirector → Animation/Transform Track → StoryActor │
│  Signal Track → StoryStage                                 │
└──────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────┐
│ DialoguePanel    │  ← 全局 UI，StoryStage 在 Signal 点弹出
└──────────────────┘
```

### 典型事件节拍（示例：进门→对话→巡视→对话→离开）

| Beat | 内容 | Timeline | StoryStage |
|------|------|----------|------------|
| 1 | 走进房间 | intro 片段或 master TL 前段：walk + 位移 | 播 Director；可选开始时 Hide 对话 |
| 2 | 第一段对话 | Signal `Story.Dialogue.1` | Pause Director → `ShowDialogueRange(0,2)` → 结束后 Resume |
| 3 | 巡视四周 | TL 中段：walk 路径 + look 动画 | 自动播放，无玩家点击 |
| 4 | 第二段对话 | Signal `Story.Dialogue.2` | Pause → `ShowDialogueRange(3,5)` → Resume |
| 5 | 离开房间 | TL 后段：walk out | 播完 → Signal `Story.Sequence.Complete` |
| 6 | 奖励 + 回地图 | — | `ApplyRewards` → `LeaveEventNode` |

**推荐 authoring**：一条事件优先使用 **一条 master Timeline**（含 Signal 标点），减少 StoryStage 多段拼接；复杂 Act 可拆 intro/outro 两个 Asset，仍由 StoryStage 顺序播放。

### 奖励链路（明确答案）

```text
❌ 不推荐：Timeline Signal → Inspector 绑 RoguelikeRunPropPool.AddProp
✅ 推荐：
   Timeline 末帧 Signal "Story.Sequence.Complete"
     → SignalReceiver（挂在 StoryStage 同 GO）
     → EnterMapPlugin_StoryStage.OnSequenceComplete()
         → LevelOutCome_RoguelikeEventStory.HandleOutcome()
             → RoguelikeEventDefinition.propIds / propReward
             → RoguelikeRunService.LeaveEventNode()
             → ReturnToMapUI()
```

**理由**：

1. 经济、Run 状态、节点通关必须在 C# 可测路径，不宜散落在 Timeline Inspector。
2. 对话可能插在 Timeline 中间，**不能在第一个 Timeline 结束就发奖**。
3. 与 `LevelOutCome_Roguelike` 一致，便于读档/跳过/二期加选项分支。

奖励配置放在 **`RoguelikeEventDefinition`**（或 Story LevelData 的 Outcome 序列化字段），StoryStage 只读配置并交给 Outcome 执行。

### LevelData（StoryMode）配置规范

```text
LevelData
  levelMode: StoryMode
  sceneName: RoguelikeStory          // 通用壳场景
  MaxWave: 0
  zombieList: 空
  EnterMapPlugin:
    - EnterMapPlugin_StoryStage
        stagePrefab: Stage_Event_XXX   // ★ 内含场景+演员+Director+Timeline
        dialogue: DialogueData_xxx     // 对话框文案（与 stage 内演出配合）
        dialogueRanges:                // Signal ↔ 对话 entry 区间
          - { signal: Story.Dialogue.1, start: 0, end: 2 }
          - { signal: Story.Dialogue.2, start: 3, end: 5 }
        // 无需 masterTimeline（已在 stage 的 Director 上）
        // 无需 actorBindings / spawnActors
  GameStartPlugin: 空
  PreParePlugin: 空
  outcome: LevelOutCome_RoguelikeEventStory
```

Editor 校验：`StoryMode` 且挂了战斗/出怪插件时 **Warning**。

### StoryActor 规范（stage 预摆方案）

- 作为 stage 预制体子物体：`SpriteRenderer` + `Animator`（Timeline 在 Editor 绑好）。
- 可选组件 `StoryActor`：仅 `[SerializeField] DialogueCharacter dialogueCharacter` 供策划对照「场景演员 ↔ 对话 speaker」。
- **无运行时逻辑**：不巡逻、不绑轨、不生成。
- 对话框仍用 `DialogueData.entries[].speaker` → `DialogueCharacter`（名字、头像）。

### 策划制作流程（一条新事件）

1. 复制模板 stage 预制体 → 改名 `Stage_Event_XXX`。
2. 摆背景、摆 StoryActor、在 Timeline 窗口 K 动画/位移/Signal。
3. 写 `DialogueData`，`dialogueRanges` 与 Signal 名对齐。
4. 新建 `LevelData`（StoryMode），`EnterMapPlugin_StoryStage.stagePrefab` 指向上面的预制体。
5. 写入 `RoguelikeEventDefinition` → `ActMapConfig.eventStoryPool`。

## 功能清单

### 核心功能（P0）

- [ ] `LevelMode.StoryMode` + `LevelController_Story`（无波次、无 ProgressBar、无战斗 GameStart）
- [ ] 场景 `RoguelikeStory` + `MapManage_Story`（或等价：`EnsureLevelController` 识别 StoryMode）
- [ ] `StoryActor`（可选标记，stage 预制体子物体）
- [ ] `EnterMapPlugin_StoryStage`（Instantiate stage、Signal、DialogueRange、Complete；**无运行时绑轨**）
- [ ] `DialoguePanel` 扩展：`ShowDialogueRange(data, start, end, onComplete)`（或 StoryStage 包装）
- [ ] `LevelOutCome_RoguelikeEventStory`（道具奖励 + 离关）
- [ ] `RoguelikeEventDefinition` + `ActMapConfig.eventStoryPool`
- [ ] `RoguelikeRunService.EnterStoryEventNode` / `LeaveEventNode`
- [ ] `RoguelikeMapPanel`：Event → `EnterStoryEventNode`
- [ ] `TryResumePendingNonCombatNode`：Event 恢复
- [ ] 1 条示例：`LevelData` + stage + master Timeline + Dialogue + 道具奖励

### 扩展功能（P2 / 另开需求）

- [ ] RG-002-B：`eventLevelPool` 小游戏战斗事件
- [ ] RG-002-C：地图内 StS 图文抉择（无独立场景）
- [ ] 剧情跳过 / 已读跳过
- [ ] 事件节点内分支选项（影响奖励）

## 验收标准

- [ ] 肉鸽地图点击 Event → 进入 `RoguelikeStory` → 完整播完 Timeline+对话 → 获得配置道具 → 回地图且节点已通关
- [ ] 全程无 ProgressBar、无出怪、无选卡、无 `EnterCombatNode`
- [ ] StoryActor 移动/动画仅由 Timeline 驱动（代码无硬编码巡逻）
- [ ] 奖励仅在 `Story.Sequence.Complete` 后触发一次
- [ ] 战斗/商店/休息/Boss 回归不受影响
- [ ] 示例事件可在 Editor 改 LevelData / Timeline / Dialogue 复配

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| Timeline Signal 与对话暂停不同步 | 中 | 中 | StoryStage 统一 Pause/Resume；Signal 命名规范 |
| 默认 LevelController 仍跑战斗逻辑 | 中 | 高 | Story 专用场景只挂 `LevelController_Story` |
| DialoguePanel 无 Range API | 低 | 中 | StoryStage 包装或小幅扩展 Panel |
| 美术 Timeline 工作量大 | 中 | 中 | 先 1 条 master TL 模板；文档给 Beat 示例 |
| Event 无 pool 配置 | 低 | 低 | 空 pool Warning + 节点不可点或占位文案 |

## 关联 Context

- 涉及模块：LevelSystem、UI、Event（Dialogue 子目录）
- 参考文档：`context/modules/LevelSystem.md`、`context/index.md`
- 关联 issue 文档：`docs/roguelike-known-issues.md` RG-002

## 后续需求拆分

| ID | 名称 | 说明 |
|----|------|------|
| RG-002-A | 本卡片 | 剧情事件 |
| RG-002-B | 小游戏事件 | `eventLevelPool` + 现有战斗链路 |
| RG-002-C | 图文抉择 | 地图内轻量 UI，可选 |
