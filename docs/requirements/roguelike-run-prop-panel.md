# 功能需求卡片: RG-007 Run 道具栏（查看已拥有道具）

## 基本信息
- **功能名称**: 肉鸽 Run 道具查看面板
- **所属模块**: Roguelike / UI / Prop
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 3–5 小时（含 Prefab 接线）
- **提出日期**: 2026-08-12
- **需求 ID**: roguelike-run-prop-panel
- **关联**: RG-007 剩余项；RG-003 领取链路已手测通过

## 功能描述
### 详细描述
在地图 / 商店 / 休息使用的 `RoguelikeRunInfoHud` 上，增加与「植物卡组」对等的 **已拥有道具** 入口与弹层：左侧道具卡列表，右侧详情（名称 + **稀有度** + 效果说明）。数据直接读本 Run `ownedPropIds`。

### 用户故事
作为肉鸽玩家，我希望在地图侧查看本 Run 已拿到的道具及效果说明，以便确认领取结果并为下一场战斗做构筑预期。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `RoguelikeRunInfoHud` 植物卡组弹层 | Roguelike UI | 相似/模板 | `openDeckButton` → `deckPanelRoot` → 左 `ShopSelectIcon` + 右 `ShowPlantDetail` |
| `RoguelikeRunPropPool` | Roguelike | 依赖 | `ResolveProps(ownedPropIds)` |
| `PropItemData` | Prop | 依赖 | `displayName` / `icon` / `effectDescription` |
| `PropIcon` / 战斗 `PropPanel` | Prop UI | 可复用图标 | 战斗栏；本需求不做战斗 HUD 整板 |
| Codex 左列表右详情 | UI | 交互参考 | 布局语义类似，不强制复用 Codex 脚本 |

### 需求类型判定理由
在已有 Run HUD 上补 RG-007 未完成的道具栏；不新建遗物框架（与 Prop 同一套）。

### 集成点
- **调用**: `RoguelikeRunService.State.ownedPropIds` → `RoguelikeRunPropPool.ResolveProps`
- **触发**: HUD 按钮打开/关闭；领取道具后若弹层打开可刷新（或下次打开再刷）
- **不改**: 战斗内 `PropPanel` 逻辑（进战仍靠 `ApplyRunPropsToGame` + `GameStartPlugin_Prop`）

### 对现有功能的影响
- **行为变更**: HUD 多一个「道具」入口与弹层
- **数据变更**: `PropItemData` 新增四档稀有度字段（与植物 `Property.rarity` 刷怪权重无关）；存档仍只存 propId

## 已锁定决策
| # | 结论 |
|---|------|
| 1 | **仅**地图 / 商店 / 休息的 `RoguelikeRunInfoHud` 显示入口与弹层（不做战斗整板道具栏） |
| 2 | 交互对标植物卡组：专门弹层；左列表右详情 |
| 3 | 详情：**名称** + **稀有度** + **效果说明**；图标展示 |
| 4 | 数据：**直接读** `ownedPropIds` / `ResolveProps` |
| 5 | 稀有度：**需要**。`PropItemData` 增加四档枚举，UI 显示；钉耙/闪电先配占位档 |

### 稀有度四档（已锁定）
| 枚举 | 显示名 |
|------|--------|
| `Common` | 普通 |
| `Uncommon` | 稀有 |
| `Rare` | 史诗 |
| `Legendary` | 传说 |

（与植物 `int rarity` 刷怪权重分离，新建 `PropRarity`。）

占位：钉耙=普通，闪电=稀有。

## 功能清单
### 核心功能（必须）
- [x] `PropRarity` 四档 + `PropItemData.rarity`；钉耙/闪电填占位
- [x] HUD：`openPropsButton`（与卡组共用弹层骨架）
- [x] `RefreshPropsList`：按 `ownedPropIds` 生成左侧道具卡
- [x] 选中后右侧刷新名称、稀有度、图标、效果说明
- [x] 空列表友好提示（无道具时）
- [x] Prefab 接线（地图 HUD）

### 扩展（可选）
- [ ] 领取瞬间若面板已开则自动 Refresh
- [ ] 左侧卡边框按稀有度变色

## 验收标准
- [ ] Elite/Boss 领取道具后，回地图打开面板能看到对应道具
- [ ] 点击左侧项，右侧名称、稀有度、效果正确
- [ ] 商店 / 休息 HUD 同源可见（与地图同一 Prefab/脚本）
- [ ] 战斗场景不出现该整板；进战效果仍由现有 Prop 管线负责
- [ ] 植物卡组行为无回归

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| Prefab 未预留道具按钮位 | 中 | 中 | 复制卡组按钮/弹层改绑 |
| `allProps` 缺图标/文案 | 低 | 低 | 钉耙/闪电补全 displayName/effectDescription |

## 关联 Context
- Roguelike UI、`docs/roguelike-known-issues.md` RG-007
- `docs/requirements/prop-system.md`
