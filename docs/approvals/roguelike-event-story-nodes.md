# 需求开发审批报告：肉鸽事件节点 — 剧情关（RG-002-A）

## 基本信息

- **需求名称**：Roguelike Event Story — 地图事件剧情节点
- **需求 ID**：RG-002-A
- **所属模块**：Roguelike / LevelSystem / Dialogue
- **分析日期**：2026-05-20
- **修订**：2026-05-20 — stage 预制体一体化（场景+演员+Timeline 打包）

## 需求摘要

为肉鸽 `MapRoomType.Event` 实现 **非战斗剧情路径**：通用场景 `RoguelikeStory` + **stage 预制体（场景/演员/Timeline 一体）** + `EnterMapPlugin_StoryStage` 编排对话与结算，一期奖励为 **道具**，战斗与小游戏另开 RG-002-B。

**需求类型**：功能扩展  
**与现有功能的关系**：对称扩展 Shop/Rest 非战斗节点；复用 Dialogue + LevelData 插件；Timeline 模式参考 `MapManage_PVZ.dir`

## 分析结果汇总

### Context 复用

- 已读取 `context/index.md`（v2.0.0）
- 涉及模块：LevelSystem、UI、Roguelike、Dialogue、Event
- 参考：`context/modules/LevelSystem.md`

### 现有业务分析

- **相关现有功能**：Rest/Shop 非战斗节点、`EnterMapPlugin_Dialogue`、`LevelOutCome_Roguelike`
- **需求类型判定**：功能扩展 — Event 从「误进战斗」改为「剧情关」
- **集成点**：`RoguelikeRunService` 进出节点、`LevelManage.ChangeLevel`、`DialoguePanel`、Timeline Signal
- **对现有功能的影响**：Event 点击分支变更；新增 StoryMode，不改动战斗关默认行为

### 需求卡片

- 已生成 → `docs/requirements/roguelike-event-story-nodes.md`
- 含 Timeline 编排、奖励链路、StoryActor 规范、示例 Beat 表

### Skills 白名单检查

| 技术点 | Skill | 结论 |
|--------|-------|------|
| Timeline / PlayableDirector / Signal | `@unity-timeline` | ✅ |
| UI 对话面板 / TMP / Button | `@unity-ui-system` | ✅ |
| 2D Sprite / Animator | `@unity-2d-sprite` / `@unity-2d-animation` | ✅ |
| 场景加载 / ChangeLevel | `@unity-scene-management` | ✅ |
| ScriptableObject 配置 | `@unity-scriptableobject-config` | ✅ |
| 协程（Dialogue 逐字已有） | `@unity-coroutine-system` | ✅ |
| Run 存档 PendingNode | `@unity-save-system` | ✅ |
| 预制体 stage / StoryActor | `@unity-prefab-system` | ✅ |

**未覆盖**：无阻塞项。Story 专用 Signal 命名规范建议在实现后写入策划文档。

**结论**：通过

### 架构分析（摘要）

| 决策 | 选择 | 理由 |
|------|------|------|
| 动画/移动 | **Timeline 全权** | 用户确认；策划可视化 K 轴 |
| 流程编排 | **`EnterMapPlugin_StoryStage`** | 暂停/对话/Complete 单点控制 |
| 对话 | **Timeline Signal → StoryStage → DialoguePanel** | 玩家点击与 Timeline 时间解耦 |
| 奖励 | **StoryStage → LevelOutCome_RoguelikeEventStory** | 不在 Timeline 绑经济；整段结束一次结算 |
| 角色 | **预摆在 stage 预制体内** | Editor 绑 Timeline；不用 Chess |
| 演员配置 | **无 Definition / 无 SlotBinding** | 对话用 `DialogueCharacter`；stage 内自包含 |
| 数据 | **LevelData StoryMode + eventStoryPool** | 与现有关卡工作流一致 |
| 场景 | **单壳场景 + 多 stagePrefab 一体化** | 用户定稿 2026-05-20 |
| Controller | **`LevelController_Story`** | 参考 TestArena，切断战斗链路 |

**设计模式**：插件编排（ILevelPlugin）+ Timeline Signal 驱动状态机（StoryStage 内隐式 FSM）

**风险等级**：中（主要在新场景与 Timeline 协作，技术可行）

### 影响面分析

**预估新增文件（约 9～11）** — stage 一体化后脚本更少

| 路径 | 说明 |
|------|------|
| `LevelController_Story.cs` | 剧情关 Controller |
| `EnterMapPlugin_StoryStage.cs` | Instantiate stage + Signal + 对话（无绑轨） |
| `LevelOutCome_RoguelikeEventStory.cs` | 道具 + 回地图 |
| `StoryActor.cs` | 可选，stage 内标记 + 可选 DialogueCharacter 引用 |
| `RoguelikeEventDefinition.cs` | 事件 SO |
| `RoguelikeEventFlow.cs` | 抽事件 / 解析 LevelData |
| `MapManage_Story.cs`（或扩展 EnsureLevelController） | StoryMode 挂载 |
| `RoguelikeStory.unity` | 壳场景 |
| `DialoguePanel.cs` | 小幅扩展 Range API |
| `ModeManage.cs` | +StoryMode |
| `ActMapConfig.cs` | +eventStoryPool |
| `RoguelikeRunService.cs` | Enter/Leave Story |
| `RoguelikeMapPanel.cs` | Event 分支 |

**取消**：~~`StoryActorDefinition`~~、~~`StoryActorSlotBinding`~~、~~运行时 SetGenericBinding~~

**修改现有文件（约 4～6）**：见上表；`ResolveLevelForPendingCombat` 对 Event 一期不再用于剧情路径。

**回归测试项**

- [ ] Normal / Elite / Boss 战斗进关与奖励
- [ ] Shop / Rest 进出与存档
- [ ] Event → 剧情完整流程 + 道具入 Run
- [ ] 剧情中暂停回主菜单 → 继续冒险（PendingNodeId）
- [ ] `eventStoryPool` 为空时行为（Warning / 不可进）
- [ ] StoryMode LevelData 误挂战斗插件时 Editor Warning

**高风险点（2）**

1. Story 场景若误挂默认 `LevelController` 可能触发出怪 — **缓解**：专用场景 + EnsureLevelController 强制 Story
2. Timeline 未完成 Signal 即断线 — **缓解**：StoryStage OnDestroy 清理 Receiver；Complete 前不写 Run 通关

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 有 Rest/Dialogue/Timeline 先例 |
| 改动范围 | 中 | 新文件为主，Run 核心少量分支 |
| 性能风险 | 低 | 低频场景切换 |
| 时间评估 | 3～5 天 | 含示例 Timeline 与 1 条事件 |

## 建议的 Skills 使用清单

开发时加载：

- `@unity-timeline` — master Timeline、Signal、Director 暂停/恢复
- `@unity-ui-system` — DialoguePanel
- `@unity-2d-animation` — StoryActor Animator 绑定
- `@unity-scene-management` — RoguelikeStory 场景与回地图
- `@unity-scriptableobject-config` — EventDefinition / LevelData
- `@unity-prefab-system` — stagePrefab、StoryActor

## 开发优先级建议

**P0**

1. `StoryMode` + `LevelController_Story` + 空场景跑通 ChangeLevel
2. `EnterMapPlugin_StoryStage`：单 Timeline 播完 → Complete → 回地图（无对话）
3. Signal 对话暂停 + `ShowDialogueRange`
4. `LevelOutCome_RoguelikeEventStory` + Run 集成（Enter/Leave Event）
5. 1 条完整示例事件

**P1**

- `eventStoryPool` 加权抽取、读档恢复
- Editor StoryMode 校验

**P2（另开）**

- RG-002-B 小游戏 / RG-002-C 图文抉择

## 决策审批

✅ **通过** — 可以开始开发  
⬜ **修改后通过** — 需调整方案  
⬜ **驳回** — 暂不开发  

**审批意见**：采用 stage 预制体一体化（场景+演员+Timeline 打包）；Timeline 控动画；StoryStage 编排对话与奖励；一期道具、无战斗。  
**日期**：2026-05-20
