# 功能需求卡片: 章鱼祥（致盲被动）

## 基本信息
- **功能名称**: 章鱼祥 — 对齐八九寺数值 + 攻击 25% 致盲
- **所属模块**: Chess / Skill / Buff
- **需求类型**: 功能扩展
- **优先级**: P0
- **预估复杂度**: L1
- **预估耗时**: 1–2 小时
- **提出日期**: 2026-08-16

## 功能描述

### 详细描述

实现 Mygo **章鱼祥**（`章鱼祥.asset` → `Prefab/ChessPrefab/Mygo/章鱼祥`）：

1. **数值与配置**：与 **八九寺真宵** 相同（ATK 20、HP 300、价格 0、CD 7.5、射程 8.75、闪避 0.1、MainPlant 等）。
2. **战斗配置**：对齐八九寺 — `Weapon_Sample` + `StraightLaser` + `ShootBullet` + 植物 `stateGraph`。
3. **被动**：每次对敌方成功走完 `TakeDamage` 后，以 **25%** 概率对目标施加 **致盲** Buff，持续 **2 秒**；已有致盲则 **刷新时长**。
4. **致盲**：持续期间，持有者经 `TakeDamage` 打出的 **所有非治疗伤害**，伤害类型变为 `DamageType.Miss`（伤害为 0）。
5. **Miss 表现**：与闪避一致，飘字显示 **"Miss"**（尊重 `suppressFloatingDamage`）。

### 用户故事

作为玩家，我希望种下章鱼祥后，她能像八九寺一样输出，并有概率让敌人致盲导致攻击 Miss，以便用控制换取生存空间。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 八九寺真宵 PropertyCreator / Prefab | Chess | 相似 / 数据基准 | 数值与武器配置对齐 |
| `PassiveSkillEffect_TakiWang` | Skill | 相似 | `onTakeDamage` 命中后挂 Buff |
| `TimeBuff` / `ColdBuff` | Buff | 依赖 | 时长 Buff + `BuffReset` 刷新 |
| `DamageType.Miss` + `DamagePanel.ShowMiss` | Chess / UI | 依赖 | Miss 结算与飘字 |
| Mygo `章鱼祥` Prefab / asset | Chess | 被改 | 现为空壳，需接线 |

### 需求类型判定理由

在现有棋子 + `ISkillEffect` + `TimeBuff` 框架上扩展新单位被动与致盲 Buff，不新建技能专用 MonoBehaviour。

### 已确认决策

| # | 决策 |
|---|------|
| 1 | 致盲覆盖持有者全部非治疗 `TakeDamage` 输出 |
| 2 | 触发：`onTakeDamage` 后掷 25% |
| 3 | 重复挂载：刷新 2 秒 |
| 4 | Prefab 对齐八九寺武器与状态图 |
| 5 | Miss 飘字与闪避一致显示 `Miss` |

### 集成点

- **调用的现有接口**: `propertyController.onTakeDamage`、`buffController.AddBuff`、`TakeDamage` / `GetDamage`、`DamagePanel.ShowMiss`
- **触发的现有事件**: 无新全局事件
- **需要的新接口**:
  - `PassiveSkillEffect_ZhangyuXiang`（或等价命名）
  - `Buff_Blind`（致盲 TimeBuff）
  - `PropertyController.onBeforeTakeDamage`（在暴击/增伤/`GetDamage` 之前，供致盲改写 `damageType`）

### 对现有功能的影响

- **接口变更**: `PropertyController` 新增 `onBeforeTakeDamage`；`GetDamage` 对已是 `Miss` 类型时补飘字并宜早退（与 Heal 对称）
- **行为变更**: 仅章鱼祥被动；致盲持有者输出变为 Miss；喂球等本就用 Miss 的路径若未 `suppressFloatingDamage` 可能多出飘字（需核对棒球喂球）
- **数据变更**: `章鱼祥.asset` 数值对齐八九寺；Prefab 挂被动与武器

## 技术要求

- **Unity版本**: 与项目一致
- **依赖模块**: Chess、Skill、Buff、UI（DamagePanel）
- **性能要求**: 与立希汪同级（命中监听 + 概率 + AddBuff）
- **兼容性要求**: 不改八九寺行为；技能仅 `ISkillEffect`，禁止技能专用 `AddComponent`
- **平台支持**: 现有平台

## 功能清单

### 核心功能（必须）

- [x] `章鱼祥.asset` 数值/文案/MainPlant 对齐八九寺，并绑定 Prefab
- [x] Prefab：武器（StraightLaser + ShootBullet）、stateGraph、被动挂载
- [x] `PassiveSkillEffect_*`：命中后 25% 挂致盲 2s
- [x] `Buff_Blind`：`onBeforeTakeDamage` 将非治疗伤害改为 Miss
- [x] Miss 飘字与闪避一致
- [x] 重复致盲刷新时长

### 扩展功能（可选）

- [x] Inspector 可配：概率、时长
- [ ] 致盲视觉（变色/特效）— 本期不做除非已有资源

## 验收标准

- [ ] 章鱼祥 ATK/HP/价格/CD/射程/闪避与八九寺一致
- [ ] 能正常射击（同八九寺索敌/子弹路径）
- [ ] 攻击命中敌方时约 25% 挂致盲；2 秒内目标攻击飘 Miss 且无有效伤害
- [ ] 再次触发刷新 2 秒
- [ ] 治疗类伤害不受致盲改写
- [ ] 八九寺行为不变

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| `onTakeDamage` 在结算后触发，致盲改写需 before 钩子 | 中 | 中 | 新增 `onBeforeTakeDamage` |
| Miss 飘字影响喂球等 Miss 用途 | 低 | 低 | `suppressFloatingDamage` / 喂球补标记 |
| Prefab 动画/贴图未完成 | 中 | 低 | 逻辑先通；美术后补 |

## 关联 Context

- 涉及模块: Chess, Skill, Buff
- 参考文档: `context/modules/Chess.md`, `context/modules/Skill.md`, `context/modules/Buff.md`
- 依赖关系: `context/architecture/dependency-graph.md`
