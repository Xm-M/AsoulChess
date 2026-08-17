# 功能需求卡片: 立希汪（椎名立希动物形态）

## 基本信息
- **功能名称**: 立希汪
- **需求 ID**: taki-wang
- **需求类型**: 新增功能
- **提出日期**: 2026-08-11

## 功能描述
椎名立希动物形态：独立 MainPlant，`fetterMemberId=椎名立希`，不变身。近战普攻；每次命中敌方叠「压力」+1。

## 已锁定
| # | 结论 |
|---|------|
| 1 | 独立，`fetterMemberId=椎名立希`，不变身 |
| 2 | 压力加在攻击命中的敌人 |
| 3 | 每次 +1 |
| 4 | 索敌格由 Prefab `IGridFindTarget` 配置（占位默认前方一格） |
| 5 | 被动 + 近战武器 + Asset/Prefab |

## 实现
- `PassiveSkillEffect_TakiWang`：`onTakeDamage` → `OkuwakiStressSpread.ApplyStressDelta`
- Prefab：`Weapon_Sample` + `CloseAttack` + `IGridFindTarget`
