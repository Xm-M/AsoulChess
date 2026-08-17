# 功能需求卡片: AVZ 全量逻辑框架导出（L0 基础设施）

## 基本信息

- **功能名称**: AVZ 全量逻辑框架导出（Full Framework Export）
- **所属模块**: 工程基建 / `Packages/com.asoulchess.*` + AVZ 渐进迁移
- **需求类型**: 功能重构 + 新增功能（工程基建）
- **优先级**: P0（L0 阶段）；整体为 P1 长期工程
- **预估复杂度**: L0 = L2；全量导出 = A 级（多阶段）
- **预估耗时**: L0 约 3～5 天；全量按模块分期另行评估
- **提出日期**: 2026-08-10
- **上游规划**: [reusable-systems-packaging](./reusable-systems-packaging.md)、[core-upm-package](./core-upm-package.md)、[combat-framework-vertical-slice](./combat-framework-vertical-slice.md)
- **工作流**: `@requirement-workflow` P1-0 实现完成（2026-08-10）

## 功能描述

### 详细描述

将 AVZ（AsoulChess）的**可复用游戏逻辑**逐步导出为 Unity UPM 框架包族，使新项目可引用同一套战斗/关卡/管理基础设施，而不拷贝 `Assets/Script` 业务代码。

**与既有 slim UPM 的区别**：

| 方向 | 既有尝试（`com.asoulchess.game.core/.entity/.board`） | 本需求目标 |
|------|--------------------------------------------------------|------------|
| 范围 | 机制骨架 + 空项目 Demo | **通用基础设施 UPM**（L0 Manage 层）；**玩法模块**（Map / Level / Chess / Skill 等）留 AVZ |
| AVZ 关系 | 选项 A：不动玩法代码 | **渐进迁移**：先拆 Manage，AVZ 侧用 Shim/Adapter 接线 |
| Save | 未涉及 | **明确排除**：各游戏自定义存档 |
| GameManage | 未纳入 | **最终纳入**，但必须在各 Manage 拆分完成之后 |

**本期卡片范围**：**Phase 0 — L0 基础设施**（用户已逐项确认边界）。Phase 1+（Chess / Skill 等机制骨架）仅列路线图，**Map / Level 插件 / LevelSystem 已永久排除**。

### 用户故事

作为开发者，我希望把 AVZ 中经过实战验证的全局基础设施（事件、UI 壳、计时、音频、对象池、物理查询缓冲池）抽成独立 UPM 包，以便在新 Unity 项目中直接引用，并在 AVZ 中逐步替换旧 Manage 实现，而不是一次性硬迁导致项目不可用。

---

## L0 框架边界（已确认）

### 纳入框架（Phase 0）

| 模块 | AVZ 源文件 | 框架内命名/形态 | 框架化要点 |
|------|------------|-----------------|------------|
| **事件总线** | `Event/EventController.cs` | `EventController`（或 `IEventBus` 对齐 Core 包） | 保留 Add/Remove/Trigger；**不含** `EventName` 枚举 |
| **UI 壳** | `Manage/UIManage.cs`、`UI/UIRoot.cs`、`UI/View/View.cs` | 同名或 `IUIService` | 仅 `GetView<T>` / Show / Close / UIRoot 注册；**剥离** Save、`StartUI`、`LoadSaveDataPanel` 等 AVZ 启动逻辑 |
| **物理查询缓冲池** | `Manage/CheckObjectPool.cs` | **`Physics2DQueryBufferPool`** | `RaycastHit2D[]` / `Collider2D[]` NonAlloc 借还；去掉 `EventName.WhenLeaveLevel` → 暴露 `ClearAll()` 由关卡调用 |
| **计时服务** | `Manage/TimerManage.cs` | 合并/扩展 `com.asoulchess.game.core` 的 `TimerService` | 保留 AddTimer、GameTime、LeftTime/ResetTime、倍速；**解耦** `LevelManage.IfGameStart` → 注入 `IsSimulationRunning`；去掉 Save 的 `GetSaveState` / `SetGameTimeForLoad` |
| **音频（精简）** | `Manage/AudioManage.cs`、`Chess/AudioPlayer.cs` | 框架版 + AVZ `AvzAudioPlayer` 扩展 | 见下文「Audio 定稿」 |
| **对象池** | `Manage/ObjectPool.cs` | 合并 `GameObjectPool` | Create/Recycle/ClearPool + 独立 Pool Scene；去掉 `EventName.GameStart`、`GameManage.IsDestroy`、死代码 `AddMember(Chess, Tile)` |
| **场景切换** | `Manage/SceneManage.cs` | **`SceneLoadService`**（或 `ISceneTransitionService`） | 异步卸载/叠加加载、进度、`beforeLoad`/`onLoaded` 回调；**剥离** PixelUI `ClockDemo`、Animator 过场、`LevelData` 重载、`EventName.WhenSceneLoad`；过场 UI 由游戏注入 |

### 排除原则（已确认）

**专门服务于特定游戏内容的模块一律不进框架**——只抽跨项目可复用的机制层；IP、关卡环境、羁绊、道具等玩法系统留在 AVZ（或各游戏项目）内实现。

### 明确排除（不进框架）

| 模块 | 理由 |
|------|------|
| **SaveSystem**（~7 脚本，~29 处引用） | 存档结构为 AVZ/肉鸽特化；框架只留生命周期 Hook，由游戏实现 `IProgressService` 等 |
| **GameCameraManage** | 仅 Shake + Level 耦合；全项目仅 `Effect_Shake` 使用 |
| **EventName 枚举** | 各游戏自定义事件名；框架只用 string |
| **具体 UI Panel** | `UI/View/*Panel` 留在 AVZ |
| **WeatherManage** + `Effect_Smoke` / `Effect_Snow` | 雾气/铺冰等 AVZ 关卡环境特效；依赖 Map、Level 插件、Chess/Skill，非通用「天气系统」 |
| **FetterController**（`fetterManage`） | 羁绊为 AVZ/Mygo/AveMujica 等 IP 玩法，永久留游戏侧 |
| **PropController** + `allProps` / `playerOwnedProps` | 本局被动道具（钉耙/闪电等）；AVZ 经济/肉鸽/存档特化，**永久留游戏侧** |
| **DifficultyManager** | Test/AVZ 难度配置，非框架能力 |
| **ChessFactory / ChessTeamManage** | 与 Chess 强绑定；**Phase 2 Chess 框架完成后再评估**是否抽象进包（非 L0） |
| **MapManage / Tile / IceCell 等** | AVZ 格子地图（PVZ 式 `Tile[,]`、种植列、冰格）；**永久留游戏侧**；其他项目地图结构可能完全不同 |
| **LevelSystem / 关卡插件体系** | PVZ 式关卡流程：`LevelManage`、`LevelController`、`LevelData`；`ILevelPlugin` / `IRoundEndPlugin` / `ISaveableLevelPlugin`；全部 `EnterMapPlugin_*` / `PreParePlugin_*` / `GameStartPlugin_*` / `LevelOutCome_*`（传送带、雾气、墓碑、小推车等）；**整体不进框架** |
| **com.asoulchess.game.board** | 早期竖切 Demo 包，**不纳入**全量框架必选项；若保留仅作可选参考 |
| **Chess / Skill 玩法内容** | 机制骨架可分期评估（Phase 1+）；**关卡与地图表示不进包** |

### Audio 定稿（用户选方案 1）

**框架 AudioManage（~80–100 行）**

- `AddPlayer` / `RemovePlayer`
- `ChangeSoundEffect` / `ChangeBGMValue`
- `PauseAll` / `ResumeAll` / `StopAll`
- **唯一 dedup**：`PlayAudioUniqueLimit` 机制（`AcquireUniqueLimited` / `ReleaseUnique` 可内聚）
- **不监听** `EventName`；离关/暂停由游戏层显式调用

**框架 AudioPlayer（~180–220 行）**

| 保留 | 删除（留 AVZ `AvzAudioPlayer`） |
|------|--------------------------------|
| `PlayAudio`, `RandomPlay`, `Play`, `Stop`, `Pause`, `UnPause` | `PlayAudioUnique`, `PlayAudioUniqueRandom`, `PlayAudioUniqueLoop` |
| `SetLoop`, `Seek`, `ChangeAudio` | `PlayWithSubUnique*` 等 Unique 变体 |
| `PlaySubN`, `PlaySub(int, string)` | Prefab 动画事件仍走 AVZ Shim |
| **`PlayAudioUniqueLimit`** + `SetAudioUniqueLimit` | |
| `TryGetCurrentPlaybackState`, `TryPlayFirstClip`（P1 可选） | |

### GameManage 策略

- **L0**：不迁移 `GameManage` 本体；AVZ 继续用现有单例组合根
- **访问约定（已确认）**：框架化后 **统一经 `GameManage.instance`（或 Phase 3 的薄组合根）取各 Manage**，逐步废弃平行单例直调（如 `SceneManage.instance`、`ObjectPool.instance` 对外改为 `GameManage.instance.xxx` 或注入接口）
- **AVZ 侧长期保留字段**（不进框架包）：`weatherManage`、`fetterManage`、`propManage`、`allProps` / `playerOwnedProps`、`allChess` / `playerOwnedCreators`、Save 钩子等——可收敛为 `AvzGameManage` 或 partial，与框架薄组合根分离
- **Phase 3（最后）**：框架侧瘦身为 `GameServices` / 薄组合根，**仅**挂载 L0 及后续已框架化的通用接口；不含 AVZ 特化字段

---

## 分期路线图

```
Phase 0 (L0) ── 本卡片 ── Event / UI壳 / Physics2DQueryBufferPool / Timer / Audio / ObjectPool / SceneLoad
    │
Phase 1 ── Chess/Entity 机制骨架（逐项迁入，Move 留 AVZ）
    │    P1-0 ✅ → [chess-entity-p1-0-lifecycle](./chess-entity-p1-0-lifecycle.md)
    │    P1-1 ✅ → [chess-entity-p1-1-property](./chess-entity-p1-1-property.md)（Property + DamageInfo）
    │    P1-2 ✅ → [chess-entity-p1-2-buff](./chess-entity-p1-2-buff.md)（Buff + BuffController）
    │    P1-3 ✅ → [chess-entity-p1-3-state](./chess-entity-p1-3-state.md)（State FSM）
    │    P1-4～P1-6 待办：Skill → Attack → IAnimBridge
    │         ※ Map / Tile / MapManage 不进框架（用户确认 2026-08-10）
    │         ※ LevelSystem / 关卡插件 不进框架（用户确认 2026-08-10）
    │         ※ MoveController 留 AVZ（用户确认 2026-08-10）
    │
Phase 2 ── ChessFactory / ChessTeamManage（Chess 骨架定稿后再拆）
    │
Phase 3 ── 瘦身 GameManage + AVZ 切框架 API（最后）
    │
    └── 或：L0 验证通过后 **停在 L0**，不再扩展框架包
```

**L0 实施策略**：

1. 在 `Packages/com.asoulchess.game.core`（或拆分子包）中**合并**已有 Core 实现与 AVZ 源行为，避免双轨 API 长期并存
2. AVZ 侧新增 **Adapter/Shim**（如 `AvzAudioPlayer`），保证 Prefab 动画事件与 Unique 系列 API 不断
3. **单模块迁移 + 回归**：每迁一个 Manage，跑相关场景 smoke test
4. **不一次性删除** `Assets/Script/Manage` 旧文件，直到 AVZ 引用清零

---

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| EventController | Event | 相似 / 迁入 L0 | ~1 文件；使用面广但实现无外部依赖 |
| UIManage + UIRoot + View | Manage / UI | 相似 / 迁入 L0 | 壳简单；Init 与 Save 强耦合需剥离 |
| CheckObjectPoolManage | Manage | 相似 / 迁入 L0 | ~20 处引用；static API |
| TimerManage | Manage | 相似 / 合并 Core | 与 LevelManage、Save、EventName 耦合 |
| AudioManage + AudioPlayer | Manage / Chess | 相似 / 精简迁入 | ~530 行 Player；大量 Unique API 留 AVZ |
| ObjectPool | Manage | 相似 / 合并 Core | ~60+ 处引用；含 EventName/GameManage 耦合 |
| GameManage | Manage | 被依赖 / Phase 3 | 15 文件 Manage 模块的组合根 |
| SaveSystem | SaveSystem | **排除** | GameManage/UIManage/Level/Roguelike 等多处引用 |
| com.asoulchess.game.core | Packages | 已有 / 需合并 | EventBus、TimerService、GameObjectPool |
| com.asoulchess.game.entity/.board | Packages | 并行路线 | 战斗竖切；与本需求 L0 可共存 |

### 需求类型判定理由

- **功能重构**：从 AVZ 单体脚本抽离 Manage 层，行为目标等价
- **新增功能**：UPM 包边界、接口注入、AVZ Shim 层为新增工程资产
- 非局内玩法功能，属跨模块工程基建

### 用户已确认选型

| 项 | 选择 |
|----|------|
| 交付形态 | Unity UPM Monorepo（`Packages/com.asoulchess.*`） |
| 框架范围 | 全量逻辑（分期）；**L0 先落地** |
| Save | **不进框架** |
| EventName | **不进框架**（各游戏自定义 string） |
| Audio dedup | **仅 `PlayAudioUniqueLimit` 进框架** |
| GameCameraManage | **不进框架** |
| ObjectPool | **进框架**（与 Core 合并） |
| SceneManage | **进框架**（精简为 SceneLoadService；过场 UI/Animator 留 AVZ） |
| GameManage | **最后**再瘦身为框架组合根 |
| WeatherManage | **永久不进框架**（AVZ 环境特效） |
| FetterController | **永久不进框架**（IP 羁绊玩法） |
| PropController | **永久不进框架**（AVZ 道具/经济） |
| Map / Tile / MapManage | **永久不进框架**（PVZ 格子地图） |
| LevelSystem / 关卡插件 | **永久不进框架**（PVZ 关卡流程与插件） |
| ChessFactory / ChessTeamManage | **Phase 2 Chess 后再拆**，非 L0 |
| 内容模块原则 | **专门服务特定游戏内容的模块不保留** |
| AVZ 迁移 | **渐进**（非 core-upm 的「零改动选项 A」） |
| Manage 访问 | **统一经 GameManage**（废弃散落单例直调） |

### 集成点

**框架 → 游戏（Hook，由 AVZ 实现）**

- 关卡开始/结束：`TimerManage.ClearAll()`、`Physics2DQueryBufferPool.ClearAll()`、`ObjectPool.ClearPool()`、`AudioManage.StopAll()`
- 场景切换：框架 `LoadSceneAsync` + 可选 `ISceneTransitionView`；**不**硬编码 `EventName.WhenSceneLoad`
- 模拟暂停/恢复：`TimerManage.IsSimulationRunning`、`AudioManage.PauseAll/ResumeAll`
- UI 启动：游戏侧 `UIManage.Initialize()` 不含 Save/具体 Panel
- 进度持久化：游戏侧 Save，框架不提供 `GetSaveState`

**AVZ → 框架（L0 迁移后）**

- `GameManage` 仍创建各 Manage，但类型改为框架命名空间或接口
- `EventController.Instance` → 框架 EventBus（可保留同名兼容层）
- Prefab `AnimationEvent` → `AvzAudioPlayer` 保留 Unique 系列

**需要的新接口（L0）**

| 接口 | 用途 |
|------|------|
| `IEventBus` | 已有于 Core；与 EventController 对齐 |
| `ITimerService` + `IsSimulationRunning` | 解耦 LevelManage |
| `IGameObjectPool` | 合并 ObjectPool + GameObjectPool |
| `IPhysics2DQueryBufferPool` | 新命名，static 或单例均可 |
| `IAudioService` / `IAudioPlayer` | 精简 API |
| `IUIService` | GetView / Show / Close |
| `ISceneLoadService` | 异步切场景、进度、beforeLoad/onLoaded；可选过渡 UI 注入 |

### 对现有功能的影响

- **接口变更**: AVZ 逐步改用框架命名空间；过渡期保留 Shim 避免大规模改 Prefab
- **行为变更**: 目标行为等价；Event 监听清理由显式调用替代隐式 `WhenLeaveLevel`
- **数据变更**: 无存档格式变更（Save 不进框架）
- **工程变更**: Packages 扩展；`Assets/Script/Manage` 逐步变薄

---

## 技术要求

- **Unity 版本**: 与当前工程一致（2022.3+ / 现有 URP 2D 工程）
- **依赖模块**: L0 仅 `UnityEngine` + 可选 `UnityEngine.UI`（View 基类）；不依赖 Odin、不依赖 AVZ 玩法程序集
- **程序集**: 扩展 `AsoulChess.Game.Core.asmdef` 或拆 `com.asoulchess.game.framework`（架构分析阶段定名）
- **性能要求**: Event / Timer / Pool / Physics2D 缓冲池不低于现有 AVZ 基线（NonAlloc 路径保持）
- **兼容性要求**: AVZ 在 L0 迁移过程中可编译、可进关；框架包可独立被空项目引用
- **平台支持**: 与当前 2D 项目一致

---

## 功能清单

### 核心功能（必须 · Phase 0 / L0）

- [ ] 需求卡片审批通过（本文档）
- [ ] 架构分析：`docs/architecture/analysis/avz-full-framework-export-l0.md`
- [ ] 影响面分析：Save 引用、Timer/Audio/ObjectPool 调用点清单
- [ ] **EventController**：迁入框架；无 EventName
- [ ] **Physics2DQueryBufferPool**：改名 + `ClearAll()`；去掉 WhenLeaveLevel 监听
- [ ] **TimerService**：合并 AVZ TimerManage 能力 + `IsSimulationRunning` 注入
- [ ] **GameObjectPool**：合并 AVZ ObjectPool（Pool Scene、Create/Recycle/ClearPool）
- [ ] **AudioManage + AudioPlayer（精简）**：仅方案 1 dedup；无 EventName 监听
- [ ] **UIManage + UIRoot + View**：剥离 Save/StartUI；保留壳 API
- [ ] **AVZ Shim**：`AvzAudioPlayer`（Unique 系列）、`AvzSceneTransitionView`（加载时钟/Animator）、离关/暂停显式调用清单接入
- [ ] **Sample**：L0 Demo（事件 → Timer → Pool → 音频 → UI 壳最小演示）
- [ ] **README**：包边界、迁移指南、与旧 Core 包关系

### 扩展功能（可选 · L0）

- [ ] 将 `EventController` 与 `IEventBus` 统一为单一入口
- [ ] Editor 调试窗口（Timer 列表、Pool 统计）
- [ ] `TryGetCurrentPlaybackState` / `TryPlayFirstClip` 纳入框架 P1

### 后续阶段（非 L0 · 待独立卡片）

- [x] Phase 1 P1-0: Entity 根 + Controller 生命周期 — [chess-entity-p1-0-lifecycle](./chess-entity-p1-0-lifecycle.md) **已审批**
- [ ] Phase 1 P1-1～P1-6: Property / Buff / State / Skill / Attack / IAnimBridge
- [ ] Phase 2: Chess 骨架 + Controllers + Factory/TeamManage
- [ ] Phase 3: GameManage 瘦身 + AVZ 全量切框架
- [ ] 可选: **仅 L0**，不继续 Phase 1+

---

## 验收标准

### L0 框架包

- [ ] `Packages/com.asoulchess.game.core`（或既定包名）含 L0 全部模块，空项目可引用并编译
- [ ] Sample 可演示：订阅 string 事件 → 延时 Timer → Physics2D 缓冲借还 → ObjectPool Create/Recycle → PlayAudio / PlayAudioUniqueLimit → UIRoot 加载 View
- [ ] 框架程序集 **无** 引用：`SaveSystem`、`EventName`、`LevelManage`、`LevelController`、`ILevelPlugin`、`GameManage`（AVZ 特化）、`WeatherManage`、`FetterController`、`PropController`、`MapManage`、具体 Panel
- [ ] README 明确「什么不进框架」与 Phase 1+ 路线

### AVZ 迁移（L0 完成定义）

- [ ] AVZ 至少一条完整进关路径使用框架 L0 API（或 Adapter 透传）
- [ ] 离关/暂停时无依赖 `EventName.WhenLeaveLevel` 的隐式清理（改为显式调用文档化）
- [ ] Prefab 动画音频事件通过 `AvzAudioPlayer` 仍正常工作
- [ ] 无 Save 相关代码进入框架包

### 流程

- [ ] 阶段 3 Skills 白名单检查完成
- [ ] 阶段 4 架构分析文档完成
- [ ] 阶段 5 影响面分析完成
- [ ] 阶段 6 审批报告：`docs/approvals/avz-full-framework-export.md`

---

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 与已有 Core/Entity/Board 包 API 重复 | 高 | 中 | L0 以**合并**为主，文档标明单一真相源 |
| Timer/ObjectPool 隐式 Event 清理遗漏 | 中 | 高 | 离关清单 + 回归用例；影响面分析列全调用点 |
| Audio Prefab 动画事件断链 | 中 | 中 | AVZ 保留 `AvzAudioPlayer` Shim，不批量改 Prefab |
| Save 与 UIManage Init 剥离遗漏 | 中 | 中 | Init 拆成 `InitializeShell()` + AVZ `InitializeGame()` |
| 范围膨胀（一次迁完全部 Manage） | 高 | 高 | 本卡片 **锁 L0**；Phase 1+ 单独审批 |
| Context 过期（331 vs 540 脚本） | 中 | 低 | 实现前更新 `context/modules/Manage.md` |

---

## 关联 Context

- 涉及模块: Manage, Event, UI, Chess（AudioPlayer 参考）
- 参考文档: `context/index.md`, `context/modules/Manage.md`, `context/modules/Event.md`, `context/architecture/dependency-graph.md`, `context/architecture/decisions.md`
- 规划文档: `docs/architecture/reusable-systems-packaging-plan.md`
- 并行需求: `docs/requirements/core-upm-package.md`, `docs/requirements/combat-framework-vertical-slice.md`

---

## 产出物（预期）

| 阶段 | 路径 |
|------|------|
| 需求卡片 | `docs/requirements/avz-full-framework-export.md`（本文） |
| 架构分析 | `docs/architecture/analysis/avz-full-framework-export-l0.md` |
| 影响面分析 | `docs/architecture/impact/avz-full-framework-export-l0.md` |
| 审批报告 | `docs/approvals/avz-full-framework-export.md` |
| 实现 | `Packages/com.asoulchess.game.core/`（或拆分后的 Framework 包） |
| AVZ 适配 | `Assets/Script/Manage/` Shim + 渐进替换 |

---

## 待用户确认（阶段 2 出口）

请确认以下内容是否准确；确认后进入 **阶段 3～6**（Skills 白名单 → 架构分析 → 影响面 → 审批报告）：

1. **L0 七项**（Event / UI壳 / Physics2DQueryBufferPool / Timer / Audio精简 / ObjectPool / **SceneLoad**）边界是否与上表一致  
2. **排除项**（Save、Weather、Fetter、**Prop**、**Map/Tile**、**LevelSystem/关卡插件**、GameCameraManage、EventName、具体 Panel；内容模块原则）是否完整  
3. **Audio 方案 1**（仅 `PlayAudioUniqueLimit`）是否仍为最终选择  
4. **GameManage 放 Phase 3 最后** 是否同意  
5. **L0 实施为「合并 Core + 渐进迁移 AVZ」**，而非「只建包不动 AVZ」——是否同意  
