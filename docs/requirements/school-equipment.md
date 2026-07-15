# 功能需求卡片: 学校道具（自动贩卖机 / 文具盒 / 课本）

## 基本信息

- **功能名称**: 学校道具三件套 + 章鱼噼（已实现）
- **所属模块**: Chess + Skill + Buff + UI（阳光 / 手牌）
- **需求类型**: **新增功能**（3 个新 plant；章鱼噼已落地）
- **优先级**: P1（课本 / 贩卖机可 P2；章鱼噼 P0 已完成）
- **预估复杂度**: **L2**（贩卖机发牌交互略复杂）
- **提出日期**: 2026-07-01
- **状态**: **📄 仅文档沉淀** — 细节后续再定，暂不开发

## 功能描述

策划设计见 [`school-equipment-design.md`](../game-design/school-equipment-design.md)。

| 道具 | 定位 | 机制概要（用户指定） |
|------|------|----------------------|
| **自动贩卖机** | MainPlant | 坚果级 HP；花 **25 阳光** 摇奖 → 随机 **食物类** 卡牌（甜甜圈、抹茶芭菲、红茶等） |
| **文具盒** | SupportPlant | 套 Main；宿主 **普攻时额外发射 1 发随机文具弹** |
| **课本** | Consume | 种 Main 上；**右方角色 +压力**，**玩家 +阳光**；用后消失 |
| **章鱼噼** | Consume | ✅ 已实现 → [`octopus-takopi-consumable.md`](./octopus-takopi-consumable.md) |

### 用户故事

- 作为玩家，我希望在小卖部种贩卖机，用阳光抽奖换食物牌，支撑学校关经济。
- 作为玩家，我希望给射手套文具盒，让普攻附带 random 文具攻击。
- 作为玩家，我希望用课本换阳光，并承担给队友/右邻叠压力的代价。

## 现有业务上下文

### 相关现有功能

| 功能 | 关系 |
|------|------|
| `MainPlant` / `SupportPlant` / `ConsumePlant` | 种植规则 |
| `PlantsShop` / `SunLightPanel.ChangeSunLight` | 贩卖机扣 25 阳、发牌 |
| `Buff_Vocal.OnAttack` | 文具盒额外子弹模式 |
| `Buff_StressBuff_Death` / `context["stress"]` | 课本压力 |
| 食物 Consume：甜甜圈、蛋包饭、抹茶芭菲、牛肉饭 | 贩卖机奖池 |
| `SkillEffect_Takopi` / `ConsumablesSkill` | 课本 / 章鱼噼 Consume 管线 |
| [`band-stage-equipment-design.md`](../game-design/band-stage-equipment-design.md) | 同类道具壳参考 |

### 集成点（实现阶段）

- 贩卖机：奖池 SO + 手牌增加 API + 阳光校验
- 文具盒：`equipWeapon.OnAttack` + 随机 Bullet prefab
- 课本：右邻 `Tile` 查询 + `AddBuff(压力)` + `ChangeSunLight(+)`

### 对现有功能的影响

- 新 plant 独立；贩卖机若发牌需与 **商店手牌上限** 对齐（`PlantsShop.maxCount` 等）

## 功能清单

### 文档阶段（当前）

- [x] 策划设计三道具 + 章鱼噼索引
- [ ] 课本「右方角色」定稿
- [ ] 贩卖机发牌流程定稿
- [ ] 程序实现

### 实现阶段（后续）

- [ ] 自动贩卖机 / 文具盒 / 课本 Asset + 脚本
- [ ] 红茶等食物牌若未建 Asset 则奖池后补

## 验收标准（实现阶段启用）

- [ ] 贩卖机：HP 达坚果预期；25 阳不足时不可摇；奖池仅食物 Consume
- [ ] 文具盒：每次普攻额外 1 发；Support 仅可种 Main 上
- [ ] 课本：消耗后玩家阳光增加、目标压力增加（按拍板后的「右方」定义）
- [ ] 章鱼噼：回归不受影响

## 风险评估

| 风险 | 应对 |
|------|------|
| 贩卖机发牌与手牌/槽位冲突 | 实现前对照 `PlantsShop` 上限 |
| 课本右邻判定歧义 | §8 #1 拍板后再写代码 |
| 课本 + 章鱼噼压力联动过强 | 数值分关配置 |

## 关联 Context

- `context/modules/Chess.md`, `Skill.md`, `Buff.md`, `UI.md`
- 策划: `docs/game-design/school-equipment-design.md`

## 审批状态

- ✅ **文档沉淀** — 用户指定机制已写入，细节后续再定
- ⬜ 拍板后开发审批
