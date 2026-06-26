# 功能需求卡片: 肉鸽通用 HUD（RoguelikeRunInfoPanel）

## 基本信息
- **功能名称**: RoguelikeRunInfoPanel（肉鸽通用面板 / 局内 HUD）
- **所属模块**: Roguelike / UI
- **需求类型**: 功能扩展 + 体验重构
- **优先级**: P0
- **提出日期**: 2026-05-20
- **修订日期**: 2026-06-11

## 功能描述
Run 进行中常驻展示 **金币、当前 Act**；通过按钮打开子物体上的 **植物卡组只读界面**（复用 `ShopSelectIcon` + 详情区）；**设置**按钮打开 `ParsePanel` 暂停菜单。不做地图按钮（一期）。

### 用户故事
作为玩家，我希望在肉鸽地图/商店/休息界面随时看到金币与层数，并能查看本局植物卡组详情，以及打开设置/暂停。

## 预制体（手动）
1. `Resources/UIPrefab/RoguelikeRunInfoPanel.prefab`
2. 根物体挂 `RoguelikeRunInfoPanel`，name = `RoguelikeRunInfoPanel`
3. **HUD 条**：拖 `actTitleText`、`runGoldText`；按钮：打开卡组、设置
4. **子物体**：整段植物仓库 UI（`deckPanelRoot`）
   - `selectIconParent` + `shopSelectIconPre`（与 PlantsShop 同款）
   - 详情区 TMP/Image/ScrollRect（与 PlantsShop 详情区同结构）
   - 子物体上的 `ShopSelectIcon` 请勾选 **viewOnly**（只读，不加入出战栏）
5. 场景 `UIRoot` 注册该预制体

## HUD 字段

| 字段 | 一期 |
|------|------|
| `actTitleText` | ✅ 显示 |
| `runGoldText` | ✅ 显示 |
| `bandNameText` / `summaryText` | ❌ 已移除 |
| 植物图标 / `ownedPlantCountText` | ❌ 改为卡组按钮 |
| 地图按钮 | ❌ 一期不做 |
| 道具按钮 | ❌ 二期 |
| `settingsButton` → `ParsePanel` | ✅ |

## 集成点
- 订阅 `RoguelikeRunService` 事件自动刷新
- `RoguelikeMapPanel` / Shop / Rest 打开时 `TryShowAndRefresh()`
- 放弃/通关 `HideForRunEnded()`

## 验收标准
- [ ] Run 开始后 HUD 显示金币与 Act
- [ ] 点「卡组」打开子面板，列表为本局 `ownedPlantCreatorIds`，点击卡片显示详情
- [ ] 卡组内不可选入出战栏（`ShopSelectIcon.viewOnly`）
- [ ] 点「设置」打开暂停面板（`ParsePanel.Show` + `ShowMenuPanel`）
- [ ] 选路/战斗返回后数据更新
- [ ] 放弃或通关后 HUD 与卡组层隐藏

## 关联
- `docs/requirements/roguelike-run-save.md`
- `PlantsShop` / `ShopSelectIcon`（只读模式）
