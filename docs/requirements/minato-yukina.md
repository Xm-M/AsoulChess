# 功能需求卡片: 凑友希那

## 基本信息
- **功能名称**: 凑友希那
- **所属模块**: Chess / Skill / Weapon / Buff（压力）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2–4 小时（含 Prefab 基本配置；动画/标签由策划自填）
- **提出日期**: 2026-07-13

## 功能描述
### 详细描述
新增植物棋子「凑友希那」：自身为**远程射击**。被动：每次**攻击时**，对自身格子**上下左右四邻格**内的**友方**棋子施加「压力」Buff（无则挂上；已有则通过同名 Buff 再施加走 `BuffReset`，每次默认 `extraStress = 1`）。自身**不挂**压力 Buff。

### 用户故事
作为玩家，我希望上场凑友希那远程输出的同时，每次开火给邻格队友叠压力，以便与 GBC/老仓育等多压力体系联动。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Buff_StressBuff_Death` + `BuffReset` | Buff | 依赖 | 同名「压力」再 `AddBuff` → `extraStress` 累加 |
| `OkuwakiStressSpread.ApplyStressDelta` | Skill | 可复用 | Ensure + BuffReset；邻格现为八向，友希那改四向 |
| 棒球手 `OnAttack` 被动 | Skill | 相似 | `equipWeapon.OnAttack` 挂钩时机 |
| 今井莉莎等邦多利远程 | Chess | 相似 | `ShootBullet` + 索敌配置可参考 |
| 占位 Prefab / PropertyCreator | 资源 | 扩展 | `邦多利/凑友希那`、`ChessData/Player/凑友希那.asset` 已存在 |

### 需求类型判定理由
全新棋子与被动效果；压力与射击均复用现有协议，不改全局压力规则。

### 压力叠加说明（已确认）
**是的**：对已有「压力」Buff 的目标再次 `AddBuff(同名压力 Buff)` 时，`BuffController` 会调用 `BuffReset`，`Buff_StressBuff_Death` 内执行 `stress += extraStress` 并写回 context `"stress"`。  
因此被动实现即为：每次攻击对邻格友方 `AddBuff(压力模板 Clone)`；模板上配置 `extraStress = 1` 即可每次 +1。无需另写一套加压力 API（可直接复用 / 薄封装 `ApplyStressDelta` 逻辑）。

### 集成点
- **调用的现有接口**: `equipWeapon.OnAttack`；`moveController.standTile`；`MapManage.IfInMapRange` / `tiles[x,y]`；`buffController.AddBuff`；`ShootBullet`
- **触发的现有事件**: 攻击动画/武器触发的 `OnAttack`
- **需要的新接口**: `PassiveSkillEffect_Yukina`（名可定）— 四邻格友方加压；可选小工具 `CollectNeighbor4Tiles`

### 对现有功能的影响
- **接口变更**: 无破坏性变更
- **行为变更**: 无；仅新棋子入场后影响邻格友方压力
- **数据变更**: 配置 `凑友希那.asset` + Prefab 被动/武器；`plantTags` 由用户自填

## 技术要求
- **依赖模块**: Skill、Buff、Chess、Weapon、Map
- **性能要求**: 每次攻击最多扫描 4 格；邻格棋子数量通常很少
- **兼容性要求**: 向后兼容；不影响现有压力 Buff 子类（MMK/Rupa 等仍走各自 `BuffReset`）
- **规范**: `ISkillEffect`，禁止技能专用运行时 `AddComponent` MonoBehaviour；离场卸 `OnAttack`

## 已确认规则

| 项 | 结论 |
|----|------|
| 触发时机 | 攻击时（`OnAttack`） |
| 范围 | 上下左右四格；越界/空格跳过；不含自身 |
| 对象 | **仅友方**（同 tag） |
| 压力行为 | 无则挂「压力」；有则再施加 → `BuffReset`，默认每次 +1 |
| 自身压力 | **不挂** |
| 攻击形态 | 远程射击（`ShootBullet`） |
| 标签 / 文案 / 动画 | 用户自填；开发只配被动与射击骨架 |

## 功能清单
### 核心功能（必须）
- [x] `PassiveSkillEffect_*`：`OnAttack` → 四邻格友方 `AddBuff(压力)`
- [x] 压力模板可配置（`extraStress` 默认 1；`stressLimit` 等可配）
- [x] 远程武器：`Weapon_Sample` + `ShootBullet` + 索敌（直线或格子，Prefab 配）
- [x] PropertyCreator / Prefab 基本接线（标签留空或占位，用户自填）
- [x] `OnRemove` 卸载监听

### 扩展功能（可选）
- [ ] 抽取共用 `CollectNeighbor4Tiles`（若多处需要）
- [ ] 子弹 Prefab 独立资源（可先复用现有弹）

## 验收标准
- [ ] 凑友希那能远程射击
- [ ] 每次攻击仅给上下左右邻格**友方**加压；敌方与自身不受影响
- [ ] 友方首次获得「压力」Buff；再次被施加时压力数值 +1（默认）
- [ ] 离场后不再给邻居加压
- [ ] 代码通过审查；无技能专用运行时 MonoBehaviour

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 邻格友方已是 MMK/Nina 等子类压力 Buff | 中 | 低 | 同名走子类 `BuffReset`，一般仍累加；测一轮即可 |
| `OnAttack` 触发次数与动画事件不一致 | 低 | 中 | 对齐莉莎/棒球手已有挂钩方式 |
| Prefab 占位无弹道资源 | 中 | 低 | 先复用现有子弹 Prefab |

## 关联 Context
- 涉及模块: Skill, Buff, Chess
- 参考文档: `context/modules/Skill.md`, `context/modules/Buff.md`
- 依赖关系: `context/architecture/dependency-graph.md`
