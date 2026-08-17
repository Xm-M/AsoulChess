# 功能需求卡片: 黄瓜睦（窝瓜式水平移动下砸）

## 基本信息
- **功能名称**: 黄瓜睦 — 左右索敌 → 水平移动 → 动画下砸圆形伤害 → 自毁
- **所属模块**: Chess / Skill
- **需求类型**: 功能扩展（机制扩展）
- **优先级**: P0
- **预估复杂度**: L2
- **提出日期**: 2026-05-30

## 功能描述

### 详细描述

实现 **Mygo 文件夹下的黄瓜睦**（`黄瓜睦.asset` → `Prefab/ChessPrefab/Mygo/黄瓜睦`）为窝瓜式单位：

1. **索敌**：同排 **左右各 2 格**（`mapPos.x ±1、±2`）检测敌方僵尸；有目标则进入技能流程。
2. **阶段 A — 移动**：离开当前占格，**水平移动**到目标所在格（`MoveController.MoveToTarget(Tile)`）；**不用** `JumpToTarget`（垂直起伏由动画表现）。
3. **阶段 B — 下砸**：到位后播下砸动画；动画事件触发时，以自身为中心做 **圆形 Overlap**，对范围内所有敌方造成 **攻击力** 伤害，随后 **自毁**。

动画由策划/美术自行制作与接线；代码提供动画事件可调用的结算入口。

### 用户故事

作为玩家，我希望种下黄瓜睦后，当左右两格内出现僵尸时，她会水平冲到目标处下砸并清掉范围内敌人后消失，以便获得类似原版窝瓜的节奏与爽感。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `MoveController.MoveToTarget(Tile)` | Chess | 依赖 | 水平移到目标格 |
| `Physics2D.OverlapCircle` / `ICircleSearch` | Weapon/Skill | 复用 | 圆形范围检敌 |
| `PassiveSkillEffect_TomoriChessPiece` | Skill | 相似 | 定时检敌 → 范围伤 → 自毁 |
| `AnimatorController_Squash` | Chess | 可选 | 空壳，可挂下砸事件转发 |
| AveMujica `若叶睦` / 黄瓜 Prefab | Chess | **不改** | 本需求只动 Mygo 黄瓜睦 |
| `SkillEffect_TriggerRain` | Skill | 移除 | Mygo 黄瓜睦当前若挂降雨，改为窝瓜逻辑 |

### 需求类型判定理由

在现有棋子 + `ISkillEffect` 框架上扩展新被动/技能流程，不新建 MonoBehaviour 运行时组件。

### 集成点

- **调用**: `MoveController.MoveToTarget`、`propertyController.TakeDamage`、`Death()`
- **触发**: 种植进场后被动定时索敌；到位后 `PlaySkill`；动画事件 → 圆形伤害结算
- **需要的新接口**: `PassiveSkillEffect_HuangguaMutsumi`（或等价命名）+ 可选 `AnimatorController` 事件转发 `OnSmashHit`

### 对现有功能的影响

- **接口变更**: 无全局接口变更
- **行为变更**: 仅 Mygo `黄瓜睦` Prefab / 技能配置
- **数据变更**: PropertyCreator 文案/数值可微调；去掉弹跳/降雨类配置

## 技术要求

- **Unity版本**: 与项目一致
- **依赖模块**: Chess、Skill、Map、ChessTeamManage
- **性能要求**: 索敌定时器间隔 ≥0.1s；圆形检测用 NonAlloc
- **兼容性要求**: 不影响 `若叶睦` / Mortis / 多首升级线
- **技能规范**: 仅 `ISkillEffect` + Chess 协程/计时器，禁止技能专用 `AddComponent`

## 功能清单

### 核心功能（必须）

- [x] 同排 ±1、±2 格索敌
- [x] 有目标时水平 `MoveToTarget` 到目标格（离格 + UnSelectable）
- [x] 到位后播下砸动画
- [x] 动画事件触发圆形范围伤害 = `GetAttack()`
- [x] 结算后自毁
- [x] Prefab 挂窝瓜被动（Mygo 黄瓜睦）

### 扩展功能（可选）

- [x] Inspector 可配：索敌检测间隔、圆形半径、移动速度覆盖
- [x] 多目标时优先最近 / 同距更靠家（x 更小）

## 验收标准

- [ ] 左右各 2 格无僵尸时：待机，不移动
- [ ] 任一格有僵尸：水平移向该目标格，到位后下砸
- [ ] 下砸帧：圆形范围内敌方均受到 ATK 伤害
- [ ] 下砸后黄瓜睦死亡并离场
- [ ] 不调用抛物线 `JumpToTarget`
- [ ] `若叶睦`（AveMujica Prefab）行为不变
- [ ] 动画事件可由用户自行绑到结算方法（`AnimatorController_Squash.UseSkill` / `OnSmashHit`）

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| Prefab 动画状态名不一致 | 中 | 中 | 可配置 skill 状态名；文档写清事件名 |
| MoveToTarget 与 standTile 不同步 | 中 | 中 | 移动前 ChessLeave；到位后设 standTile |
| 圆形半径过大误伤 | 低 | 中 | 默认半径约 1 格宽，可配 |

## 关联 Context

- 涉及模块: Chess, Skill
- 参考: `context/modules/Skill.md`、`docs/单位制作.md`
- 设计: 过度类定位（窝瓜）— [plant-deckbuilding-design.md](../game-design/plant-deckbuilding-design.md) §4 定位 **5**
