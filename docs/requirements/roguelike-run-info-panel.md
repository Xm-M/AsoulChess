# 功能需求卡片: 肉鸽局内数据面板

## 基本信息
- **功能名称**: RoguelikeRunInfoPanel（局内数据 HUD）
- **所属模块**: Roguelike / UI
- **需求类型**: 功能扩展
- **优先级**: P0
- **提出日期**: 2026-05-20

## 功能描述
Run 进行中展示本局关键数据：乐队、Act、当前/待进入节点、地图进度、种子、已拥有植物列表等。布局由策划/美术在预制体中完成，脚本提供可拖拽绑定槽位。

### 用户故事
作为玩家，我希望在肉鸽 Run 中随时看到当前局内状态，以便掌握进度与卡组。

## 现有业务上下文

| 功能 | 关系 |
|------|------|
| `RoguelikeRunState` | 数据源 |
| `RoguelikeRunService` | 事件驱动刷新 |
| `RoguelikeMapPanel` | 打开地图时联动显示 |
| `RoguelikeRunPlantPool` | 解析植物图标 |

## 集成点
- 订阅 `OnRunStarted` / `OnActMapGenerated` / `OnNodeEntered` / `OnNodeResolved` / `OnRunCompleted`
- `RoguelikeMapPanel.ShowAndRefresh()` → `TryShowAndRefresh()`
- 放弃/通关 → `HideForRunEnded()`

## 预制体（手动）
1. `Resources/UIPrefab/RoguelikeRunInfoPanel.prefab`
2. 根物体挂 `RoguelikeRunInfoPanel`，name = `RoguelikeRunInfoPanel`
3. 按需拖 TMP / Image / Button；不用的字段留空即可

## 可绑定字段
| 字段 | 内容 |
|------|------|
| bandNameText | 所选乐队 |
| runConfigNameText | RunMapConfig 资产名 |
| actTitleText | 当前 Act |
| runSeedText | 种子 |
| currentNodeText | 当前节点 |
| pendingNodeText | 待战斗节点 |
| selectableNextText | 可选下一层 |
| progressText | 抵达/通关/地图节点数 |
| ownedPlantCountText | 植物数量 |
| summaryText | 多行摘要 |
| plantIconImages | 固定槽位图标 |
| plantIconRoot + plantIconPrefab | 动态生成图标 |

## 验收标准
- [ ] Run 开始后面板自动显示并刷新
- [ ] 选路/战斗返回后数据更新
- [ ] 放弃或通关后隐藏
- [ ] 未配置的 TMP 不影响运行

## 后续扩展（P1）
- Run 内金币、遗物字段加入 `RoguelikeRunState` 后在本面板增加对应 TMP 绑定
- 战斗场景若需 HUD，将同一预制体注册到该场景 UIRoot
