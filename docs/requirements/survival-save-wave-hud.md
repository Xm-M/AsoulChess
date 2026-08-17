# 功能需求卡片: 生存模式存档修复 + 波次 HUD

## 基本信息
- **功能名称**: 生存模式存档修复与 ProgressBar 波次显示
- **所属模块**: LevelSystem / SaveSystem / UI(ProgressBar) / PlantsShop 插件
- **需求类型**: Bug修复 + 功能扩展
- **优先级**: P0
- **预估复杂度**: L2
- **预估耗时**: 2–3 小时
- **提出日期**: 2026-08-14
- **需求 ID**: `survival-save-wave-hud`

## 功能描述
### 详细描述
1. **重新开始**：删除关卡存档的同时清空选卡内存缓存（`lockedHand`），真正从空白选卡开局。
2. **重新加载**：任意时机（战斗中 / 轮间选卡）读档后，必须恢复场上植物；选卡可按存档预勾，但草坪植物不得丢失。
3. **ProgressBar 标题**：生存模式显示 `关卡名 · 第X/Y波`（本轮内波次，Y=本轮总波数）。

### 用户故事
- 作为玩家，重新开始时我希望选卡与进度都被清空，以免上一局残留。
- 作为玩家，读档后我希望草坪上的植物还在，以便继续构筑。
- 作为玩家，我希望进度条旁能看到当前波次，以便掌握本轮进度。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `SaveSystem` | SaveSystem | 依赖 | 按 `levelName` 存 JSON；`IfGameStart` 门闸 |
| `PreParePlugun_ShowPlantShop` | LevelSystem | 修改 | `lockedHand` / Prefill / CaptureTo |
| `LevelController_Endless` | LevelSystem | 修改 | EnterMap 恢复植物；轮末存档；`RunRoundEnter` |
| `ProgressBar` | UI | 扩展 | `stadgeName`（肉鸽已有格式化） |
| `LevelManage.RestartLevel` | LevelSystem | 修改 | DeleteSave 未清插件缓存 |

### 需求类型判定理由
存档与重开行为错误属 **Bug修复**；波次标题属在现有 ProgressBar 上的 **功能扩展**。不新建存档框架，沿用 `SaveSystem` + 生存 Controller。

### 集成点
- **调用的现有接口**: `SaveSystem.DeleteSave` / `SaveCurrentLevel` / `LoadSaveData`；`RestorePlayerPlants`；`UIManage` + `ProgressBar.Show` / `MoveBar`
- **触发的现有事件**: `WhenLeaveLevel`（离开关卡清上下文）
- **需要的新接口**: `ClearLockedHand()`（或等价）挂在选卡插件；可选生存专用 ProgressBar 标题格式化

### 对现有功能的影响
- **接口变更**: 选卡插件增加清空缓存 API；Restart 调用之
- **行为变更**: 重开不再 Prefill 上局卡；读档保证种回植物；生存标题带波次
- **数据变更**: 无存档 schema 版本变更（沿用 `playerPlants` / `plantsShopData`）；可能补强「轮间也可落盘」策略

## 技术要求
- **Unity版本**: 项目现用版本
- **依赖模块**: LevelSystem, SaveSystem, UI
- **性能要求**: 与现有存档同级，无额外每帧开销（标题仅在 Show/进波时刷新）
- **兼容性要求**: 向后兼容旧存档 JSON；冒险 / 肉鸽 ProgressBar 行为不变
- **平台支持**: 与现有存档一致（本地文件）

## 功能清单
### 核心功能（必须）
- [x] 重新开始：删档 + 清空 `lockedHand` + 不读旧 `SaveLoadContext`
- [x] 读档：`EnterMap` 可靠 `RestorePlayerPlants`；轮间退出再进仍有植物
- [x] 轮间 `IfGameStart=false` 时：生存离场补存快照
- [x] 修复读档后 `RunRoundEnter` 冲掉波次进度（战斗中保留；轮末存 `-1`）
- [x] 生存读档正常播 Timeline（不走 loadSkipToTime）；轮末存档；阳光/手牌保留；场上有植物不重种

### 扩展功能（可选）
- [ ] 读档后 Continue 面板与冒险对齐（非本需求必须）

## 验收标准
- [ ] 生存局种植 → 重新开始 → 选卡为空、草坪为空、阳光为初始
- [ ] 生存局种植 → 轮间选卡退出再进 → 草坪植物与存档一致
- [ ] 生存局战斗中退出再进 → 草坪植物恢复
- [ ] 生存 ProgressBar 标题为 `关卡名 · 第X/Y波`；冒险/肉鸽标题不受影响
- [ ] 无存档 schema 破坏；旧档可加载

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 轮间补存放宽 `IfGameStart` 门闸误伤冒险 | 中 | 高 | 仅 SurvivalMode 分支放宽或单独 API |
| `lockedHand` 清不干净（多份 LevelData） | 低 | 中 | Restart/无档进关统一 Clear |
| 读档种植物时机与地图未就绪 | 中 | 高 | 对齐地图就绪或延迟一帧再 Restore |

## 关联 Context
- 涉及模块: LevelSystem, UI, SaveSystem（见 `context/modules/LevelSystem.md`、`UI.md`）
- 参考文档: `docs/requirements/survival-endless-mode.md`
- 依赖关系: `context/architecture/dependency-graph.md`
