# 功能需求卡片: Chess/Entity P1-0 — 根对象与 Controller 生命周期

## 基本信息

- **功能名称**: Chess/Entity P1-0 — 根对象与 Controller 生命周期
- **所属模块**: `Packages/com.asoulchess.game.entity` + AVZ `Chess` 渐进适配
- **需求类型**: 功能重构 + 工程基建
- **优先级**: P0（Phase 1 第一步）
- **预估复杂度**: L2
- **预估耗时**: 1～2 天
- **提出日期**: 2026-08-10
- **上游规划**: [avz-full-framework-export](./avz-full-framework-export.md) Phase 1
- **审批**: [chess-entity-p1-0-lifecycle](../approvals/chess-entity-p1-0-lifecycle.md)（用户通过 2026-08-10）

## 功能描述

### 详细描述

将 AVZ `Chess` 的**组合根与 Controller 生命周期**抽入 `com.asoulchess.game.entity`，作为 Phase 1 的第一刀。

**本步只做「壳」**：

- `IEntityController` 生命周期三阶段（对齐 AVZ `Controller`）
- `GameEntity` 根 MonoBehaviour：`Init` → `EnterCombat` → `Tick` → `LeaveCombat` / `Die`
- Controller 注册表与统一遍历
- 框架侧 string 事件 id（`EntityEvents`），不含 `EventName`
- **移除**对 `com.asoulchess.game.board` 的 package 依赖

**本步不做**（后续 P1-1～P1-6 独立卡片）：

- PropertyController / Buff / State / Skill / Attack 迁入与行为对齐
- MoveController（永久留 AVZ）
- ChessFactory / ChessTeamManage（Phase 2）
- AVZ `Chess` 预制体批量改造（本步可零改动或仅文档化 Shim 方案）

### 用户故事

作为框架维护者，我希望 Entity 包先提供与 AVZ `Chess` 等价的**生命周期壳**，以便后续 Property、Buff、State、Skill 等 Controller 按依赖顺序逐个迁入，而不用一次性重写整个战斗层。

---

## Phase 1 分步路线图（总览）

| 步骤 | 内容 | 本卡片 |
|------|------|--------|
| **P1-0** | Entity 根 + `IEntityController` 生命周期；去 board 依赖 | ✅ 本文 |
| P1-1 | PropertyController + DamageMessage | 待办 |
| P1-2 | BuffController + Buff 基类 | 待办 |
| P1-3 | StateController + State/Transition 骨架 | 待办 |
| P1-4 | SkillController + ISkill 接口族 | 待办 |
| P1-5 | AttackController + IWeapon 最小实现 | 待办 |
| P1-6 | IAnimBridge 动画钩子 | 待办 |
| — | MoveController | **留 AVZ** |
| Phase 2 | EntityFactory + ITeamRegistry | 待办 |

---

## P1-0 框架边界

### 纳入框架

| 项 | AVZ 对照 | 框架形态 |
|----|----------|----------|
| Controller 契约 | `Controller.cs` | `IEntityController` |
| 组合根 | `Chess.cs` 生命周期部分 | `GameEntity` |
| 进场/离场/死亡清理顺序 | `InitChess` / `WhenChessEnterWar` / `Death` | `InitEntity` / `EnterCombat` / `Die` |
| 事件 id | 不使用 `EventName` | `EntityEvents`（string 常量） |
| 服务注入 | 隐式 `GameManage` | `IEventBus` / `ITimerService`（来自 Core `GameServices`） |

### 明确排除（P1-0）

| 项 | 理由 |
|----|------|
| `com.asoulchess.game.board` | 用户确认 Map/Tile 不进框架 |
| MoveController / `standTile` | 绑 PVZ 格子，留 AVZ |
| 全部具体 Controller 实现 | P1-1 起逐步迁入 |
| `LevelManage.IfGameStart` 门闩 | 改为 `EnterCombat` 显式调用 + 可选 `ISimulationGate` 注入（P1-0 仅预留，不实现关卡逻辑） |
| `ChessTeamManage.RecycleChess` | Death 回收由游戏侧注入 `IEntityRecycleHandler` 或 Phase 2 Factory |

---

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Chess` | Chess | 相似 / 源 | 7 个 Controller 字段 + 生命周期 |
| `Controller` | Chess | 相似 / 源 | Init / EnterWar / LeaveWar |
| `GameEntity` | Packages/entity | 已有 / 需重构 | 当前 bundled 全部 Controller 且依赖 board |
| `com.asoulchess.game.core` | Packages | 依赖 | EventBus、TimerService |
| `com.asoulchess.game.board` | Packages | **移除依赖** | P1-0 从 entity 包解耦 |

### 需求类型判定理由

- **功能重构**：从 AVZ `Chess` 抽生命周期；行为目标与现有进场/死亡清理顺序等价
- **功能扩展**：Entity 包从「竖切 Demo」变为可渐进扩展的 Controller 宿主

### 集成点

**框架 → Core**

- `GameServices.Events` / `IEventBus` — 触发 `EntityEvents.EnterCombat` / `Death`
- `GameServices.Timers` / `ITimerService` — P1-0 仅注入持有，Tick 由 `Update` 驱动

**框架 → 游戏（Hook，AVZ 后续实现）**

- `IEntityRecycleHandler.OnRecycle(GameEntity)` — 替代 `ChessTeamManage.RecycleChess`（P1-0 接口定义 + 空实现可跳过）
- 可选 `ISimulationGate.IsRunning` — 替代 `LevelManage.IfGameStart` 门闩（P1-0 不强制）

**AVZ → 框架（P1-0 不强制改 Chess）**

- 长期：`Chess : GameEntity` 或组合委托；本步允许 AVZ 零改动

### 对现有功能的影响

- **接口变更**: `GameEntity` API 精简；现有 Entity Demo 需改为「仅生命周期」Sample
- **行为变更**: 无 AVZ 运行时行为变更（未接 Shim 前）
- **包依赖变更**: `com.asoulchess.game.entity` **仅依赖** `com.asoulchess.game.core`（去掉 board）
- **Sample 变更**: `CombatGridDemo` 移至可选或标记 deprecated，新增 `LifecycleDemo`

---

## 技术要求

- **Unity 版本**: 2022.3+（与工程一致）
- **依赖模块**: 仅 `com.asoulchess.game.core` ≥ 0.2.0
- **程序集**: `AsoulChess.Game.Entity.asmdef` — 无 AVZ 程序集引用
- **性能要求**: Controller 遍历 O(n)，n 为注册 Controller 数；不低于 AVZ 单帧 `InitChess` 开销
- **兼容性要求**: 空项目引用 entity 包可编译；AVZ 不接 Shim 时不受影响
- **命名对齐**:

| AVZ | 框架 P1-0 |
|-----|-----------|
| `InitController(Chess)` | `InitController(GameEntity)` |
| `WhenControllerEnterWar()` | `OnEnterCombat()` |
| `WhenControllerLeaveWar()` | `OnLeaveCombat()` |
| `stateController.StateUpdate()` | 各 Controller `Tick(dt)` + 后续 P1-3 State 专责 |

---

## 功能清单

### 核心功能（必须 · P1-0）

- [x] **`IEntityController`**：Init / OnEnterCombat / OnLeaveCombat / Tick
- [x] **`GameEntity` 精简**：RegisterController → Init → EnterCombat / LeaveCombat / Die
- [x] **去掉 board 依赖**：主 asmdef 仅 Core；Move/Attack → `Optional/Board/`
- [x] **`EntityEvents`**：string 常量
- [x] **`package.json` 0.3.0**
- [x] **Sample `LifecycleDemo`**
- [x] **README**

### 扩展功能（可选 · P1-0）

- [x] `IEntityRecycleHandler` 接口定义
- [x] `ISimulationGate` 接口定义
- [ ] AVZ `Chess` partial Shim 设计文档（留 P1-1）

### 前置条件

- [ ] L0 Core 0.2.0 在 AVZ 可编译（理想：Unity smoke test；不阻塞 P1-0 包内开发）

---

## 验收标准

### 框架包

- [ ] 空项目仅引用 `core` + `entity` 可编译，**无** board 包
- [ ] `LifecycleDemo` 可运行：Console/log 可见 Controller 四阶段调用顺序
- [ ] `GameEntity` 不引用 `LevelManage`、`ChessTeamManage`、`MapManage`、`Tile`
- [ ] README 列出 P1-1 下一步与 Move 留 AVZ 说明

### AVZ

- [ ] 不接 Shim 时 AVZ **零行为变化**、可编译
- [ ] （可选）Shim 设计文档经用户扫一眼无歧义

### 流程

- [ ] Skills 白名单检查完成
- [ ] 审批报告：`docs/approvals/chess-entity-p1-0-lifecycle.md`

---

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 现有 Entity 0.2 bundled 设计与 P1-0「壳优先」冲突 | 高 | 中 | 拆分 GameEntity；Controller 类保留文件但移出 Init 默认链 |
| CombatGridDemo 因去 board 无法编译 | 高 | 低 | Sample 标记 optional 或迁到 AVZ 参考 |
| AVZ Controller 命名与框架 Tick 分裂 | 中 | 中 | 文档对照表；P1-3 State 再对齐 StateUpdate |
| L0 未验证即叠 P1-0 | 中 | 低 | P1-0 仅依赖 Core；L0 与 P1-0 可并行 |

---

## 关联 Context

- 涉及模块: Chess, Skill（后续）, State（后续）, Buff（后续）
- 参考文档: `context/modules/Chess.md`, `context/index.md`
- 父需求: `docs/requirements/avz-full-framework-export.md`

---

## 产出物（预期）

| 阶段 | 路径 |
|------|------|
| 需求卡片 | `docs/requirements/chess-entity-p1-0-lifecycle.md`（本文） |
| 审批报告 | `docs/approvals/chess-entity-p1-0-lifecycle.md` |
| 实现 | `Packages/com.asoulchess.game.entity/` |
| Sample | `Packages/com.asoulchess.game.entity/Samples~/LifecycleDemo/` |
