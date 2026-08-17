# 功能需求卡片: Chess/Entity P1-1 — PropertyController 与伤害管线

## 基本信息

- **功能名称**: Chess/Entity P1-1 — PropertyController 与伤害管线
- **所属模块**: `Packages/com.asoulchess.game.entity`（框架）+ AVZ `Property.cs`（后续 Shim）
- **需求类型**: 功能重构
- **优先级**: P0（Phase 1 第二步）
- **预估复杂度**: L2
- **预估耗时**: 2～3 天（框架 1 天 + AVZ Shim 可选另计）
- **提出日期**: 2026-08-10
- **前置**: [P1-0](./chess-entity-p1-0-lifecycle.md) ✅ 项目已验证可运行
- **审批**: [chess-entity-p1-1-property](../approvals/chess-entity-p1-1-property.md)

## 功能描述

将 AVZ `PropertyController` 的**数值与伤害管线核心**迁入 Entity 包，UI / Animator / Buff / 关卡特化逻辑留 AVZ 订阅事件实现。

### 本步纳入框架

| AVZ 源 | 框架形态 |
|--------|----------|
| `Property` | `EntityStats` |
| `DamageMessege` | `DamageInfo` |
| `DamageType` / `ElementType` | 同名枚举（`Combat/`） |
| `GetDamage` / `TakeDamage` / `Heal` | `ReceiveDamage` / `DealDamage` / `Heal` |
| AR / 闪避 / 暴击 / 增伤减伤 / 吸血 | 同等公式 |
| `Change*` / `Get*` 属性 API | 迁入（不含 Animator/State 副作用） |
| `UnityEvent<DamageMessege>` ×4 | C# `event Action<DamageInfo>` |

### 本步排除（留 AVZ 或后续 P1-x）

| 内容 | 理由 |
|------|------|
| `UIManage` / `DamagePanel` | 游戏 UI；订阅 `ReceivedDamage` 等 |
| `animatorController.OnGetDamage` / `ChangeSpeed` | P1-6 IAnimBridge |
| `buffController.AddBuff(takeBuff)` | P1-2 Buff |
| `StateName.DizzyState` 切状态 | P1-3 State |
| `Buff_HoukagoTeaTime` 等 IP 逻辑 | AVZ 订阅 `BeforeReceiveDamage` |
| `PropertyCreator` SO | AVZ 内容；框架用 `EntityStats` 模板 |
| 修改 AVZ `Assets/Script/Chess/Property.cs` | **P1-1b 可选**；本步可零改动 |

## 伤害管线（框架）

```
DealDamage(DamageInfo)  // 攻击方
  → 暴击 / 增伤
  → Target.ReceiveDamage(info)
       → BeforeReceiveDamage (可改 Amount)
       → 护甲 / 减伤 / 闪避
       → 扣 HP / ReceivedDamage / HpRatioChanged
       → Grind 体型碾压 → Die()
       → HP≤0 → Die()
  → DamageDealt / 吸血 Heal
```

## 功能清单

### 核心（P1-1 · 框架）

- [x] `EntityStats` + `DamageInfo` + 枚举
- [x] `PropertyController` 完整数值 API + 伤害管线
- [x] C# 事件替代 UnityEvent
- [x] `EntityCombatBootstrap` 支持 `EntityStats` 模板
- [x] `GameEntity.IsAlive` 结合 Property HP
- [x] package **0.4.0** + README

### 可选（P1-1b · AVZ Shim，未做）

- [ ] `AvzPropertyController` 或 Property partial：接 DamagePanel + Animator + Buff
- [ ] `DamageMessege` → `DamageInfo` 适配扩展方法

## 验收标准

- [x] Entity 包编译；AVZ **未改**时仍可正常运行
- [ ] Entity Demo：Space 键暴击/护甲/闪避行为与改前 Sample 一致（待 Unity 验证）
- [x] 框架程序集无 `UIManage` / `DamagePanel` / `PropertyCreator` 引用

## 关联 Context

- `context/modules/Chess.md` — PropertyController 职责
- 父路线: `avz-full-framework-export.md` Phase 1 P1-1
