# 功能需求卡片: 战斗框架竖切（Board + Entity 加厚）

## 基本信息
- **功能名称**: 数值–战斗–移动框架竖切（可空项目演示）
- **所属模块**: `Packages/com.asoulchess.game.board` + 加厚 `com.asoulchess.game.entity`
- **需求类型**: 新增功能（工程基建）+ 功能扩展（Entity）
- **优先级**: P1
- **预估复杂度**: A 级
- **预估耗时**: 5～10 天
- **提出日期**: 2026-07-28
- **上游**: Core / Entity 包；规划 `reusable-systems-packaging`

## 功能描述

### 详细描述

在**不迁移 AVZ 玩法**的前提下，把「数值–战斗–移动」等**框架**补到可在新项目里搭出「格子上单位互殴」的程度：

1. **新建 Board 包**：格子棋盘、占位进入/离开、邻格查询；无冰格/门等 AVZ 特化规则  
2. **加厚 Entity 包**：  
   - 更完整的状态骨架（至少 Idle / Attack / Dead，可含 Skill）  
   - 攻击框架（冷却、选目标接口、造成伤害；非具体武器内容）  
   - 移动框架（依赖 Board 占格；非特殊寻路内容）  
   - 动画桥接口（PlayIdle/Attack/Skill/Death 空实现或日志实现）  
3. **Sample**：空项目可引用 Core+Entity+Board，跑通「两单位占格、靠近、攻击掉血、死亡」  

策略：精简重写进包；**不**剪切粘贴 AVZ Chess/Map。内容（具体棋子/技能/关卡）不进包。

### 用户故事

作为开发者，我希望引用一套完整的战斗框架积木（格子+单位+攻移状态），以便在新项目里配置单位并跑通最小战斗，而不拷贝 AVZ 内容。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| Core UPM | Packages | 依赖 | 事件/计时/池 |
| Entity UPM 0.1 | Packages | 扩展 | 已有薄实体；本需求加厚 |
| Map / Tile | AVZ Map | 参考 | 抽象进 Board，不搬具体规则 |
| AttackController / Move / State / Animator | AVZ Chess | 参考 | 只抽接口与最小实现 |
| AVZ Chess | Chess | 本期不触碰 | 选项 A |

### 用户已确认选型

| 项 | 选择 |
|----|------|
| 完整度 | **1** 竖切可玩 Demo |
| 落点 | **1** 继续 AVZ 仓库 Packages |
| AVZ | **A** 不迁 |

### 集成点

- Board ← Core（可选事件）  
- Entity ← Core；Move/占位 → Board  
- Sample 引用三包；AVZ `Assets/Script` 零改动  

### 对现有功能的影响

- **本期**: 仅新增/修改 Packages；玩法脚本不变  
- **Entity 破坏性**: 可能扩展 `GameEntity` API；保持 0.1 Sample 可修到仍能跑  

## 技术要求

- **Unity**: 2022.3+  
- **包名建议**: `com.asoulchess.game.board`；Entity 升至 `0.2.0`  
- **依赖**: Board→Core；Entity→Core，Entity 可选引用 Board（asmdef）  
- **兼容**: AVZ 不迁；空项目三包引用可编译  
- **内容黑名单**: 无具体技能效果、无 AVZ EventName、无 GameManage  

## 功能清单

### 核心功能（必须 · P0 竖切）

- [x] Board：`IBoard` / `ITile` / 占位 Enter/Leave / 获取邻格或范围内格  
- [x] Entity：`MoveController`（移到邻格或指定格，更新占位）  
- [x] Entity：`AttackController`（冷却、对目标 `ApplyDamage`）  
- [x] Entity：状态至少 Idle/Attack/Dead 与攻击流程衔接  
- [x] Entity：`IAnimBridge`（可空实现）  
- [x] Sample：生成棋盘 + 两实体 → 移动接近 → 攻击 → 死亡日志  
- [x] README：三包如何引用、Demo 步骤、框架≠内容  

### 扩展（本期可不做）

- [ ] 完整 Transition 图编辑器 / SO 状态图  
- [ ] A* 寻路  
- [ ] 具体 Weapon 库  
- [ ] AVZ 迁移  

## 验收标准

- [ ] 空项目 Add from disk 三包后，Import Sample，Play 可完成互殴流程  
- [ ] asmdef 无引用 AVZ `Assets/Script`  
- [ ] AVZ 原玩法工程可照常打开（未改玩法脚本）  
- [ ] 文档写清：新项目如何挂 Board + 配置单位攻击/移动  

## 风险评估

| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| 范围膨胀成半个 AVZ | 高 | 高 | 严格竖切清单；A* / 特殊移动不做 |
| Entity 与 Board 循环依赖 | 中 | 中 | Move 依赖 Board；Board 不依赖 Entity（占位用接口/弱类型） |
| 工期超时 | 中 | 中 | P0 先交付 Sample，再磨 API |

## 关联 Context

- `context/modules/Chess.md`, `Map.md`, `State.md`  
- `Packages/com.asoulchess.game.core`, `...entity`  
- `docs/architecture/reusable-systems-packaging-plan.md`  

## 产出物

- `Packages/com.asoulchess.game.board/`  
- `Packages/com.asoulchess.game.entity/`（0.2）  
- `docs/requirements/combat-framework-vertical-slice.md`（本文）  
- 架构/审批文档  
