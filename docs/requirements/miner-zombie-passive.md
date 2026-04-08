# 功能需求卡片: 矿工僵尸被动（地下掘进 → 出土作战）

## 基本信息
- **功能名称**: 矿工僵尸被动技能（掘进状态 / 出土状态）
- **所属模块**: 技能系统（`ISkillEffect` 被动）+ `Chess`（选中、攻击）+ `MoveController`（位移、落地）
- **需求类型**: 功能扩展（在现有棋子/被动框架上新增一种状态切换）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 4–8 小时（含Prefab与动画对接）
- **提出日期**: 2026-03-30
- **修订**: 2026-03-30 — 移速改为 `ChangeMoveAcceleRate`；出土移除加速；出土条件改为**第 0 列**（`mapPos.x == 0`）

## 功能描述

### 详细描述
矿工僵尸在**进场后**处于「掘进」状态：**无法被选中**、**无法普攻**（与土豆地雷埋地类似），同时通过 **`PropertyController.ChangeMoveAcceleRate(+3f)`** 提高**仅移速**（不影响攻速/CD/动画速度，见 `Property.cs` 注释）。

当棋子**移动到第一列**（`mapPos.x == 0`）时，通过 **`MoveController.OnReachTile`** 检测并切换为「出土」状态：**可被选中**、**可普攻**、**转向**；同时 **`ChangeMoveAcceleRate(-3f)`** 撤销掘进移速加成。

### 用户故事
作为关卡设计者，我希望矿工僵尸先以掘进移速、不可交互的方式移动到第 0 列，再出土参与战斗。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 说明 |
|------|------|------|
| 土豆地雷被动 | `PassiveSkillEffect_PotatoMine` | `AttackAble = false` |
| 冰道尾迹 | `PassiveSkillEffect_IceTrail` | 使用 `ChangeMoveAcceleRate` 逐格改移速倍率 |
| 无法选中 | `Chess.UnSelectable` / `ResumeSelectable` | Layer |
| 地图列 | `MapManage.mapSize`、`Tile.mapPos` | **第一列** `x == 0` |
| 落地检测 | `MoveController.OnReachTile` | 与 `Armor_Sled` 冰道检测同类 |

### 集成点（已定稿）
- **移速（仅移速）**: `user.propertyController.ChangeMoveAcceleRate(3f)` 掘进开始；出土 `ChangeMoveAcceleRate(-3f)`。
- **选中 / 攻击**: `UnSelectable` / `ResumeSelectable`；`equipWeapon.AttackAble`。
- **出土条件**: `OnReachTile` 中 `newTile != null && newTile.mapPos.x == 0`（并校验 `MapManage.instance` 存在时可与 `mapSize` 做范围检查）。
- **转向**: `UpdateFacingFromHorizontalMove` 或 `ForceFlip`，按现有僵尸习惯。

### 与「加速 Buff」的区别
- `Buff_BaseValueBuff_AcceleRate` 走 `ChangeAcceleRate`，会带动画/攻速等。
- 本需求 **掘进加速** 用 **`ChangeMoveAcceleRate`**，与「只改移速」一致。

## 功能清单

### 核心功能（必须）
- [ ] `WhenEnterGame`（或等价）：`UnSelectable`、`AttackAble = false`、`ChangeMoveAcceleRate(+3f)`。
- [ ] 订阅 `OnReachTile`：`newTile.mapPos.x == 0` 时出土。
- [ ] 出土：`ResumeSelectable`、`AttackAble = true`、`ChangeMoveAcceleRate(-3f)`、转向、取消订阅。
- [ ] `MapManage.instance == null` 时安全降级（不抛异常）。

## 验收标准
- [ ] 掘进阶段：不可选中、不普攻、移速相对未掘进增加（与 `+3` 的 `moveAcceleRated` 增量一致）。
- [ ] 落地在 **x==0** 列时出土：可选中、可普攻、掘进移速已移除（`moveAcceleRated` 恢复）。
- [ ] 朝向合理（与移动或目标侧一致）。

## 风险评估
| 风险点 | 说明 |
|--------|------|
| 寻路是否必经 x=0 | 若关卡僵尸从不踏 x=0，需关卡/寻路配合 |
| 重复触发 | 出土后必须移除 `OnReachTile` 监听 |

## 关联 Context
- `workflow_v2/context/index.md`
