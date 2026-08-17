# 功能需求卡片: Chess/Entity P1-3 — State 状态机

## 基本信息

- **功能名称**: P1-3 StateController + EntityStateGraph
- **所属模块**: `Packages/com.asoulchess.game.entity`
- **需求类型**: 功能重构
- **优先级**: P0
- **提出日期**: 2026-08-10
- **前置**: P1-2 ✅
- **审批**: [chess-entity-p1-3-state](../approvals/chess-entity-p1-3-state.md)

## 纳入框架

| AVZ | 框架 |
|-----|------|
| `State` Enter/Execute/Exit | `IEntityState` |
| `StateGraph` / `StateDate` | `EntityStateGraph` + `StateNode` |
| `Transition.ifReach` | `IStateTransition.Evaluate` |
| `StateController.StateUpdate` | `StateController.TickState` |
| `ChangeAnyState` | `EnterOverlayState` |
| `DizzinessState` | `DizzyEntityState` + `ChangeDizzinessTime` → Dizzy |
| `LevelManage.IfGameStart` | `ISimulationGate` → Prepare |

## 排除

- `StateGraph` SO 资产（延后）
- `InRangeTransition` 等地图条件
- AVZ `StateController.cs` 不改

## 验收

- [x] 默认图含 Idle/Attack/Skill/Dead/Dizzy/Prepare/Move
- [x] `IAnimBridge.PlayDizzy` 钩子
- [x] Attack/Skill 仍用 `SetState`
