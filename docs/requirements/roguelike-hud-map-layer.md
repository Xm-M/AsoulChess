# 功能需求卡片: 肉鸽 HUD 地图层数 + 战斗进度条标题（RG-007 / RG-005）

## 基本信息
- **功能名称**: 地图层数上 HUD；战斗 ProgressBar 肉鸽标题
- **所属模块**: Roguelike / UI
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **提出日期**: 2026-05-20
- **关联**: RG-007（通用 HUD 补层数）、RG-005（战斗场景 Run 上下文，采用 ProgressBar 扩展而非整板 HUD）

## 功能描述

### RG-007 · RoguelikeRunInfoPanel 地图层数

在现有 HUD 条（金币、卡组、小推车、携带格）旁增加 **地图层数** 一项：

- 图标 + `TMP_Text`（与现有 stat 一致）
- 文案：`L{n}`，`n = CurrentNode.layer + 1`；无节点时显示 `—`
- 支持悬停 tooltip（`RoguelikeRunInfoHudStatKind.MapLayer`）

### RG-005 · ProgressBar 肉鸽标题

战斗关 `ProgressBar.stadgeName` 原仅显示 `LevelData.levelName`。

肉鸽战斗关（`roguelikeKind != None` 且 active Run）改为：

```text
{Act 显示名} · L{层数} · {关卡名}
```

例：`前院 · L3 · _ring_01`

非肉鸽关卡 **行为不变**，仍只显示 `levelName`。

层数取自当前战斗对应地图节点（优先 `PendingNodeId`，否则 `currentNodeId`）。

## 验收标准

- [x] 代码：HUD 层数字段 + Formatter + tooltip
- [x] 代码：`ProgressBar` 肉鸽标题 `Act · Ln · 关卡名`
- [x] Prefab：`runMapLayerText` / `mapLayerStatIconRoot` 已绑
- [x] Play：地图 HUD 显示 Ln；肉鸽战斗 ProgressBar 标题正确

## 关联 Context

- `RoguelikeRunInfoPanel.cs`、`ProgressBar.cs`、`RoguelikeRunInfoFormatter.cs`
