# 功能需求卡片: 闪电道具 — DamageMessege 模板 + 命中特效

## 基本信息
- **功能名称**: 闪电道具配置增强
- **所属模块**: Prop（`PropEffect_Lightning`）
- **需求类型**: 功能扩展（已上线闪电的效果配置方式调整）
- **优先级**: P1
- **预估复杂度**: L1（约 0.5 人日）
- **提出日期**: 2026-06-11
- **状态**: 已实现

## 功能描述

### 详细描述
将闪电道具的伤害配置从分散字段（`damage` / `damageType` / `elementType`）改为 **一个可序列化的 `DamageMessege` 模板**（与 `CloseAttack.DM`、`Fetter.DM` 一致）。新增 **命中特效预制体** `GameObject`；闪电触发时在 **目标位置** 通过 `ObjectPool` 生成该特效。

### 与现有功能的关系
| 功能 | 关系 |
|------|------|
| `PropEffect_Lightning` | 直接修改 |
| `DamageMessege` | 配置模板；运行时 `damageFrom=null`，`damageTo=目标` |
| `PropEffect_Rake` | 同类：`GameObject` + `ObjectPool.Create` |
| `ObjectPool` | 特效生成 |

## 策划确认

| 项 | 决定 |
|----|------|
| 伤害配置 | 单字段 `DamageMessege`（含 damage、类型、元素、takeBuff 等） |
| 特效 | 可配置 `GameObject strikeEffectPrefab` |
| 特效位置 | 目标 `transform.position` |
| 无目标 | 不生成特效（与「计数仍消耗」一致） |
| 对象池 | 与钉耙相同，`ObjectPool.instance.Create` |

## 实现要点

```csharp
// 从模板复制，避免污染 SO 上序列化实例的 damageTo 引用
var mes = new DamageMessege(null, target, template.damage, template.damageType, template.damageElementType);
mes.ifCrit = template.ifCrit;
mes.suppressFloatingDamage = template.suppressFloatingDamage;
mes.takeBuff = template.takeBuff;
target.propertyController.GetDamage(mes);

if (strikeEffectPrefab != null)
    ObjectPool.instance.Create(strikeEffectPrefab).transform.position = target.transform.position;
```

## 验收标准
- [x] Inspector 显示 `DamageMessege` 块，可配伤害/类型/元素/Buff
- [x] 触发时在目标处生成配置的特效预制体
- [x] 旧 `闪电.asset` 迁移为 `damageMessage` 结构（damage=200, Magic）

## 关联文档
- `docs/requirements/prop-lightning.md`
