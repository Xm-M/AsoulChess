# 功能需求卡片: 火爆辣椒（整行伤害 + 整行融冰）

## 基本信息
- **功能名称**: 火爆辣椒主动技能（类 PVZ Jalapeno）
- **所属模块**: 技能系统（`ISkillEffect`）+ 战斗伤害 + 地图冰面（`Effect_Snow` / `IceCell`）
- **需求类型**: 功能扩展（复用现有队伍枚举、伤害消息、冰管理）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 6–12 小时（含特效、数值与关卡验证）
- **提出日期**: 2026-03-30

## 功能描述

### 详细描述
释放技能时，对**同一行**（与施法者所在格 `mapPos.y` 一致，或见「待澄清」）产生效果：

1. **伤害**：对该行上**所有敌方单位**造成伤害（伤害类型、数值、元素等由配置决定，走现有 `DamageMessege` / `PropertyController` 链路）。
2. **融冰**：将该行上**所有已登记冰格**融化，**不分敌我**（己方 `Player` 冰与 `Enemy` 冰均移除），与冰归属无关。

### 用户故事
作为玩家，我希望用火爆辣椒清掉一整行的僵尸，并同时清掉该行冰面，以便在冰面关卡恢复种植与走位空间。

## 现有业务上下文（基于项目代码）

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 敌方枚举 | `ChessTeamManage.GetEnemyTeam(user.tag)` | 相似 | 多段技能对敌方遍历 |
| 冰登记与融化 | `Effect_Snow` + `IceCell.Melt()` | 依赖 | `ClearAllIce()` 已示范遍历融冰；可按行新增 API 或行内 `for (x)` 调 `TryGetIceCell` + `Melt()` |
| 圆形融冰 | `IceCell.MeltIceInRadius` | 参考 | 物理 Overlap；整行更适合**网格遍历**避免漏格 |
| 雪地关卡 | `GameStartPlugin_Snow` / `WeatherManage` | 依赖 | 无 `Effect_Snow` 时技能应安全 no-op |

### 需求类型判定理由
在现有 **技能效果类** 与 **全图冰字典** 上增加「按行」语义，不替换现有铺冰/单车逻辑。

### 集成点（实现草案）
- **伤害**: 遍历 `GetEnemyTeam(user.tag)`，筛选 `moveController.standTile.mapPos.y == rowY` 且存活，构造 `DamageMessege` 调用 `TakeDamage` 或等价入口（与项目现有植物技能一致）。
- **融冰**: `Effect_Snow.GetInstanceOrNull()`；对 `rowY` 上 `x ∈ [0, mapSize.x-1]` 若 `TryGetIceCell((x,rowY), out var ice)` 则 `ice.Melt()`（或封装为 `Effect_Snow.MeltRow(int mapY)` 集中维护字典一致性）。
- **注意**: `IceCell.Melt` 内会 `UnregisterIce`；需确认与 `Effect_Snow` 单例引用（`GetInstanceOrNull`）一致。

### 对现有功能的影响
- **接口变更**: 建议在 `Effect_Snow` 增加 **`MeltIceOnRow(int mapPosY)`**（或等价命名），避免技能脚本重复字典细节；属**新增 API**，旧行为不变。
- **行为变更**: 无（新技能）。
- **数据变更**: 无。

## 技术要求
- **Unity**: 与项目一致
- **依赖**: `MapManage.mapSize` / `tiles[,]`，`ChessTeamManage`，`Effect_Snow`
- **性能**: 单行格子数 ×1 次字典查找 + 融冰，可忽略
- **兼容性**: 无雪地/无 `Effect_Snow` 时仅执行伤害或跳过融冰并打日志（待统一规范）

## 功能清单

### 核心功能（必须）
- [ ] 确定行号 `rowY`（默认：施法者 `standTile.mapPos.y`）
- [ ] 对该行所有**敌方**棋子造成伤害（可配置伤害量/元素/是否爆炸等）
- [ ] 对该行**所有格**尝试融冰，**不区分**冰的 `IceOwnerTag`
- [ ] 无地图/无雪地时的降级行为明确且不抛异常

### 可选 / 待策划
- [ ] 释放表现：火焰横条特效、音效
- [ ] 是否对**无敌/隐身**敌方过滤（沿用现有伤害规则）

## 验收标准
- [ ] 单行上多个敌方均受到配置伤害（含同一格多单位若存在）
- [ ] 该行上 Player 与 Enemy 铺设的冰均被移除，种植挡板与冰层表现消失
- [ ] 其他行冰不受影响
- [ ] 非雪地关卡不报错

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| `Effect_Snow.Instance` 与 `GetInstanceOrNull` 不一致 | 低 | 中 | 融冰统一走 `GetInstanceOrNull` + 与 `IceCell.Melt` 注销路径一致 |
| 行定义与「视觉行」不一致 | 中 | 中 | 以 `mapPos.y` 为准并在文档写明 |

## 关联 Context
- `workflow_v2/context/index.md`
- 涉及模块: Chess、Skill、Map/Effect_Snow

---

## Skills 白名单检查（阶段 3 摘要）

| 技术点 | Skills |
|--------|--------|
| Animator / 技能表现 | unity-2d-animation（若需） |
| 2D 碰撞 / 冰（现有 Ice 层） | unity-2d-physics |
| 数据配置 | unity-scriptableobject-config（伤害 Scriptable） |

**结论**: 核心为 C# 逻辑与现有 API，白名单覆盖充分。

---

## 架构与影响面（阶段 4–5 摘要）

- **新增**: `SkillEffect_*` 类（如 `SkillEffect_Jalapeno`）+ 可选 `Effect_Snow.MeltIceOnRow`。
- **回归**: 冰车/初雪铺冰、全局 `ClearAllIce`；不修改 `PlaceOrRefreshIce` 语义。
- **测试建议**: 有冰无怪、有怪无冰、同行混合归属冰、空行释放。

---

## 已定稿（策划确认）
1. **伤害**：仅该行**敌方**；融冰**该行全清**（不分敌我）。
2. **行**：**施法者 `standTile.mapPos.y`**。
3. **数值**：`GetAttack() * config.baseDamage[0]`。
4. **己方**：**不对己方棋子造成伤害**。

## 实现
- `SkillEffect_Jalapeno`：`Assets/Script/Skill/ISkillEffect_Plant/SkillEffect_Jalapeno.cs`
- `Effect_Snow.MeltIceOnRow(int mapPosY)`：按行融冰

---

## 审批决策
⬜ 通过 ⬜ 修改后通过 ⬜ 驳回  

审批意见: ________________
