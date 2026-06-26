# 功能需求卡片: 闪电道具（PropEffect_Lightning）

## 基本信息
- **功能名称**: 闪电道具
- **所属模块**: Prop（`Assets/Script/Prop/`）
- **需求类型**: 功能扩展（在现有 `PropEffect` / `PropItemData` 上新增一种效果）
- **优先级**: P1（内容向；依赖肉鸽道具 P0 已通）
- **预估复杂度**: L1
- **预估耗时**: 0.5～1 人日（含 SO 配置 + 简单 VFX 可选）
- **提出日期**: 2026-06-11
- **状态**: 已实现（`PropEffect_Lightning` + `Assets/SO/Props/闪电.asset`）
- **增强**: 见 `docs/requirements/prop-lightning-vfx-damage-template.md`（DamageMessege 模板 + 命中特效）

## 功能描述

### 详细描述
持有「闪电」道具的本局战斗中：监听玩家**种植事件**；当己方累计种植 **n** 次后，从场上敌方单位中**随机选取 1 个可被选中的目标**，对其造成**可配置伤害**；触发后计数归零。若当时无可选目标，仍视为触发（计数消耗，伤害放空）。

### 用户故事
作为肉鸽玩家，我希望多种道具在战斗中有可见收益，以便 Run 内搜刮的道具不只是钉耙，形成不同构筑节奏。

## 策划确认（2026-06-11）

| 项 | 决定 |
|----|------|
| 种植统计范围 | **任意己方种植格**均计数（不限「右方」列） |
| 触发次数 n | **SO 可配置**，默认 **3** |
| 伤害数值 | **SO 可配置**（示例默认 200） |
| 伤害类型 | **SO 可配置**（`DamageType`，默认 `Magic`） |
| 触发后计数 | **归零** |
| 无可选目标 | **仍消耗计数**（闪电放空） |
| 计数事件 | 每次 `WhenPlantChess` 计 1（含列种植等多格；不含读档 `forRestore`） |
| 目标筛选 | `Enemy` 标签、`!IfDeath`、`!IfSelectable`（可被技能/索敌选中的单位） |

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PropItemData` + `PropEffect` | Prop | 扩展 | `SerializeReference` 配置进局效果 |
| `PropEffect_Rake` | Prop | 相似 | 监听事件，无运行时 Mono |
| `GameStartPlugin_Prop` | LevelSystem | 依赖 | 开局 `CheckProps()` + `PropPanel` |
| `EventName.WhenPlantChess` | Event | 集成 | `ChessManage.CreateChess` 种植时广播 |
| `DamageMessege` | Chess | 集成 | `damageFrom = null` 的环境伤害（参考羁绊） |

### 集成点
- **监听**: `EventController` + `WhenPlantChess`
- **种植判定**: 事件参数 `Chess` 为 `Player` 标签（事件本身仅正常种植触发）
- **目标池**: `ChessTeamManage.Instance.GetEnemyTeam("Player")`
- **伤害**: `new DamageMessege(null, target, damage, damageType)` → `GetDamage`

### 对现有功能的影响
- 无接口/行为变更；新增 `PropEffect_Lightning` + SO 资产

## 实现配置字段（`PropEffect_Lightning`）

| 字段 | 类型 | 默认 | 说明 |
|------|------|------|------|
| `plantCountThreshold` | int | 3 | 累计种植次数 n |
| `damage` | float | 200 | 单次闪电伤害 |
| `damageType` | DamageType | Magic | 伤害类型 |
| `elementType` | ElementType | None | 可选元素（默认无） |

## 技术要求
- 依赖：Event、Chess、Prop
- 性能：种植时 O(敌人数) 扫列表；无每帧逻辑
- 主线 / 肉鸽 / Test 共用

## 功能清单

### 核心功能（必须）
- [x] `PropEffect_Lightning`（`Assets/Script/Prop/PropEffect_Lightning.cs`）
- [x] `PropItemData` SO：`Assets/SO/Props/闪电.asset`
- [x] 注册 `GameManage.allProps`（`开始.unity`）
- [ ] （可选）肉鸽经济白名单 / 掉落

### 扩展功能（可选）
- [ ] 闪电 VFX / 音效
- [ ] 说明文案动态显示剩余次数

## 验收标准
- [ ] 默认 n=3、damage=200：种 3 次后随机可选敌方受伤
- [ ] 改 SO 伤害为其他值，实际伤害随之变化
- [ ] 无可选目标时计数仍归零、不报错
- [ ] `ClearProps` / 离关后不再监听
- [ ] 与钉耙等同局无冲突

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 列种植一次多格多次计数 | 低 | 低 | 按事件次数计，文档写明 |
| `damageFrom=null` 与护甲交互 | 低 | 中 | 走标准 `GetDamage` |

## 关联文档
- `docs/requirements/prop-system.md`
- `context/index.md`
