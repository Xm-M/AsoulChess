# 需求开发审批报告 — Chess/Entity P1-0

## 基本信息

- **需求名称**: Chess/Entity P1-0 — 根对象与 Controller 生命周期
- **所属模块**: `Packages/com.asoulchess.game.entity`
- **分析日期**: 2026-08-10

## 需求摘要

精简 `GameEntity` 为 Controller 生命周期壳，对齐 AVZ `Chess` + `Controller` 三阶段；移除 `com.asoulchess.game.board` 依赖；Property/Buff/State/Skill/Attack/Move 不在本步迁入。

**需求类型**: 功能重构 + 工程基建  
**与现有功能的关系**: 基于 AVZ `Chess` 生命周期扩展；合并并重写现有 Entity 0.2 竖切

## 分析结果汇总

### Context 复用

✅ 已读取项目 Context  
- 涉及模块: Chess, Manage（Phase 2 Factory）  
- 参考文档: `context/modules/Chess.md`

### 现有业务分析

- **相关现有功能**: `Chess`, `Controller`, 现有 `GameEntity`
- **需求类型判定**: 功能重构 — 先壳后 Controller
- **集成点**: Core `IEventBus` / `ITimerService`；Death 回收留游戏侧 Hook
- **对现有功能的影响**: P1-0 无 AVZ 运行时变更；Entity 包 API  breaking（去 board）

### 需求卡片

✅ 已生成 → `docs/requirements/chess-entity-p1-0-lifecycle.md`

### Skills 白名单检查

| 技术点 | Skill | 结论 |
|--------|-------|------|
| MonoBehaviour 生命周期 | unity-prefab-system | ✅ |
| 组件组合模式 | unity-design-patterns | ✅ |
| 协程 StopAllCoroutines | unity-coroutine-system | ✅ |
| 状态机（P1-3 预研） | unity-state-machine | ✅ 本步不实现 |
| ScriptableObject（不涉及） | — | — |

**结论**: 全部覆盖，可进入开发。

### 架构决策

- **包**: `com.asoulchess.game.entity` → **0.3.0**（建议）
- **依赖**: 仅 `com.asoulchess.game.core` ≥ 0.2.0；**移除 board**
- **模式**: Controller 组合 + 显式生命周期（对齐 AVZ）
- **Move**: 整模块留 AVZ；Factory/Team Phase 2
- **AVZ**: P1-0 不要求改 `Chess.cs`；Shim 后续

### 影响面（P1-0）

| 区域 | 影响 |
|------|------|
| `Packages/com.asoulchess.game.entity/Runtime/Entity/GameEntity.cs` | 高 — 精简 Init，去掉 board |
| Entity Controllers（Property/Skill/…） | 中 — 保留源码，移出默认 Init 链 |
| `CombatGridDemo` Sample | 高 — 依赖 board，需 deprecated 或迁出 |
| `Assets/Script/Chess/*` | 无（本步） |
| AVZ 预制体 | 无 |

**回归建议**: 跑 `LifecycleDemo`；AVZ 全量回归留 P1-1 接 Shim 后

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 已有 Entity 0.2 可改；范围清晰 |
| 改动范围 | 小～中 | 仅 Packages/entity + docs |
| 性能风险 | 低 | 生命周期遍历 |
| 时间评估 | 1～2 天 | 含 Sample + README |

## 建议的 Skills 使用清单

- `@unity-prefab-system` — GameEntity 预制体与 Sample
- `@unity-coroutine-system` — Death 时 StopAllCoroutines
- `@unity-state-machine` — P1-3 预读

## 开发优先级建议

- **P0**: IEntityController + GameEntity 生命周期 + 去 board + LifecycleDemo
- **P1**: package.json/README 更新
- **P2**: IEntityRecycleHandler / ISimulationGate 接口草案

## 决策审批

✅ **通过** — 开始 P1-0 实现（用户确认 2026-08-10）

审批意见: Phase 1 第一步；Move 留 AVZ；Factory/Team Phase 2  
日期: 2026-08-10

---

## 下一步

1. 实现 P1-0（`Packages/com.asoulchess.game.entity`）
2. 新增 `LifecycleDemo` Sample
3. L0 Unity smoke test 与 P1-0 可并行；AVZ 接 Shim 从 P1-1 起
4. P1-0 完成后开 **`chess-entity-p1-1-property`** 需求卡片
