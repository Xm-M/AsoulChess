# 功能需求卡片: InRangeTransition_Interval

## 基本信息
- **功能名称**: InRangeTransition_Interval（带普攻间隔的进攻击迁移）
- **所属模块**: State / Transition
- **需求类型**: 新增功能（新 Transition 类，不改 AttackController）
- **优先级**: P1
- **提出日期**: 2026-04-05

## 功能描述
与 `InRangeTransition` 相同前提（`AttackAble` 且 `FindEnemy > 0`），且累计时间达到 `weapon.GetInterval() / propertyController.GetAccelerate()` 后才允许切入攻击态；触发后清零累计。无目标或不可攻击时清零累计。攻击态内本边不评估，间隔不含攻击动画占用时间。

## 集成点
- StateGraph：`MoveState`（或等价）→ `AttackState` 的迁移将 `InRangeTransition` 换为 `InRangeTransition_Interval`。
- 离开攻击：`IfAnimPlayOverTransition` 等（与 `OutRangeTransition` 解耦）。

## 验收标准
- [ ] 有敌且间隔未满时不进 `AttackState`
- [ ] 间隔满后进攻击且 `t` 重置
- [ ] 失目标后 `t` 不保留虚高

## 关联 Context
- `workflow_v2/context/index.md`
