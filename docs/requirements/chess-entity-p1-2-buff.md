# 功能需求卡片: Chess/Entity P1-2 — Buff 系统

## 基本信息

- **功能名称**: Chess/Entity P1-2 — Buff 与 BuffController
- **所属模块**: `Packages/com.asoulchess.game.entity`
- **需求类型**: 功能重构
- **优先级**: P0
- **提出日期**: 2026-08-10
- **前置**: [P1-1 Property](./chess-entity-p1-1-property.md) ✅
- **审批**: [chess-entity-p1-2-buff](../approvals/chess-entity-p1-2-buff.md)
- **P1-2b AVZ 适配**: 用户决定 **暂不实施**

## 纳入框架

| AVZ | 框架 |
|-----|------|
| `Buff` / `BuffReset` | `Buff` / `OnReset` |
| `BuffController.AddBuff` | `BuffController.Add` |
| `TimeBuff` | `TimedBuff` |
| `Buff_BaseValueBuff_*` | `StatModifierBuff` |
| `takeBuff` | `DamageInfo.AttachedBuff` |

## 排除

- AVZ `Buff_*` 业务类、Save、P1-2b 适配层

## 验收

- [x] Entity 0.5.0；AVZ Buff 未改
- [x] Entity Demo TimedAttackBuff 可用
