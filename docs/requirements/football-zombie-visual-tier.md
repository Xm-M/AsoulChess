# 功能需求卡片: 橄榄球僵尸 VisualTier 一体外观

## 基本信息
- **功能名称**: 橄榄球僵尸 VisualTier 一体外观
- **所属模块**: Chess / Armor / Animator
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2–4 小时
- **提出日期**: 2026-08-12
- **需求 ID**: football-zombie-visual-tier

## 功能描述
### 详细描述
橄榄球僵尸改用 6 档一体贴图（头盔完整→破损→脱落→断手→断头），全部由 Animator `VisualTier` 驱动，不再用独立帽子 `SpriteRenderer` 叠层（解决重叠排序问题）。

头盔阶段由新护甲 `HeadArmor_VisualTier` 写入档位；本体血量阶段仍由 `AnimatorController_SampleZombie` 写入。护甲未破时 SampleZombie **禁止**按血量覆盖 VisualTier（对齐 IceCarArmor 门禁；爆炸会扣甲且仍打本体，故不能只靠 `damage<=0`）。

### 用户故事
作为玩家，我希望橄榄球僵尸受伤外观随头盔/身体状态一体切换且重叠时排序正确，以便战场可读。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| HeadArmor | Armor | 相似 | 路障帽：分体 Sprite 三态；本需求不改它 |
| IceCarArmor | Armor | 相似 | 护甲驱动 VisualTier + SampleZombie Skip |
| AnimatorController_SampleZombie | Chess | 依赖/扩展 | 血量→0/1/2；需扩展 Skip 条件 |
| 橄榄球僵尸 Prefab | Chess | 被改 | 换 AnimatorController + 挂新护甲 + Blend Tree |

### 需求类型判定理由
在现有护甲承伤 + SampleZombie VisualTier 管线上扩展「头盔档位护甲」，属功能扩展；不新开战斗框架。

### 集成点
- **调用的现有接口**: `ArmorBase` / `onSetDamage`、`AnimatorController.SetVisualTierPublic`、`ObjectPool.Create`（掉落特效）
- **触发的现有事件**: `WhenEnterGame` → `ResetArmor`；破甲 `OnArmorBroken`
- **需要的新接口**: `HeadArmor_VisualTier.IsBroken`（供 SampleZombie 查询）

### 对现有功能的影响
- **接口变更**: SampleZombie Skip 条件增加未破的 `HeadArmor_VisualTier`
- **行为变更**: 仅挂该护甲的单位（橄榄球）头盔期不受血量档覆盖；路障 `HeadArmor` 不变
- **数据变更**: 无存档格式变更

## 已锁定决策
| # | 结论 |
|---|------|
| 1 | VisualTier：3 帽满 / 4 帽损1 / 5 帽损2 / 0 无帽完好 / 1 断手 / 2 断头 |
| 2 | 新建 `HeadArmor_VisualTier`，不改路障 `HeadArmor` |
| 3 | 护甲未破：SampleZombie **Skip HP Sync**（含入场、爆炸打本体）；破甲后写 0，再交回血量同步 |
| 4 | 不用「damage<=0 不改档」作唯一门禁（`damage==0` 时本就不会进 `OnGetDamage`；爆炸仍会进） |
| 5 | Prefab：SampleZombie + 新护甲 + Animator Blend Tree 覆盖 0–5 |

## 技术要求
- **依赖模块**: Chess、Armor、Animator、对象池（掉落 FX）
- **兼容性要求**: 向后兼容；无 `HeadArmor_VisualTier` 的僵尸行为不变
- **平台支持**: 现有目标平台

## 功能清单
### 核心功能（必须）
- [x] `HeadArmor_VisualTier`：承伤逻辑对齐 HeadArmor；写 3→4→5；破甲→0 + 掉落 FX
- [x] `AnimatorController_SampleZombie`：IceCar 或未破 VisualTier 护甲时 Skip HP Sync
- [x] 橄榄球僵尸 Prefab / Animator 接线（Blend Tree 0–5 需在 Editor 配美术序列）

### 扩展功能（可选）
- [ ] 抽公共 `ShouldSkipHpVisualTierSync()` 辅助，减少重复 GetComponent

## 验收标准
- [ ] 头盔期仅 3/4/5，普通弹与爆炸打本体均不跳到 0/1/2
- [ ] 破甲后为 0，再按血量进 1/2
- [ ] 路障僵尸 / 普通僵尸 / 冰车外观行为无回归
- [ ] 重叠时无独立帽子层排序错乱

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| Animator 0–5 Blend Tree 未配全 | 中 | 高 | Prefab/Controller 接线列入 P0；缺 clip 时先占位 |
| 爆炸+破甲同帧档位竞态 | 低 | 中 | 破甲在 onSetDamage 内先写 0，再放行本体伤害 |

## 关联 Context
- 涉及模块: Chess
- 参考文档: `context/modules/Chess.md`、`context/index.md`
